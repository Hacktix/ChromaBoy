using System;
using System.Runtime.InteropServices.ComTypes;

namespace ChromaBoy.Hardware
{
    public class PPU
    {
        private Gameboy parent;
        private long PPUCycles = 0;
        private long TimeoutCycles = 0;
        private byte DMACount = 0;
        private bool LineDone = false;
        private byte LastMode = 255;
        private bool statRequest = false;
        private bool lastStatRequest = false;
        private byte bSLX = 0;
        private byte lastLY = 255;
        private bool handledScrollTimeout = false;

        private bool WindowEnable = false;
        private byte WX = 0;
        private byte WY = 0;
        private byte WindowLineCount = 0;

        private ushort BGTilemapBaseAddr = 0x9800;
        private ushort BGTileDataBaseAddr = 0x8000;
        private ushort WDTilemapBaseAddr = 0x9800;

        public static bool CanDraw = false;
        public static byte[,] Display = new byte[Emulator.SCREEN_WIDTH, Emulator.SCREEN_HEIGHT];
        public static byte[,] Background = new byte[256, 256];
        public static byte[,] Window = new byte[256, 256];

        // TODO: Draw Object Sprites
        // TODO: Merging Object Sprites & Background Tiles

        public PPU(Gameboy parent)
        {
            this.parent = parent;
        }

        public void ProcessCycle()
        {
            if(TimeoutCycles > 0)
            {
                TimeoutCycles--;
                return;
            }

            // Set LY
            byte ly = (byte)(PPUCycles / 456);
            parent.Memory[0xFF44] = ly;

            statRequest = false;

            if (lastLY != ly)
            {
                lastLY = ly;
                bool lyc = ly == parent.Memory[0xFF45];
                parent.Memory[0xFF41] = (byte)((parent.Memory[0xFF41] & 0b1111011) | (lyc ? 0b100 : 0));
                if (lyc && (parent.Memory[0xFF41] & 0b1000000) != 0) statRequest = true;
            }

            // Handle OAM DMA Transfer
            if (parent.Memory.DMATransfer)
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

                switch(mode)
                {
                    case 0:
                        if ((parent.Memory[0xFF41] & 0b1000) != 0) statRequest = true;
                        break;
                    case 1:
                        if ((parent.Memory[0xFF41] & 0b10000) != 0) statRequest = true;
                        parent.Memory[0xFF0F] |= 1;
                        MergeToDisplay();
                        break;
                    case 2:
                        if ((parent.Memory[0xFF41] & 0b100000) != 0) statRequest = true;
                        LineDone = false;
                        WindowLineCount = 0;
                        handledScrollTimeout = false;
                        break;
                }
            }

            if(parent.Memory.UpdatedSTAT)
            {
                parent.Memory.UpdatedSTAT = false;
                switch (mode)
                {
                    case 0:
                        if ((parent.Memory[0xFF41] & 0b1000) != 0) statRequest = true;
                        break;
                    case 1:
                        if ((parent.Memory[0xFF41] & 0b10000) != 0) statRequest = true;
                        break;
                    case 2:
                        if ((parent.Memory[0xFF41] & 0b100000) != 0) statRequest = true;
                        break;
                }
                if ((ly == parent.Memory[0xFF45]) && (parent.Memory[0xFF41] & 0b1000000) != 0) statRequest = true;
            }

            if(lastStatRequest != statRequest)
            {
                lastStatRequest = statRequest;
                if(statRequest) parent.Memory[0xFF0F] |= 0b10;
            }

            WindowEnable = (parent.Memory[0xFF40] & 0b100000) > 0;
            WX = (byte)(parent.Memory.Get(0xFF4B) - 7);
            WY = parent.Memory.Get(0xFF4A);

