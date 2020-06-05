using System;

namespace ChromaBoy.Hardware
{
    public class PPU
    {
        private Gameboy parent;
        private long PPUCycles = 0;
        private byte DMACount = 0;
        private bool LineDone = false;
        private byte LastMode = 0;
        private byte bSLX = 0;

        private ushort BGTilemapBaseAddr = 0x9800;
        private ushort BGTileDataBaseAddr = 0x8000;

        public static byte[,] Display = new byte[Emulator.SCREEN_WIDTH, Emulator.SCREEN_HEIGHT];
        public static byte[,] Background = new byte[256,256];

        // TODO: Draw Object Sprites
        // TODO: Merging Object Sprites & Background Tiles
        // TODO: Background Scrolling
        // TODO: Window Display

        public PPU(Gameboy parent)
        {
            this.parent = parent;
        }

        public void ProcessCycle()
        {
            // Set LY
            byte ly = (byte)(PPUCycles / 456);
            parent.Memory[0xFF44] = ly;

            // Handle OAM DMA Transfer
            if(parent.Memory.DMATransfer)
            {
                parent.Memory.Set(0xFE00 + DMACount, parent.Memory[parent.Memory.DMAAddr + DMACount]);
                DMACount++;
                if(DMACount > 0x9F)
                {
                    DMACount = 0;
                    parent.Memory.DMAAddr = 0;
                    parent.Memory.DMATransfer = false;
                }
            }

            // Set Mode
            byte mode = (byte)((PPUCycles % 456) < 80 ? 2 : ly > 143 ? 1 : LineDone ? 0 : 3);
            if (LineDone && mode == 2) LineDone = false;
            parent.Memory[0xFF41] = (byte)((parent.Memory[0xFF41] & 0b1111100) | mode);

            // Mode Interrupts
            if(mode != LastMode)
            {
                LastMode = mode;
                if (mode == 2)
                {
                    if ((parent.Memory[0xFF41] & 0b100000) != 0) parent.Memory[0xFF0F] |= 0b10;
                    bool lyc = ly == parent.Memory[0xFF45];
                    parent.Memory[0xFF41] = (byte)((parent.Memory[0xFF41] & 0b1111011) | (lyc ? 0b100 : 0));
                    if ((parent.Memory[0xFF41] & 0b1000000) != 0) parent.Memory[0xFF0F] |= 0b10;
                }
                    
                else if (mode == 3 && ((parent.Memory[0xFF41] & 0b10000) != 0))
                    parent.Memory[0xFF0F] |= 0b10;
                if (mode == 1)
                {
                    if ((parent.Memory[0xFF41] & 0b1000) != 0) parent.Memory[0xFF0F] |= 0b10;
                    parent.Memory[0xFF0F] |= 1;
                    MergeToDisplay();
                }
            }

            // Draw Frame
            if(mode == 3)
            {
                // Draw Background
                int tileOffset = (ly % 8) * 2;
                byte tileNo = parent.Memory.Get(BGTilemapBaseAddr + (bSLX/8) + 32 * (ly/8));
                ushort tileBaseAddr = (ushort)(BGTileDataBaseAddr + tileNo*16 + tileOffset);
                ushort tileData = (ushort)((parent.Memory.Get(tileBaseAddr) << 8) + (parent.Memory.Get(tileBaseAddr + 1)));
                ushort bmp = (ushort)(0b1000000010000000 >> (bSLX % 8));

                byte lc = (byte)((tileData & bmp) >> (7 - (bSLX % 8)));
                byte uc = (byte)((tileData & bmp) >> (14 - (bSLX % 8)));
                byte color = (byte)(lc | uc);
                byte shade = color == 0 ? (byte)(parent.Memory.Get(0xFF47) & 0b11) : color == 1 ? (byte)((parent.Memory.Get(0xFF47) & 0b1100) >> 2) : color == 2 ? (byte)((parent.Memory.Get(0xFF47) & 0b110000) >> 4) : (byte)((parent.Memory.Get(0xFF47) & 0b11000000) >> 6);

                Background[bSLX, ly] = shade;

                if (++bSLX == 0) LineDone = true;
            }

            // Increment Cycle Count
            PPUCycles++;
            if (PPUCycles == 70224) PPUCycles = 0;
        }

        private void MergeToDisplay()
        {
            for(int x = 0; x < Emulator.SCREEN_WIDTH; x++)
            {
                for(int y = 0; y < Emulator.SCREEN_HEIGHT; y++)
                {
                    Display[x, y] = Background[x, y];
                }
            }
        }
    }
}
