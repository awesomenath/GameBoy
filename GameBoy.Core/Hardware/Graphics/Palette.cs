using System.Collections.Generic;

namespace GameBoy.Core.Hardware.Graphics
{
    public class Palette
    {
        /// <summary>
        /// Set if an object palette, if true then values of 0 should be treated as transparent
        /// </summary>
        public bool IsObjectPalette { get; set; }
        public byte Colour0 { get; set; }
        public byte Colour1 { get; set; }
        public byte Colour2 { get; set; }
        public byte Colour3 { get; set; }

        public static Colour Colour0Rgb { get; } = new Colour(224, 248, 208);
        public static Colour Colour1Rgb { get; } = new Colour(136, 192, 112);
        public static Colour Colour2Rgb { get; } = new Colour(52, 104, 86);
        public static Colour Colour3Rgb { get; } = new Colour(8, 24, 32);

        private Dictionary<byte, byte> ColourMap { get; } = new Dictionary<byte, byte>();
        public byte[] ColourByteMapArray { get; } = new byte[4];
        public Colour[] ColourMapArray { get; } = new Colour[4];

        //filter = 0x1 << (7 - index);
        private static readonly byte[] PixelFilters = new byte[] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };

        public Palette()
        {
            ColourMap.Add(0, 0);
            ColourMap.Add(1, 0);
            ColourMap.Add(2, 0);
            ColourMap.Add(3, 0);
        }

        public static Palette Parse(byte paletteByte, bool isObjectPalette)
        {
            var retVal = new Palette()
            {
                IsObjectPalette = isObjectPalette
            };

            retVal.Update(paletteByte);

            return retVal;
        }

        private static byte SelectByteColour(int value)
        {
            // Inverted from black to white
            switch (value)
            {
                case 0:
                    return 255;
                case 1:
                    return 192;
                case 2:
                    return 96;
                default:
                case 3:
                    return 0;
            }
        }

        public void Update(byte paletteByte)
        {
            Colour0 = SelectByteColour(paletteByte & 0x03);
            Colour1 = SelectByteColour((paletteByte & 0x0C) >> 2);
            Colour2 = SelectByteColour((paletteByte & 0x30) >> 4);
            Colour3 = SelectByteColour(paletteByte >> 6);

            ColourMap[0] = Colour0;
            ColourMap[1] = Colour1;
            ColourMap[2] = Colour2;
            ColourMap[3] = Colour3;

            ColourByteMapArray[0] = Colour0;
            ColourByteMapArray[1] = Colour1;
            ColourByteMapArray[2] = Colour2;
            ColourByteMapArray[3] = Colour3;

            for (var i = 0; i < ColourByteMapArray.Length; i++)
            {
                ColourMapArray[i] = GetRgbColour(ColourByteMapArray[i]);
            }
        }

        private static Colour GetRgbColour(byte monochromeByte)
        {
            if (monochromeByte == 255)
            {
                return Colour0Rgb;
            }

            if (monochromeByte == 192)
            {
                return Colour1Rgb;
            }

            if (monochromeByte == 96)
            {
                return Colour2Rgb;
            }

            return Colour3Rgb;
        }

        public byte GetColourByte(byte colourIndex)
        {
            return ColourByteMapArray[colourIndex];
        }

        public byte GetColourByte(byte upperByte, byte lowerByte, byte index)
        {
            return ColourByteMapArray[GetNumber(upperByte, lowerByte, index)];
        }

        public Colour GetColour(byte upperByte, byte lowerByte, byte index)
        {
            return ColourMapArray[GetNumber(upperByte, lowerByte, index)];
        }

        public byte GetColourByte(ushort tileData, byte index)
        {
            return ColourByteMapArray[GetNumber(tileData, index)];
        }

        public static byte GetNumber(byte upperByte, byte lowerByte, byte index)
        {
            var filter = PixelFilters[index];

            var bottomVal = (lowerByte & filter) != 0;
            var topVal = (upperByte & filter) != 0;

            if (bottomVal && topVal)
            {
                return 3;
            }
            else if (bottomVal)
            {
                return 2;
            }
            else if (topVal)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        private static byte GetNumber(ushort tileData, byte index)
        {
            return GetNumber((byte)(tileData >> 8), (byte)tileData, index);
        }
    }
}
