using ChromaBoy.Software;
using System;
using System.Collections.Generic;

namespace ChromaBoy.Hardware
{
    public class PPU
    {
        private Gameboy parent;

        // # Cycle-related Variables
        private int PPUCycles = 0;
        private int TimeoutCycles = 0;

        // # Determines whether PPU should be reset due to unset LCD Enable bit
        private bool isReset = false;

        // # PPU Registers
        private byte Mode = 0;

        // * OAM Scans
        private List<ObjectSprite> scanlineSprites = new List<ObjectSprite>();
        private ushort scanAddress = 0xFE00;
        public bool LargeSprites = false;

        // * Drawing
        private byte LY = 255;
        private byte LX = 0;
        private bool EnableWindow = false;
        private byte WX = 0;
        private byte WY = 0;
        private byte WLY = 0;
        private byte SCX = 0;
        private byte SCY = 0;
        private bool DrawBackground = false;
        private bool DrawSprites = false;
        private bool scrollTimeout = false;

        // # Addresses
        private ushort WindowTilemapBaseAddr = 0x9800;
        private ushort BackgroundTileDataBaseAddr = 0x8800;
        private ushort BackgroundTilemapBaseAddr = 0x9800;
        private byte DMACount = 0;

        // # Buffers, temporary variables, etc.
        private bool lastStatRequest = false;
        private bool lineDone = false;
        private bool drewWindow = false;
        private bool checkedSTAT = false;

        // # Frontend Variables
        public byte[,] LCD = new byte[Emulator.SCREEN_WIDTH, Emulator.SCREEN_HEIGHT];
        public byte[,] LCDBuffer = new byte[Emulator.SCREEN_WIDTH, Emulator.SCREEN_HEIGHT];
        public bool HasUpdated = true;

        public PPU(Gameboy parent)
        {
            this.parent = parent;
        }

        public void ProcessCycle()
        {
            // PPU Clock Paused
            if (TimeoutCycles > 0)
            {
                TimeoutCycles--;
                return;
            }

            // LCD Disabled
            if ((parent.Memory[0xFF40] & 0b10000000) == 0)
            {
                if (!isReset) Reset();
                return;
            }
            else if (isReset) isReset = false;

            // Update PPU Register Values
            InitVariables();
            if (!checkedSTAT && parent.Memory.UpdatedSTAT)
            {
                HandleSTAT();
                parent.Memory.UpdatedSTAT = false;
            }
            else if(parent.Memory.UpdatedSTAT) parent.Memory.UpdatedSTAT = false;

            // Handle OAM DMA Transfer
            if (parent.Memory.DMATransfer)
            {
                parent.Memory.Set(0xFE00 + DMACount, parent.Memory.Get(parent.Memory.DMAAddr + DMACount));
                DMACount++;
                if (DMACount > 0x9F)
                {
                    DMACount = 0;
                    parent.Memory.DMAAddr = 0;
                    parent.Memory.DMATransfer = false;
                }
            }

            // Do calculations or whatever
            switch (Mode)
            {
                case 2: // OAM Scan
                    ProcessOAMTick();
                    break;
                case 3: // Drawing
                    ProcessDrawTick();
                    break;
            }

            // Increment Cycle Count
            PPUCycles++;
            if (PPUCycles == 70224) PPUCycles = 0;
        }

        private void ProcessOAMTick()
        {
            // Limit scan to OAM memory region
            if (scanAddress > 0xFE9F) return;

            // Fetch sprite data
            ObjectSprite sprite = new ObjectSprite(parent, scanAddress);
            scanAddress += 4;

            // Check sprite data & add to buffer
            int heightCheck = LargeSprites ? 16 : 8;
            if (LY - sprite.Y >= 0 && LY - sprite.Y < heightCheck && scanlineSprites.Count < 10)
                scanlineSprites.Add(sprite);
        }

