using System;

namespace GameBoy.Core.Hardware.Graphics
{
    public class Sprite
    {
        private short y;
        private short x;

        public byte Id { get; set; }
        public short Y { get => y; private set => y = value; }
        public short X { get => x; private set => x = value; }
        public short TileNum { get; set; }
        public bool AboveBackground { get; set; }
        public bool YFlip { get; set; }
        public bool XFlip { get; set; }
        public bool UsePallete0 { get; set; }

        public bool ContainedInLine(byte line, bool doubleSize)
        {
            var ySize = doubleSize ? 16 : 8;

            return line >= y && line < y + ySize;
        }

        public bool ContainedInLineSection(byte xPos)
        {
            return xPos >= x && xPos < x + 8;
        }

        public void UpdateDetails(Span<byte> objectAttributeMemory, bool doubleSize)
        {
            Y = (short)(objectAttributeMemory[0] - 16);
            X = (short)(objectAttributeMemory[1] - 8);

            var tileNumByte = objectAttributeMemory[2];

            TileNum = (short)(doubleSize ? tileNumByte - tileNumByte % 2 : tileNumByte);

            AboveBackground = (objectAttributeMemory[3] & 0x80) == 0;
            YFlip = (objectAttributeMemory[3] & 0x40) != 0;
            XFlip = (objectAttributeMemory[3] & 0x20) != 0;
            UsePallete0 = (objectAttributeMemory[3] & 0x10) == 0;
        }

        public static Sprite Parse(byte index, byte[] objectAttributeMemory, bool doubleSize)
        {
            return Parse(index, objectAttributeMemory.AsSpan(), doubleSize);
        }

        public static Sprite Parse(byte index, Span<byte> objectAttributeMemory, bool doubleSize)
        {
            var retVal = new Sprite()
            {
                Id = index
            };

            retVal.UpdateDetails(objectAttributeMemory, doubleSize);

            return retVal;
        }
    }
}
