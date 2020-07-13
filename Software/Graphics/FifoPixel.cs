namespace ChromaBoy.Software.Graphics
{
    public class FifoPixel
    {
        public readonly byte PixelData;
        public readonly bool IsSpritePixel;

        public FifoPixel(byte pixelData, bool spritePixel)
        {
            PixelData = pixelData;
            IsSpritePixel = spritePixel;
        }
    }
}