        private void ProcessDrawTick()
        {
            if (!scrollTimeout && LX == 0 && (SCX % 8) != 0)
            {
                TimeoutCycles = (SCX % 8) - 1;
                PPUCycles--;
                scrollTimeout = true;
                return;
            }
            else scrollTimeout = false;

            byte backgroundPixel = 0;
            if (DrawBackground)
            {
                if (EnableWindow && LX >= WX && LY >= WY)
                {
                    backgroundPixel = GetWindowPixel();
                    if (!drewWindow) TimeoutCycles = 6;
                    drewWindow = true;
                }
                else backgroundPixel = GetBackgroundPixel();
            }

            byte spritePixel = 0;
            bool objPriority = false;
            bool hasSprite = false;
            if(DrawSprites)
            {
                ObjectSprite sprite = FindSprite();
                if (sprite != null)
                {
                    if (LX == sprite.X)
                    {
                        if(drewWindow) TimeoutCycles = 11 - Math.Min(5, (LX + (255 - WX)) % 8);
                        else TimeoutCycles = 11 - Math.Min(5, (LX + SCX) % 8);
                    }
                    spritePixel = GetSpritePixel(sprite);
                    objPriority = !sprite.HasAttribute(SpriteAttribute.Priority);
                    hasSprite = true;
                }
            }

            if (hasSprite)
            {
                if (spritePixel == 255 || (!objPriority && backgroundPixel != 0)) LCD[LX, LY] = backgroundPixel;
                else if (objPriority || backgroundPixel == 0) LCD[LX, LY] = spritePixel;
            }
            else LCD[LX, LY] = backgroundPixel;

            if(++LX == Emulator.SCREEN_WIDTH)
            {
                lineDone = true;
                LX = 0;
                if(drewWindow)
                {
                    drewWindow = false;
                    WLY++;
                }
            }
        }

        private byte GetSpritePixel(ObjectSprite sprite)
        {
            return sprite.GetPixel((byte)(LX - sprite.X), (byte)(LY - sprite.Y));
        }

        private ObjectSprite FindSprite()
        {
            List<ObjectSprite> candidates = new List<ObjectSprite>();
            ObjectSprite wouldApply = null;
            foreach (ObjectSprite sp in scanlineSprites)
            {
                if (sp.X <= LX && LX < (sp.X + 8))
                {
                    if (GetSpritePixel(sp) != 255)
                        candidates.Add(sp);
                    else wouldApply = sp;
                }
            }
            if(candidates.Count == 0) return wouldApply;

            ObjectSprite sprite = null;
            int lowestX = 255;
            foreach (ObjectSprite sp in candidates)
            {
                if (sp.X < lowestX)
                {
                    sprite = sp;
                    lowestX = sp.X;
                }
            }
            return sprite;
        }

        private byte GetBackgroundPixel()
        {
            byte BGX = (byte)(LX + SCX);
            byte BGY = (byte)(LY + SCY);

            int tileOffset = (BGY % 8) * 2;
            byte tileNo = parent.Memory.Get(BackgroundTilemapBaseAddr + (BGX / 8) + 32 * (BGY / 8));
            ushort tileBaseAddr = (ushort)(BackgroundTileDataBaseAddr + (BackgroundTileDataBaseAddr == 0x8000 ? tileNo : (int)(sbyte)tileNo) * 16 + tileOffset);
            ushort tileData = (ushort)((parent.Memory.Get(tileBaseAddr) << 8) + (parent.Memory.Get(tileBaseAddr + 1)));
            ushort bmp = (ushort)(0b1000000010000000 >> (BGX % 8));

            byte lc = (byte)(((tileData & bmp) >> (7 - (BGX % 8))) << 1);
            byte uc = (byte)(((tileData & bmp) >> (14 - (BGX % 8))) >> 1);
            byte color = (byte)(lc | uc);
            return color == 0 ? (byte)(parent.Memory.Get(0xFF47) & 0b11) : color == 1 ? (byte)((parent.Memory.Get(0xFF47) & 0b1100) >> 2) : color == 2 ? (byte)((parent.Memory.Get(0xFF47) & 0b110000) >> 4) : (byte)((parent.Memory.Get(0xFF47) & 0b11000000) >> 6);
        }

        private byte GetWindowPixel()
        {
            byte WLX = (byte)(LX - WX);

            int tileOffset = (WLY % 8) * 2;
            byte tileNo = parent.Memory.Get(WindowTilemapBaseAddr + (WLX / 8) + 32 * (WLY / 8));
            ushort tileBaseAddr = (ushort)(BackgroundTileDataBaseAddr + (BackgroundTileDataBaseAddr == 0x8000 ? tileNo : (int)(sbyte)tileNo) * 16 + tileOffset);
            ushort tileData = (ushort)((parent.Memory.Get(tileBaseAddr) << 8) + (parent.Memory.Get(tileBaseAddr + 1)));
            ushort bmp = (ushort)(0b1000000010000000 >> (WLX % 8));

            byte lc = (byte)(((tileData & bmp) >> (7 - (WLX % 8))) << 1);
            byte uc = (byte)(((tileData & bmp) >> (14 - (WLX % 8))) >> 1);
            byte color = (byte)(lc | uc);
            return color == 0 ? (byte)(parent.Memory.Get(0xFF47) & 0b11) : color == 1 ? (byte)((parent.Memory.Get(0xFF47) & 0b1100) >> 2) : color == 2 ? (byte)((parent.Memory.Get(0xFF47) & 0b110000) >> 4) : (byte)((parent.Memory.Get(0xFF47) & 0b11000000) >> 6);
        }

