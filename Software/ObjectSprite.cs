using ChromaBoy.Hardware;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChromaBoy.Software
{
    public enum SpriteAttribute { ZeroPalette = 0b10000, XFlip = 0b100000, YFlip = 0b1000000, Priority = 0b10000000 };

    public class ObjectSprite
    {
        private Gameboy parent;

        public int X = 0;
        public int Y = 0;
        public byte TileNo = 0;

        private byte attributes = 0;

        public ObjectSprite(Gameboy parent, ushort addr)
        {
            Y = parent.Memory.Get(addr) - 16;
            X = parent.Memory.Get(addr + 1) - 8;
            TileNo = parent.Memory.Get(addr + 2);
            attributes = parent.Memory.Get(addr + 3);

            this.parent = parent;
        }

        public byte GetPixel(byte x, byte y)
        {
            if (HasAttribute(SpriteAttribute.YFlip))
                y = (byte)(parent.PPU.LargeSprites ? 15 - y : 7 - y);

            if (HasAttribute(SpriteAttribute.XFlip))
                x = (byte)(7 - x);

            int lt = TileNo & 0xFE;
            int ut = TileNo | 1;

            ushort tileBaseAddr = (ushort)(0x8000 | ((parent.PPU.LargeSprites ? y > 7 ? ut : lt : TileNo) << 4) + 2 * y);
            ushort tileData = (ushort)((parent.Memory.Get(tileBaseAddr) << 8) + (parent.Memory.Get(tileBaseAddr + 1)));
            ushort bmp = (ushort)(0b1000000010000000 >> (x % 8));

            byte lc = (byte)(((tileData & bmp) >> (7 - (x % 8))) << 1);
            byte uc = (byte)(((tileData & bmp) >> (14 - (x % 8))) >> 1);
            byte color = (byte)(lc | uc);
            if (color == 0) return 255;
            ushort paletteReg = (ushort)(!HasAttribute(SpriteAttribute.ZeroPalette) ? 0xFF48 : 0xFF49);
            return color == 0 ? (byte)(parent.Memory.Get(paletteReg) & 0b11) : color == 1 ? (byte)((parent.Memory.Get(paletteReg) & 0b1100) >> 2) : color == 2 ? (byte)((parent.Memory.Get(paletteReg) & 0b110000) >> 4) : (byte)((parent.Memory.Get(paletteReg) & 0b11000000) >> 6);
        }

        public bool HasAttribute(SpriteAttribute attr)
        {
            return (attributes & (byte)attr) != 0;
        }
    }
}
