using System;

namespace GameBoy.Core.Hardware.Graphics
{
    public class Tile
    {
        public const byte TileByteSize = 16;
        public const byte LargeTileByteSize = 32;

        public short TileNumber { get; set; }
        public byte[] TileData { get; private set; } = new byte[LargeTileByteSize];
        public byte[] ColourIndexData { get; private set; } = new byte[LargeTileByteSize * 4];
        public byte[][] ColourIndexDataLines { get; private set; } = new byte[TileByteSize][];

        public Tile(short tileNumber)
        {
            TileNumber = tileNumber;

            for (int i = 0; i < ColourIndexDataLines.Length; i++)
            {
                ColourIndexDataLines[i] = new byte[8]; // 8 bytes in a row
            }
        }

        public (byte upperByte, byte lowerByte) GetRowBytes(byte y)
        {
            return (TileData[y * 2], TileData[(y * 2) + 1]);
        }

        public byte[] GetRowColourIndexes(byte y)
        {
            return ColourIndexDataLines[y];
        }

        public void UpdateDetails(Span<byte> tileData)
        {
            for (var i = 0; i < tileData.Length; i++)
            {
                TileData[i] = tileData[i];
            }

            CalculateColourMap();
        }

        private void CalculateColourMap()
        {
            for (byte i = 0; i < TileData.Length / 2; i++)
            {
                var (upperByte, lowerByte) = GetRowBytes(i);

                for (byte index = 0; index < 8; index++)
                {
                    var colourArrayIndex = (i * 8) + index;
                    ColourIndexData[colourArrayIndex] = Palette.GetNumber(upperByte, lowerByte, index);
                    ColourIndexDataLines[i][index] = ColourIndexData[colourArrayIndex];
                }
            }
        }
    }
}
