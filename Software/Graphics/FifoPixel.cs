namespace ChromaBoy.Software.Graphics
{
    public class FifoPixel
    {
        public readonly byte PixelData;
        public readonly bool IsSpritePixel;
        public readonly byte Palette;

        public FifoPixel(byte pixelData, bool spritePixel, byte palette = 0)
        {
            PixelData = pixelData;
            IsSpritePixel = spritePixel;
            Palette = palette;
        }
    }
}
