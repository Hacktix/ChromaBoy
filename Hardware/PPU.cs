using ChromaBoy.Software;
using ChromaBoy.Software.Graphics;
using System;
using System.Collections.Generic;

namespace ChromaBoy.Hardware
{
    public class PPU
    {
        private Gameboy parent;

        // # Cycle Counters
        private int scanlineCycles = 1;

        // # Mode Variables
        private byte mode = 2;

        // * OAM Search
        private byte oamScanOffset = 0;
        private bool oamCooldown = false;
        private List<ObjectSprite> oamBuf = new List<ObjectSprite>(10);

        // * Drawing
        private byte ly = 0;
        private byte lx = 0;
        private byte scrollShifted = 0;
        private byte wly = 0;

        // - Pixel Fetcher
        private bool drawingWindow = false;
        private byte fetcherState = 0;
        private byte fetchOffset = 0;
        private byte tileNumber = 0;
        private ushort tileData = 0;
        private Queue<FifoPixel> pixelFifo = new Queue<FifoPixel>();

        // - LCD Shifter
        private bool shiftingPaused = false;

        // # LCD Buffers
        public byte[,] LCDBuf1 = new byte[Emulator.SCREEN_WIDTH, Emulator.SCREEN_HEIGHT];
        public byte[,] LCDBuf2 = new byte[Emulator.SCREEN_WIDTH, Emulator.SCREEN_HEIGHT];
        public bool HasUpdated = false;

        // # STAT Interrupts
        private bool lastStat = false;

        // # DMA Transfer
        public byte CurrentByte = 0;
        private byte dmaCooldown = 0;
        private bool dmaInit = false;
        private ushort dmaOffset = 0;

        public PPU(Gameboy parent)
        {
            this.parent = parent;
        }

        public void ProcessCycle()
        {
            // Handle OAM DMA
            ProcessDMA();

            // Handle operations based on mode
            switch(mode)
            {
                case 2: ProcessOAMCycle(); break;
                case 3: ProcessDrawingCycle(); break;
            }
            
            if(++scanlineCycles == 457) // If end of scanline has been reached
            {
                ++ly;
                if(drawingWindow)
                {
                    ++wly;
                    drawingWindow = false;
                }
                parent.Memory.Set(0xFF44, ly);
                scanlineCycles = 1;
                if (ly < 144) // Enter new scanline to draw
                    ChangeMode(2);
                else if (ly < 154 && mode != 1) // Enter VBlank mode
                    ChangeMode(1);
                else if(ly == 154) // Reset registers for new frame
                    StartNewFrame();
            }
        }

        #region OAM Search
        private void ProcessOAMCycle()
        {
            if(oamCooldown) // Pause every second cycle
            {
                oamCooldown = false;
                if(scanlineCycles == 80) // If end of OAM scan is reached
                {
                    oamScanOffset = 0;
                    ChangeMode(3);
                }
                return;
            }

            if(oamBuf.Count < 10) // Only load up to 10 sprites per scanline
            {
                ObjectSprite obj = new ObjectSprite(parent, (ushort)(0xFE00 + 4 * oamScanOffset++));
                if (obj.X > 0 && (ly + 16) >= obj.Y && (ly + 16) < (obj.Y + ((parent.Memory.Get(0xFF40) & 0b100) == 0 ? 8 : 16)))
                    oamBuf.Add(obj);
            }

            oamCooldown = true;
        }
        #endregion

        #region Drawing
        private void ProcessDrawingCycle()
        {
            FetchBackgroundPixels(); // Tick background pixel fetcher

            if (pixelFifo.Count > 0 && !shiftingPaused)
            {
                if (scrollShifted != (parent.Memory.Get(0xFF43) % 8))
                {
                    scrollShifted++;
                    pixelFifo.Dequeue();
                }
                else
                {
                    FifoPixel pixel = pixelFifo.Dequeue();
                    byte color = (byte)((parent.Memory.Get(0xFF47) & (0b11 << (2 * pixel.PixelData))) >> (2 * pixel.PixelData));
                    LCDBuf1[lx++, ly] = (byte)(color + 1);

                    if((parent.Memory.Get(0xFF40) & 0b100000) != 0 && ly >= parent.Memory.Get(0xFF4A) && (lx + 7 == parent.Memory.Get(0xFF4B) || parent.Memory.Get(0xFF4B) < 7))
                    {
                        pixelFifo.Clear();
                        fetcherState = 0;
                        fetchOffset = 0;
                        drawingWindow = true;
                    }

                    if (lx == 160) // Clear FIFO, reset fetcher and enter HBlank if end of screen reached
                    {
                        ChangeMode(0);
                        lx = 0;
                        pixelFifo.Clear();
                        oamBuf.Clear();
                        fetcherState = 0;
                        fetchOffset = 0;
                        scrollShifted = 0;
                    }
                }
            }
        }