        private ushort GetTileData(ushort address)
        {
            return (ushort)((parent.Memory.Get(address) << 8) + (parent.Memory.Get(address + 1)));
        }

        private void HandleSTAT()
        {
            bool statRequest = false;

            // Mode STAT IRQs
            switch (Mode)
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

            // LYC Coincidence IRQ
            if ((LY == parent.Memory[0xFF45]) && (parent.Memory[0xFF41] & 0b1000000) != 0) statRequest = true;

            if(statRequest != lastStatRequest)
            {
                lastStatRequest = statRequest;
                if (statRequest) parent.Memory[0xFF0F] |= 0b10;
            } 
        }

        private void InitVariables()
        {
            checkedSTAT = false;

            // Update LY
            byte tmpLY = (byte)(PPUCycles / 456);
            if (LY != tmpLY)
            {
                checkedSTAT = true;
                LY = tmpLY;
                parent.Memory[0xFF44] = LY;
                parent.Memory.Set(0xFF41, (byte)((parent.Memory[0xFF41] & 0b11111011) | (LY == parent.Memory[0xFF45] ? 0b100 : 0b000)));

                lineDone = false;
            }

            // Update PPU Mode
            byte tmpMode = (byte)((PPUCycles % 456) < 80 ? 2 : LY > 143 ? 1 : lineDone ? 0 : 3);
            if (Mode != tmpMode)
            {
                checkedSTAT = true;
                Mode = tmpMode;
                HandleModeUpdate();
            }

            if (checkedSTAT) HandleSTAT();

            // Update registers from memory
            EnableWindow = (parent.Memory[0xFF40] & 0b100000) > 0;
            WX = (byte)(parent.Memory.Get(0xFF4B) - 7);
            WY = parent.Memory.Get(0xFF4A);
            SCX = parent.Memory.Get(0xFF43);
            SCY = parent.Memory.Get(0xFF42);
            DrawBackground = (parent.Memory.Get(0xFF40) & 0b1) != 0;
            DrawSprites = (parent.Memory.Get(0xFF40) & 0b10) != 0;
            LargeSprites = (parent.Memory.Get(0xFF40) & 0b100) != 0;

            // Update memory addresses
            WindowTilemapBaseAddr = (ushort)((parent.Memory.Get(0xFF40) & 0b1000000) == 0 ? 0x9800 : 0x9C00);
            BackgroundTileDataBaseAddr = (ushort)((parent.Memory.Get(0xFF40) & 0b10000) == 0 ? 0x9000 : 0x8000);
            BackgroundTilemapBaseAddr = (ushort)((parent.Memory.Get(0xFF40) & 0b1000) == 0 ? 0x9800 : 0x9C00);
        }

        private void HandleModeUpdate()
        {
            switch(Mode)
            {
                case 0:
                    parent.Memory.LockOAM = false;
                    parent.Memory.LockVRAM = false;
                    break;
                case 1:
                    WLY = 0;
                    parent.Memory.LockOAM = false;
                    parent.Memory.LockVRAM = false;
                    parent.Memory[0xFF0F] |= 1;
                    CopyToBuffer();
                    break;
                case 2:
                    parent.Memory.LockOAM = true;
                    parent.Memory.LockVRAM = false;
                    scanAddress = 0xFE00;
                    scanlineSprites.Clear();
                    break;
                case 3:
                    parent.Memory.LockOAM = true;
                    parent.Memory.LockVRAM = true;
                    break;
            }
            parent.Memory[0xFF41] = (byte)((parent.Memory[0xFF41] & 0b1111100) | Mode);
        }

        private void CopyToBuffer()
        {
            for (int x = 0; x < Emulator.SCREEN_WIDTH; x++)
                for (int y = 0; y < Emulator.SCREEN_HEIGHT; y++)
                    LCDBuffer[x, y] = LCD[x, y];
            HasUpdated = true;
        }

        private void Reset()
        {
            PPUCycles = 0;
            LY = 0;
            LX = 0;
            WLY = 0;
            scanlineSprites.Clear();

            lineDone = false;
            drewWindow = false;

            LCD = new byte[Emulator.SCREEN_WIDTH, Emulator.SCREEN_HEIGHT];

            parent.Memory.LockVRAM = false;
            parent.Memory.LockOAM = false;

            isReset = true;
        }
    }
}
