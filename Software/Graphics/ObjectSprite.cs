using ChromaBoy.Hardware;

namespace ChromaBoy.Software
{
    public enum SpriteAttribute { ZeroPalette = 0b10000, XFlip = 0b100000, YFlip = 0b1000000, Priority = 0b10000000 };

    public class ObjectSprite
    {
        private Gameboy parent;

        public readonly int X = 0;
        public readonly int Y = 0;
        public readonly byte TileNo = 0;
        public readonly byte Attributes = 0;

        public ObjectSprite(Gameboy parent, ushort addr)
        {
            Y = parent.Memory.Get(addr);
            X = parent.Memory.Get(addr + 1);
            TileNo = parent.Memory.Get(addr + 2);
            Attributes = parent.Memory.Get(addr + 3);

            this.parent = parent;
        }

        public byte GetPixel(byte x, byte y)
        {
            if (HasAttribute(SpriteAttribute.YFlip))
                y = (byte)((parent.Memory.Get(0xFF40) & 0b100) != 0 ? 15 - y : 7 - y);

            if (HasAttribute(SpriteAttribute.XFlip))
                x = (byte)(7 - x);

            int lt = TileNo & 0xFE;
            int ut = TileNo | 1;

            ushort tileBaseAddr = (ushort)(0x8000 | (((parent.Memory.Get(0xFF40) & 0b100) != 0 ? y > 7 ? ut : lt : TileNo) << 4) + 2 * (y % 8));
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
            return (Attributes & (byte)attr) != 0;
        }
    }
}