        private void FetchBackgroundPixels()
        {
            if(fetcherState % 2 != 0) // Clock fetcher at CPU Clock / 2
            {
                fetcherState++;
                return;
            }

            switch(fetcherState)
            {
                case 0: // Fetch tile number
                    ushort baseTileAddr = 0x9800;
                    if ((parent.Memory.Get(0xFF40) & 0b1000) != 0 && !drawingWindow)
                        baseTileAddr = 0x9C00;
                    else if ((parent.Memory.Get(0xFF40) & 0b1000000) != 0 && drawingWindow)
                        baseTileAddr = 0x9C00;
                    int addressOffset = drawingWindow ? (fetchOffset++ + 32 * (wly / 8)) : (((fetchOffset++ + parent.Memory.Get(0xFF43) / 8) % 32) + 32 * (((ly + parent.Memory.Get(0xFF42)) / 8) % 32));
                    tileNumber = parent.Memory.Get(baseTileAddr + addressOffset);
                    break;
                case 2: // Fetch Tile Data Low
                    ushort lowDataAddr = (ushort)((parent.Memory.Get(0xFF40) & 0b10000) == 0 ? 0x9000 : 0x8000);
                    lowDataAddr = (ushort)(lowDataAddr + (lowDataAddr == 0x8000 ? tileNumber : (int)(sbyte)tileNumber) * 16 + 2 * (drawingWindow ? (wly % 8) : ((ly + parent.Memory.Get(0xFF42)) % 8)));
                    tileData = parent.Memory.Get(lowDataAddr);
                    break;
                case 4: // Fetch Tile Data High
                    ushort highDataAddr = (ushort)((parent.Memory.Get(0xFF40) & 0b10000) == 0 ? 0x9000 : 0x8000);
                    highDataAddr = (ushort)(highDataAddr + (highDataAddr == 0x8000 ? tileNumber : (int)(sbyte)tileNumber) * 16 + 2 * (drawingWindow ? (wly % 8) : ((ly + parent.Memory.Get(0xFF42)) % 8)) + 1);
                    tileData += (ushort)(parent.Memory.Get(highDataAddr) << 8);
                    break;
                case 6: // Attempt push
                    if(pixelFifo.Count == 0)
                    {
                        for(ushort bmp = 0b1000000010000000, bit = 7; ; bmp >>= 1, bit--)
                        {
                            ushort tmp = (ushort)(tileData & bmp);
                            pixelFifo.Enqueue((parent.Memory.Get(0xFF40) & 1) == 0 ? new FifoPixel(0, false) : new FifoPixel((byte)(((tmp >> bit) & 0xFF) + ((tmp >> (bit + 7)) & 0xFF)), false));
                            if (bit == 0) break;
                        }
                        fetcherState = 0;
                    }
                    return;
            }
            fetcherState++;
        }
        #endregion

        #region OAM DMA
        private void ProcessDMA()
        {
            if(parent.Memory.DMATransfer)
            {
                if(!dmaInit)
                {
                    // Initialize DMA in first cycle
                    dmaOffset = 0;
                    dmaInit = true;
                } else {
                    // Handle cooldown cycles
                    if(dmaCooldown > 0)
                    {
                        dmaCooldown--;
                        return;
                    }

                    // Copy memory
                    parent.Memory.Set(0xFE00 + dmaOffset, parent.Memory.Get(parent.Memory.DMAAddr + dmaOffset));
                    dmaOffset++;
                    dmaCooldown = 3;
                    
                    if(dmaOffset == 0xA0) // If end of DMA copy block has been reached
                    {
                        dmaInit = false;
                        parent.Memory.DMATransfer = false;
                    }
                }
            }
        }
        #endregion

        #region Utility Functions
        private void ChangeMode(byte newMode)
        {
            mode = newMode;
            parent.Memory.Set(0xFF41, (byte)((parent.Memory.Get(0xFF41) & 0b1111000) | (ly == parent.Memory.Get(0xFF45) ? 0b100 : 0b000) | mode));

            // Console.WriteLine("Switched to mode " + newMode + " at " + scanlineCycles + " | LY: " + ly);

            if (mode == 1) // VBlank Interrupt
                parent.Memory.Set(0xFF0F, (byte)(parent.Memory.Get(0xFF0F) | 1));

            UpdateStatInterrupts();
        }

        private void StartNewFrame()
        {
            ly = 0;
            parent.Memory.Set(0xFF44, ly);
            wly = 0;
            ChangeMode(2);

            // Swap LCD buffers
            byte[,] tmp = LCDBuf1;
            LCDBuf1 = LCDBuf2;
            LCDBuf2 = tmp;
            HasUpdated = true;
        }

        private void UpdateStatInterrupts()
        {
            bool stat = false;
            if ((parent.Memory.Get(0xFF41) & 0b1000000) != 0 && ly == parent.Memory.Get(0xFF45)) stat = true;
            else if ((parent.Memory.Get(0xFF41) & 0b100000) != 0 && mode == 2) stat = true;
            else if ((parent.Memory.Get(0xFF41) & 0b10000) != 0 && mode == 1) stat = true;
            else if ((parent.Memory.Get(0xFF41) & 0b1000) != 0 && mode == 0) stat = true;

            if(stat != lastStat)
            {
                lastStat = stat;
                if (stat) parent.Memory[0xFF0F] |= 2;
            }
        }
        #endregion
    }
}
