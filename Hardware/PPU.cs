using System;
using System.Runtime.InteropServices.ComTypes;

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

        private bool WindowEnable = false;
        private byte WX = 0;
        private byte WY = 0;

        private ushort BGTilemapBaseAddr = 0x9800;
        private ushort BGTileDataBaseAddr = 0x8000;
        private ushort WDTilemapBaseAddr = 0x9800;

        public static byte[,] Display = new byte[Emulator.SCREEN_WIDTH, Emulator.SCREEN_HEIGHT];
        public static byte[,] Background = new byte[256, 256];
        public static byte[,] Window = new byte[256, 256];

        // TODO: Draw Object Sprites
        // TODO: Merging Object Sprites & Background Tiles
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

            WindowEnable = (parent.Memory[0xFF40] & 0b100000) > 0;
            WX = (byte)(parent.Memory.Get(0xFF4B) - 7);
            WY = parent.Memory.Get(0xFF4A);

            // Draw Frame
            if (mode == 3)
            {
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
                        byte tmpX = (byte)(bSLX - WX);
                        byte tmpY = (byte)(ly - WY);

                        // Draw Window
                        int tileOffset = (tmpY % 8) * 2;
                        byte tileNo = parent.Memory.Get(WDTilemapBaseAddr + (tmpX / 8) + 32 * (tmpY / 8));
                        ushort tileBaseAddr = (ushort)(BGTileDataBaseAddr + (BGTileDataBaseAddr == 0x8000 ? tileNo : (int)(sbyte)tileNo) * 16 + tileOffset);
                        ushort tileData = (ushort)((parent.Memory.Get(tileBaseAddr) << 8) + (parent.Memory.Get(tileBaseAddr + 1)));
                        ushort bmp = (ushort)(0b1000000010000000 >> (tmpX % 8));

                        byte lc = (byte)((tileData & bmp) >> (7 - (tmpX % 8)));
                        byte uc = (byte)((tileData & bmp) >> (14 - (tmpX % 8)));
                        byte color = (byte)(lc | uc);
                        byte shade = color == 0 ? (byte)(parent.Memory.Get(0xFF47) & 0b11) : color == 1 ? (byte)((parent.Memory.Get(0xFF47) & 0b1100) >> 2) : color == 2 ? (byte)((parent.Memory.Get(0xFF47) & 0b110000) >> 4) : (byte)((parent.Memory.Get(0xFF47) & 0b11000000) >> 6);

                        Background[bSLX, ly] = shade;
                    }
                    else
                    {
                        // Draw Background
                        for (int i = 0; i < 8; i++, bSLX++)
                        {
                            int tileOffset = (ly % 8) * 2;
                            byte tileNo = parent.Memory.Get(BGTilemapBaseAddr + (bSLX / 8) + 32 * (ly / 8));
                            ushort tileBaseAddr = (ushort)(BGTileDataBaseAddr + (BGTileDataBaseAddr == 0x8000 ? tileNo : (int)(sbyte)tileNo) * 16 + tileOffset);
                            ushort tileData = (ushort)((parent.Memory.Get(tileBaseAddr) << 8) + (parent.Memory.Get(tileBaseAddr + 1)));
                            ushort bmp = (ushort)(0b1000000010000000 >> (bSLX % 8));

                            byte lc = (byte)((tileData & bmp) >> (7 - (bSLX % 8)));
                            byte uc = (byte)((tileData & bmp) >> (14 - (bSLX % 8)));
                            byte color = (byte)(lc | uc);
                            byte shade = color == 0 ? (byte)(parent.Memory.Get(0xFF47) & 0b11) : color == 1 ? (byte)((parent.Memory.Get(0xFF47) & 0b1100) >> 2) : color == 2 ? (byte)((parent.Memory.Get(0xFF47) & 0b110000) >> 4) : (byte)((parent.Memory.Get(0xFF47) & 0b11000000) >> 6);

                            Background[(byte)(bSLX - parent.Memory.Get(0xFF43)), (byte)(ly - parent.Memory.Get(0xFF42))] = shade;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 8; i++, bSLX++)
                        Background[bSLX, ly] = 0;
                }

                if (bSLX == 0) LineDone = true;
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