            // Draw Frame
            if (mode == 3)
            {
                if(bSLX == 0 && !handledScrollTimeout && (parent.Memory.Get(0xFF43) % 8) > 0)
                {
                    handledScrollTimeout = true;
                    TimeoutCycles = (parent.Memory.Get(0xFF43) % 8) - 1;
                    return;
                }

                if ((parent.Memory[0xFF40] & 128) > 0)
                {
                    // Update Base Addresses
                    if ((parent.Memory[0xFF40] & 0b1000) > 0) BGTilemapBaseAddr = 0x9C00;
                    else BGTilemapBaseAddr = 0x9800;

                    if ((parent.Memory[0xFF40] & 0b10000) > 0) BGTileDataBaseAddr = 0x8000;
                    else BGTileDataBaseAddr = 0x9000;

                    if ((parent.Memory[0xFF40] & 0b1000000) > 0) WDTilemapBaseAddr = 0x9C00;
                    else WDTilemapBaseAddr = 0x9800;
                    
                    if (WindowEnable && bSLX >= WX && ly >= WY)
                    {
                        // Draw Window
                        byte tmpX = (byte)(bSLX - WX);
                        byte tmpY = WindowLineCount;

                        int tileOffset = (tmpY % 8) * 2;
                        byte tileNo = parent.Memory.Get(WDTilemapBaseAddr + (tmpX / 8) + 32 * (tmpY / 8));
                        ushort tileBaseAddr = (ushort)(BGTileDataBaseAddr + (BGTileDataBaseAddr == 0x8000 ? tileNo : (int)(sbyte)tileNo) * 16 + tileOffset);
                        ushort tileData = (ushort)((parent.Memory.Get(tileBaseAddr) << 8) + (parent.Memory.Get(tileBaseAddr + 1)));
                        ushort bmp = (ushort)(0b1000000010000000 >> (tmpX % 8));

                        byte lc = (byte)(((tileData & bmp) >> (7 - (tmpX % 8))) << 1);
                        byte uc = (byte)(((tileData & bmp) >> (14 - (tmpX % 8))) >> 1);
                        byte color = (byte)(lc | uc);
                        byte shade = color == 0 ? (byte)(parent.Memory.Get(0xFF47) & 0b11) : color == 1 ? (byte)((parent.Memory.Get(0xFF47) & 0b1100) >> 2) : color == 2 ? (byte)((parent.Memory.Get(0xFF47) & 0b110000) >> 4) : (byte)((parent.Memory.Get(0xFF47) & 0b11000000) >> 6);

                        Background[bSLX, ly] = shade;

                        if (++bSLX == 0)
                        {
                            bSLX = 0;
                            LineDone = true;
                            WindowLineCount++;
                        }
                    }
                    else
                    {
                        // Draw Background
                        int tileOffset = (ly % 8) * 2;
                        byte tileNo = parent.Memory.Get(BGTilemapBaseAddr + (bSLX / 8) + 32 * (ly / 8));
                        ushort tileBaseAddr = (ushort)(BGTileDataBaseAddr + (BGTileDataBaseAddr == 0x8000 ? tileNo : (int)(sbyte)tileNo) * 16 + tileOffset);
                        ushort tileData = (ushort)((parent.Memory.Get(tileBaseAddr) << 8) + (parent.Memory.Get(tileBaseAddr + 1)));
                        ushort bmp = (ushort)(0b1000000010000000 >> (bSLX % 8));

                        byte lc = (byte)(((tileData & bmp) >> (7 - (bSLX % 8))) << 1);
                        byte uc = (byte)(((tileData & bmp) >> (14 - (bSLX % 8))) >> 1);
                        byte color = (byte)(lc | uc);
                        byte shade = color == 0 ? (byte)(parent.Memory.Get(0xFF47) & 0b11) : color == 1 ? (byte)((parent.Memory.Get(0xFF47) & 0b1100) >> 2) : color == 2 ? (byte)((parent.Memory.Get(0xFF47) & 0b110000) >> 4) : (byte)((parent.Memory.Get(0xFF47) & 0b11000000) >> 6);

                        Background[(byte)(bSLX - parent.Memory.Get(0xFF43)), (byte)(ly - parent.Memory.Get(0xFF42))] = shade;

                        if (++bSLX == 0)
                        {
                            bSLX = 0;
                            LineDone = true;
                        }
                    }
                }
                else
                {
                    Background[bSLX, ly] = 0;
                    if (++bSLX == 0)
                        {
                            bSLX = 0;
                            LineDone = true;
                        }
                }
            }

            // Increment Cycle Count
            PPUCycles++;
            if (PPUCycles == 70224) PPUCycles = 0;
        }

        private void MergeToDisplay()
        {
            CanDraw = false;
            for(int x = 0; x < Emulator.SCREEN_WIDTH; x++)
            {
                for(int y = 0; y < Emulator.SCREEN_HEIGHT; y++)
                {
                    Display[x, y] = Background[x, y];
                }
            }
            CanDraw = true;
        }
    }
}
