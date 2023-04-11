using GameBoy.Core.Hardware.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GameBoy.Core.Hardware
{
    public class Gpu
    {
        public enum GpuMode
        {
            HorizontalBlank = 0,
            VerticalBlank = 1,
            ScanlineOam = 2,
            ScanlineVram = 3
        }

        private enum GraphicsMemoryFlagLocations : ushort
        {
            Lcdc = 0xFF40,
            Stat = 0xFF41,
            ScrollY = 0xFF42,
            ScrollX = 0xFF43,
            LcdcY = 0xFF44,
            LyCompare = 0xFF45,
            Dma = 0xFF46,
            BgPalette = 0xFF47,
            ObjPalette0 = 0xFF48,
            ObjPalette1 = 0xFF49,
            WindowY = 0xFF4A, // 0 <= value <= 166 to be visible
            WindowX = 0xFF4B // 0 <= value <= 166 to be visible
        }

        private Mmu Mmu { get; }
        public byte[] VideoBuffer { get; private set; }
        public byte[] BgBuffer { get; private set; }
        public byte[] SpriteBuffer { get; private set; }
        public byte[] WindowBuffer { get; private set; }
        public byte[] TileBuffer1 { get; private set; }
        public byte[] TileBuffer2 { get; private set; }
        public byte[] TileBuffer3 { get; private set; }
        public bool RenderDebugBuffers { get; set; } = true;

        private int Clock { get; set; }
        private GpuMode CurrentMode { get; set; }
        private byte CurrentLine { get; set; } // Lines 0 - 143 
        private byte WindowRenderLineCount { get; set; } // Lines 0 - 143 
        private List<Sprite> CurrentLineSprites { get; set; } // Lines 0 - 143 
        private Dictionary<byte, Sprite> Sprites { get; set; } = new Dictionary<byte, Sprite>();
        private Dictionary<int, Tile> TileSet1 { get; set; } = new Dictionary<int, Tile>();
        private Dictionary<int, Tile> TileSet2 { get; set; } = new Dictionary<int, Tile>();
        private Dictionary<int, Tile> TileSet3 { get; set; } = new Dictionary<int, Tile>();

        private Palette BgPalette { get; set; } = Palette.Parse(0, false);
        private Palette ObjPalette0 { get; set; } = Palette.Parse(0, true);
        private Palette ObjPalette1 { get; set; } = Palette.Parse(0, true);

        private byte Lcdc { get; set; }
        private bool BackgroundEnabled { get; set; }
        private bool SpritesEnabled { get; set; }
        private bool SpriteUseTileMap0 { get; set; }
        private bool WindowEnabled { get; set; }
        private bool UseSignedTileMapByte { get; set; }

        private byte ScrollX { get; set; }
        private byte ScrollY { get; set; }

        private byte WindowX { get; set; }
        private byte WindowY { get; set; }

        private bool Tile8By16Mode { get; set; }
        private byte CurrentTileByteSize { get; set; } = Tile.TileByteSize;

        public const int VideoWidth = 160;
        public const int VideoHeight = 144;
        public const int BackgroundWidth = 256;
        public const int BackgroundHeight = 256;
        public const int Stride = 3 * VideoWidth;
        public const int SpriteSize = 32;
        public Func<byte[], Task> FrameRenderedEvent;

        private Queue<Tuple<Action<ushort>, ushort>> DelayedMemoryTriggers = new();

        private bool GpuDisabled = false;

        public Gpu(Mmu mmu, Func<byte[], Task> frameRenderedCallback)
        {
            Mmu = mmu;
            VideoBuffer = new byte[3 * VideoWidth * VideoHeight]; // RGB * width * height
            BgBuffer = new byte[3 * BackgroundWidth * BackgroundHeight]; // RGB * width * height
            TileBuffer1 = new byte[3 * 128 * 8 * 8]; // RGB * width * height
            TileBuffer2 = new byte[3 * 128 * 8 * 8]; // RGB * width * height
            TileBuffer3 = new byte[3 * 128 * 8 * 8]; // RGB * width * height
            SpriteBuffer = new byte[3 * VideoWidth * VideoHeight]; // RGB * width * height
            WindowBuffer = new byte[3 * VideoWidth * VideoHeight]; // RGB * width * height

            ClearBuffers();

            FrameRenderedEvent = frameRenderedCallback;

            mmu.SubscribeToMemorySpaceEvents(mmu.Io, IoMemoryWriteEventTrigger);
            mmu.SubscribeToMemorySpaceEvents(mmu.GraphicSpriteInformation, OamWriteEventTrigger);
            mmu.SubscribeToMemorySpaceEvents(mmu.TilePatternTable1Full, TilePattern1WriteEventTrigger);
            mmu.SubscribeToMemorySpaceEvents(mmu.TilePatternTable2Full, TilePattern3WriteEventTrigger);
            mmu.SubscribeToMemorySpaceEvents(mmu.BgTileMap, BgTileMapWriteEventTrigger);
        }

        private void IoMemoryWriteEventTrigger(ushort address)
        {
            if (address == (ushort)GraphicsMemoryFlagLocations.Lcdc)
            {
                Lcdc = Mmu.ReadByte(address);
                //Debug.WriteLine($"Lcdc: {Lcdc:x2}");
                var newTile8By16Mode = (Lcdc & 0x04) > 0;

                if (newTile8By16Mode != Tile8By16Mode)
                {
                    Debug.WriteLine($"8By16Mode: {newTile8By16Mode}");
                    Tile8By16Mode = newTile8By16Mode;
                    CurrentTileByteSize = Tile8By16Mode ? Tile.LargeTileByteSize : Tile.TileByteSize;
                    ReSyncSprites();
                }

                var newBackgroundEnabled = (Lcdc & 0x01) != 0;

                if (newBackgroundEnabled != BackgroundEnabled)
                {
                    Debug.WriteLine($"BG enabled: {newBackgroundEnabled}");
                    BackgroundEnabled = newBackgroundEnabled;
                }

                var newSpritesEnabled = (Lcdc & 0x02) != 0;
                if (newSpritesEnabled != SpritesEnabled)
                {
                    Debug.WriteLine($"Sprites enabled: {newSpritesEnabled}");
                    SpritesEnabled = newSpritesEnabled;
                }

                //SpriteUseTileMap0 = (Lcdc & 0x10) != 0;

                var newWindowEnabled = (Lcdc & 0x20) != 0;
                if (newWindowEnabled != WindowEnabled)
                {
                    Debug.WriteLine($"Window enabled: {newWindowEnabled}");
                    WindowEnabled = newWindowEnabled;
                }

                var newUseSignedTileMapByte = (Lcdc & 0x10) == 0;
                if (newUseSignedTileMapByte != UseSignedTileMapByte)
                {
                    Debug.WriteLine($"UseSignedTileMapByte: {newUseSignedTileMapByte}");
                    UseSignedTileMapByte = newUseSignedTileMapByte;
                }
            }
            else if (address == (ushort)GraphicsMemoryFlagLocations.ScrollY)
            {
                DelayUpdateUntilEndOfMode3(addres =>
                {
                    ScrollY = Mmu.ReadByte(address);
                    //Debug.WriteLine($"BG Y: {ScrollY}");
                }, address);
            }
            else if (address == (ushort)GraphicsMemoryFlagLocations.ScrollX)
            {
                DelayUpdateUntilEndOfMode3(addres =>
                {
                    ScrollX = Mmu.ReadByte(address);
                    //Debug.WriteLine($"BG X: {ScrollX}");
                }, address);
            }
            else if (address == (ushort)GraphicsMemoryFlagLocations.BgPalette)
            {
                BgPalette.Update(Mmu.ReadByte(address));
                //Debug.WriteLine(nameof(BgPalette));
            }
            else if (address == (ushort)GraphicsMemoryFlagLocations.ObjPalette0)
            {
                ObjPalette0.Update(Mmu.ReadByte(address));
                //Debug.WriteLine(nameof(ObjPalette0));
            }
            else if (address == (ushort)GraphicsMemoryFlagLocations.ObjPalette1)
            {
                ObjPalette1.Update(Mmu.ReadByte(address));
                //Debug.WriteLine(nameof(ObjPalette1));
            }
            else if (address == (ushort)GraphicsMemoryFlagLocations.WindowX)
            {
                DelayUpdateUntilEndOfMode3(addres =>
                {
                    WindowX = (byte)(Mmu.ReadByte(address) - 7);
                    //Debug.WriteLine($"Window X: {WindowX}");
                }, address);
            }
            else if (address == (ushort)GraphicsMemoryFlagLocations.WindowY)
            {
                DelayUpdateUntilEndOfMode3(addres =>
                {
                    WindowY = Mmu.ReadByte(address);
                    //Debug.WriteLine($"Window Y: {WindowY}");
                }, address);
            }
            else if (address == (ushort)GraphicsMemoryFlagLocations.Dma)
            {
                var transferStartAddress = (ushort)(Mmu.ReadByte(address) << 8);
                //Debug.WriteLine($"OAM DMA triggered: {transferStartAddress:x2}");

                //var transferEndAddres = (ushort)(transferStartAddress | 0x9F);

                var bytesToCopy = Mmu.GetSpan(transferStartAddress, 160);

                for (byte i = 0; i < 40; i++)
                {
                    var byteIndex = i * 4;
                    CreateOrUpdateSpriteOam(i, bytesToCopy.Slice(byteIndex, 4));
                }

                Mmu.WriteSection(0xFE00, bytesToCopy);
            }
        }

        private void DelayUpdateUntilEndOfMode3(Action<ushort> action, ushort address)
        {
            if (CurrentMode == GpuMode.ScanlineVram)
            {
                //Debug.WriteLine($"delayed write {address:x2}");
                DelayedMemoryTriggers.Enqueue(new Tuple<Action<ushort>, ushort>(action, address));
            }
            else
            {
                action.Invoke(address);
            }
        }

        private void OamWriteEventTrigger(ushort address)
        {
            byte spriteIndex = (byte)((address - Mmu.GraphicSpriteInformation.Start) / 4);
            var affectedBytes = Mmu.GetSpan((ushort)(Mmu.GraphicSpriteInformation.Start + (spriteIndex * 4)), 4);

            CreateOrUpdateSpriteOam(spriteIndex, affectedBytes);
        }

        private void CreateOrUpdateSpriteOam(byte spriteIndex, Span<byte> bytes)
        {
            if (!Sprites.TryGetValue(spriteIndex, out var foundSprite))
            {
                var newSprite = Sprite.Parse(spriteIndex, bytes, Tile8By16Mode);
                Sprites.Add(spriteIndex, newSprite);
            }
            else
            {
                foundSprite.UpdateDetails(bytes, Tile8By16Mode);
            }
        }

        private void ReSyncSprites()
        {
            var oamBytes = Mmu.GetSpan(Mmu.GraphicSpriteInformation);

            for (byte i = 0; i < 40; i++)
            {
                var byteIndex = i * 4;
                CreateOrUpdateSpriteOam(i, oamBytes.Slice(byteIndex, 4));
            }
        }

        private void TilePattern1WriteEventTrigger(ushort address)
        {
            byte tileIndex = (byte)((address - Mmu.TilePatternTable1Full.Start) / Tile.TileByteSize);
            //var tileByte = Mmu.ReadByte(address);

            var tileBytes = Mmu.GetSpan((ushort)(Mmu.TilePatternTable1Full.Start + tileIndex * Tile.TileByteSize), Tile.LargeTileByteSize);
            if (!TileSet1.TryGetValue(tileIndex, out var foundTile))
            {
                var newTile = new Tile(tileIndex);
                TileSet1.Add(tileIndex, newTile);

                foundTile = newTile;
            }

            //if (tileByte != 0)
            //{
            //    Debug.WriteLine($"Tile: {tileIndex} updated {tileByte :x2}");
            //}

            foundTile.UpdateDetails(tileBytes);
        }

        private void TilePattern3WriteEventTrigger(ushort address)
        {
            byte tileIndex = (byte)((address - Mmu.TilePatternTable2Full.Start) / Tile.TileByteSize);

            var tileBytes = Mmu.GetSpan((ushort)(Mmu.TilePatternTable2Full.Start + tileIndex * Tile.TileByteSize), Tile.LargeTileByteSize);
            if (!TileSet3.TryGetValue(tileIndex - 128, out var foundTile))
            {
                var newTile = new Tile(tileIndex);
                TileSet3.Add(tileIndex - 128, newTile);

                foundTile = newTile;
            }

            foundTile.UpdateDetails(tileBytes);
        }

        private void BgTileMapWriteEventTrigger(ushort address)
        {
            var id = address - Mmu.BgTileMap.Start;

            //if (Mmu.ReadByte(address) > 0)
            //{
            //    Debug.WriteLine($"BgTileMap ID: {id} ({id % 32}, {id / 32}) - Val: {Mmu.ReadByte(address)}");
            //}
        }

        private void ClearBuffer(byte[] buffer)
        {
            // Set the buffer to magenta
            for (var i = 0; i < buffer.Length; i += 3)
            {
                buffer[i] = byte.MaxValue;
                buffer[i + 1] = 0;
                buffer[i + 2] = byte.MaxValue;
            }
        }

        private void ClearBuffers()
        {
            ClearBuffer(VideoBuffer);

            if (RenderDebugBuffers)
            {
                ClearBuffer(BgBuffer);
                ClearBuffer(SpriteBuffer);
                ClearBuffer(WindowBuffer);
            }
        }

        public void Step(int cycles)
        {
            if (GpuDisabled)
            {
                return;
            }

            Clock += cycles; // TODO: Could this get out of sync? Should be do a step for each cycle?

            switch (CurrentMode)
            {
                // OAM read mode, scanline active, MODE 2
                case GpuMode.ScanlineOam:
                    if (Clock >= 80)
                    {
                        // Enter scanline mode 3
                        Clock = 0;
                        ChangeState(GpuMode.ScanlineVram);

                        // get the sprites for this line
                        var lineSprites = new List<Sprite>(10);
                        foreach (var kvp in Sprites)
                        {
                            if (kvp.Value.ContainedInLine(CurrentLine, Tile8By16Mode))
                            {
                                lineSprites.Add(kvp.Value);

                                if (lineSprites.Count >= 10)
                                {
                                    break;
                                }
                            }
                        }

                        CurrentLineSprites = lineSprites.OrderBy(ls => ls.X).ToList();
                    }
                    break;

                // VRAM read mode, scanline active, MODE 3
                // Treat end of mode 3 as end of scanline
                case GpuMode.ScanlineVram:
                    if (Clock >= 172)
                    {
                        // Enter hblank
                        Clock = 0;
                        ChangeState(GpuMode.HorizontalBlank);

                        // Write a scanline to the framebuffer
                        RenderLine();
                    }
                    break;

                // Hblank, MODE 0
                // After the last hblank, push the screen data to canvas
                case GpuMode.HorizontalBlank:
                    if (Clock >= 204)
                    {
                        Clock = 0;
                        SetCurrentLine((byte)(CurrentLine + 1));

                        if (CurrentLine == 144)
                        {
                            // Enter vblank
                            ChangeState(GpuMode.VerticalBlank);
                            //GPU._canvas.putImageData(GPU._scrn, 0, 0);
                        }
                        else
                        {
                            ChangeState(GpuMode.ScanlineOam);
                        }
                    }
                    break;

                // Vblank (10 lines), MODE 1
                case GpuMode.VerticalBlank:
                    if (Clock >= 456)
                    {
                        Clock = 0;
                        SetCurrentLine((byte)(CurrentLine + 1));

                        if (CurrentLine > 153)
                        {
                            // Restart scanning modes
                            ChangeState(GpuMode.ScanlineOam);
                            SetCurrentLine(0);
                            WindowRenderLineCount = 0;
                            ClearBuffers();
                        }
                    }
                    break;
            }
        }

        public void ForceRender()
        {
            RenderBackground();
            FlushBuffer();
        }

        private void SetCurrentLine(byte newLine)
        {
            CurrentLine = newLine;
            //Debug.WriteLine($"Line {newLine}");

            // Update state
            Mmu.WriteByte((ushort)GraphicsMemoryFlagLocations.LcdcY, CurrentLine);
        }

        private void ChangeState(GpuMode newMode)
        {
            var lcdStat = Mmu.ReadByte((ushort)GraphicsMemoryFlagLocations.Stat);
            lcdStat >>= 2;
            lcdStat <<= 2; // bit 1 & 0 are now 0

            switch (newMode)
            {
                case GpuMode.HorizontalBlank:
                    if ((lcdStat & 0x08) > 1)
                    {
                        // Mode 0 H-Blank Interrupt
                        RaiseLcdcInterrupt();
                    }

                    break;
                case GpuMode.VerticalBlank:
                    if ((lcdStat & 0x10) > 1)
                    {
                        // Mode 1 V-Blank Interrupt
                        RaiseLcdcInterrupt();
                    }

                    lcdStat |= 0x01;
                    Mmu.WriteByte(Mmu.InterruptFlags.Start, (byte)(Mmu.ReadByte(Mmu.InterruptFlags.Start) | 0x01)); // Write the VBlank interrupt request

                    RenderBackground();
                    FlushBuffer();

                    break;
                case GpuMode.ScanlineOam:
                    if ((lcdStat & 0x20) > 1)
                    {
                        // Mode 2 OAM Interrupt
                        RaiseLcdcInterrupt();
                    }

                    lcdStat |= 0x02;
                    break;
                case GpuMode.ScanlineVram:
                    if ((lcdStat & 0x40) > 1)
                    {
                        // LYC = LY Interrupt
                        var lyCompareVal = Mmu.ReadByte((ushort)GraphicsMemoryFlagLocations.LyCompare);
                        if (lyCompareVal == CurrentLine)
                        {
                            // LYC is used to compare a value to the LY register. If they match the match flag is set in the STAT register.
                            RaiseLcdcInterrupt();
                            lcdStat |= 0x04;

                            // ACID Debug HACK
                            //if (CurrentLine == 143)
                            //{
                            //    RenderBackground();
                            //    FlushBuffer();
                            //    GpuDisabled = true; // hack for taking screenshot
                            //}
                        }
                    }

                    lcdStat |= 0x03;
                    break;
            }

            if (CurrentMode != newMode && newMode == GpuMode.HorizontalBlank)
            {
                while (DelayedMemoryTriggers.Any())
                {
                    var delayedEvent = DelayedMemoryTriggers.Dequeue();
                    delayedEvent.Item1.Invoke(delayedEvent.Item2);
                }
            }

            Mmu.WriteByte((ushort)GraphicsMemoryFlagLocations.Stat, lcdStat);
            CurrentMode = newMode;
        }

        private void RaiseLcdcInterrupt()
        {
            Mmu.WriteByte(Mmu.InterruptFlags.Start, (byte)(Mmu.ReadByte(Mmu.InterruptFlags.Start) | 0x02)); // Write the LCDC interrupt request
        }

        /// <summary>
        /// Render the buffer to a frame
        /// </summary>
        public async Task FlushBuffer()
        {
            FrameRenderedEvent(VideoBuffer);
        }

        private Span<byte> GetTileMap(bool isBackground = true)
        {
            Span<byte> tileMap;

            var useTileMap0 = true;

            if (isBackground)
            {
                useTileMap0 = (Lcdc & 0x08) == 0;
            }
            else
            {
                useTileMap0 = (Lcdc & 0x40) == 0;
            }


            if (useTileMap0)
            {
                tileMap = Mmu.GetSpan(Mmu.BgTileMap);
            }
            else
            {
                tileMap = Mmu.GetSpan(Mmu.BgTileMap2);
            }

            return tileMap;
        }

        private Dictionary<int, Tile> GetTileSet()
        {
            var tileSetToUse = TileSet1;

            if ((Lcdc & 0x10) == 0)
            {
                tileSetToUse = TileSet3;
            }

            return tileSetToUse;
        }

        private void RenderLine()
        {
            var tileSet = GetTileSet();
            var lineStride = CurrentLine * Stride;
            var enabled = (Lcdc & 0x80) != 0;

            if (!enabled)
            {
                return;
            }

            var bakgroundTileMap = GetTileMap();
            var windowTileMap = GetTileMap(false);
            bool windowRenderedOnLine = false;

            // Render sprites
            var strideOffset = lineStride;
            for (byte i = 0; i < VideoWidth; i++) // each pixel
            {
                var spritePixelResult = GetSpritePixel(i, CurrentLine);
                var bgPixelResult = GetBackgroundPixel(i, CurrentLine, bakgroundTileMap, tileSet);
                var windowPixelResult = GetWindowPixel(i, CurrentLine, windowTileMap, tileSet);

                if (bgPixelResult != null)
                {
                    VideoBuffer[strideOffset] = bgPixelResult.Value.Colour.Red;
                    VideoBuffer[strideOffset + 1] = bgPixelResult.Value.Colour.Green;
                    VideoBuffer[strideOffset + 2] = bgPixelResult.Value.Colour.Blue;
                }

                if (windowPixelResult != null)
                {
                    VideoBuffer[strideOffset] = windowPixelResult.Value.Colour.Red;
                    VideoBuffer[strideOffset + 1] = windowPixelResult.Value.Colour.Green;
                    VideoBuffer[strideOffset + 2] = windowPixelResult.Value.Colour.Blue;

                    if (RenderDebugBuffers)
                    {
                        WindowBuffer[strideOffset] = windowPixelResult.Value.Colour.Red;
                        WindowBuffer[strideOffset + 1] = windowPixelResult.Value.Colour.Green;
                        WindowBuffer[strideOffset + 2] = windowPixelResult.Value.Colour.Blue;
                    }

                    windowRenderedOnLine = true;
                }

                if (SpritesEnabled && spritePixelResult != null)
                {
                    if (!spritePixelResult.Value.IsTransparent)
                    {
                        if (spritePixelResult.Value.Sprite.AboveBackground)
                        {
                            VideoBuffer[strideOffset] = spritePixelResult.Value.Colour.Red;
                            VideoBuffer[strideOffset + 1] = spritePixelResult.Value.Colour.Green;
                            VideoBuffer[strideOffset + 2] = spritePixelResult.Value.Colour.Blue;
                        }
                        else if ((windowPixelResult == null && bgPixelResult != null && bgPixelResult.Value.ColourIndex == 0)
                            || (windowPixelResult != null && windowPixelResult.Value.ColourIndex == 0))
                        {
                            VideoBuffer[strideOffset] = spritePixelResult.Value.Colour.Red;
                            VideoBuffer[strideOffset + 1] = spritePixelResult.Value.Colour.Green;
                            VideoBuffer[strideOffset + 2] = spritePixelResult.Value.Colour.Blue;
                        }

                        if (RenderDebugBuffers)
                        {
                            SpriteBuffer[strideOffset] = spritePixelResult.Value.Colour.Red;
                            SpriteBuffer[strideOffset + 1] = spritePixelResult.Value.Colour.Green;
                            SpriteBuffer[strideOffset + 2] = spritePixelResult.Value.Colour.Blue;
                        }
                    }
                }

                if (bgPixelResult == null && windowPixelResult == null && spritePixelResult == null)
                {
                    VideoBuffer[strideOffset] = Palette.Colour0Rgb.Red;
                    VideoBuffer[strideOffset + 1] = Palette.Colour0Rgb.Green;
                    VideoBuffer[strideOffset + 2] = Palette.Colour0Rgb.Blue;
                }

                strideOffset += 3;
            }

            if (windowRenderedOnLine)
            {
                WindowRenderLineCount++;
            }
        }

        private PixelResult? GetSpritePixel(byte x, byte y)
        {
            var sprites = CurrentLineSprites;

            if (sprites.Count == 0)
            {
                return null;
            }

            for (int i = 0; i < sprites.Count; i++)
            {
                Sprite spriteToUse = sprites[i];

                if (!spriteToUse.ContainedInLineSection(x))
                {
                    continue;
                }

                var tile = TileSet1[spriteToUse.TileNum];

                byte relativeY = (byte)(y - spriteToUse.Y);

                if (spriteToUse.YFlip)
                {
                    relativeY = (byte)(CurrentTileByteSize / 2 - relativeY - 1);
                }

                int relativeX = x - spriteToUse.X;

                if (spriteToUse.XFlip)
                {
                    relativeX = 7 - relativeX;
                }

                var colourIndexes = tile.GetRowColourIndexes(relativeY);
                var paletteToUse = spriteToUse.UsePallete0 ? ObjPalette0 : ObjPalette1;

                var colourIndex = colourIndexes[relativeX];

                var isTransparent = colourIndex == 0;

                if (isTransparent)
                {
                    continue;
                }

                var result = new PixelResult
                {
                    Sprite = spriteToUse,
                    Colour = paletteToUse.ColourMapArray[colourIndex],
                    IsTransparent = isTransparent,
                    ColourIndex = colourIndex
                };

                return result;                
            }

            return null;
        }

        private PixelResult? GetBackgroundPixel(byte x, byte y, Span<byte> tileMap, Dictionary<int, Tile> tileSet)
        {
            if (!BackgroundEnabled)
            {
                return null;
            }

            var scrolledLine = y + ScrollY;
            var bgTileStartY = scrolledLine / 8;

            // TODO: Check?
            if (bgTileStartY > BackgroundHeight / 8)
            {
                bgTileStartY -= BackgroundHeight / 8;
            }

            var tileRefIndex = bgTileStartY * SpriteSize;
            var xIndexOffset = x + ScrollX;
            if (xIndexOffset >= BackgroundWidth)
            {
                xIndexOffset -= BackgroundWidth;
            }

            tileRefIndex += xIndexOffset / 8;

            // Wrap the tile index
            if (tileRefIndex >= tileMap.Length)
            {
                tileRefIndex -= tileMap.Length;
            }
            else if (tileRefIndex < 0)
            {
                tileRefIndex = tileMap.Length - 1;
            }

            byte tileRef = tileMap[tileRefIndex];
            Tile tile;
            bool tileFound;

            if (UseSignedTileMapByte)
            {
                tileFound = tileSet.TryGetValue((sbyte)(tileRef), out tile);
            }
            else
            {
                tileFound = tileSet.TryGetValue(tileRef, out tile);
            }

            if (tileFound)
            {
                var spriteLine = (byte)(scrolledLine % 8);
                var colourIndexes = tile.GetRowColourIndexes(spriteLine);
                var colourIndex = colourIndexes[xIndexOffset % 8];

                return new PixelResult()
                {
                    ColourIndex = colourIndex,
                    Colour = BgPalette.ColourMapArray[colourIndex]
                };
            }
            

            return null;
        }

        private PixelResult? GetWindowPixel(byte x, byte y, Span<byte> tileMap, Dictionary<int, Tile> tileSet)
        {
            if (WindowEnabled && BackgroundEnabled && WindowY <= 143 && WindowX < 166 && WindowX <= x && WindowY <= y)
            {
                var tileMapRelativeX = (x - WindowX) / 8;
                var tileMapRelativeY = WindowRenderLineCount / 8;

                var tileRefIndex = (tileMapRelativeY * 32) + tileMapRelativeX;

                byte tileRef = tileMap[tileRefIndex];
                Tile tile;
                bool tileFound;

                if (UseSignedTileMapByte)
                {
                    tileFound = tileSet.TryGetValue((sbyte)tileRef, out tile);
                }
                else
                {
                    tileFound = tileSet.TryGetValue(tileRef, out tile);
                }

                if (tileFound)
                {
                    var spriteLine = (byte)(y % 8);
                    var colourByteIndexes = tile.GetRowColourIndexes(spriteLine);
                    var colourByteIndex = colourByteIndexes[(x - WindowX) % 8];

                    return new PixelResult()
                    {
                        ColourIndex = colourByteIndex,
                        Colour = BgPalette.ColourMapArray[colourByteIndex]
                    };
                }
            }

            return null;
        }

        private struct PixelResult
        {
            public Colour Colour { get; set; }
            public byte ColourIndex { get; set; }
            public Sprite Sprite { get; set; }
            public bool IsTransparent { get; set; }
        }

        private void RenderSprite(Span<byte> colours, byte startingX, int lineStride, Palette palette, byte[] buffer, bool flipX = false, bool transparencyCheck = false, bool spriteBelowBackground = false, Sprite currentSprite = null)
        {
            var paletteColourBytes = palette.ColourByteMapArray;
            var paletteColours = palette.ColourMapArray;

            //if (flipX)
            //{
            //    paletteColourBytes = paletteColourBytes.Reverse().ToArray();
            //    paletteColours = paletteColours.Reverse().ToArray();
            //}

            var startingXIndex = lineStride + (startingX * 3);
            int xIndex = startingXIndex;

            if (flipX)
            {
                xIndex += 7 * 3;
            }

            // for each pixel in row
            for (byte i = 0; i < 8; i++)
            {
                // check if pixel is already drawn by another sprite
                var foundSprite = CurrentLineSprites.FirstOrDefault(cls => cls != currentSprite && cls.X <= startingX + i && cls.X + 7 >= startingX + i);
                if (foundSprite != null)
                {
                    continue;
                }

                if (!(transparencyCheck && colours[i] == 0) && !(spriteBelowBackground && buffer[xIndex] != Palette.Colour0Rgb.Red))
                {
                    var colour = paletteColours[colours[i]];

                    buffer[xIndex] = colour.Red;
                    buffer[xIndex + 1] = colour.Green;
                    buffer[xIndex + 2] = colour.Blue;
                }

                xIndex += flipX ? -3 : 3;
            }
        }

        private void RenderBlankSpritePixel(int xOffset, int lineStride, byte[] buffer)
        {
            var startingXIndex = lineStride + (xOffset * 3);
            int xIndex = startingXIndex;

            var colour = Palette.Colour0Rgb;

            buffer[xIndex] = colour.Red;
            buffer[xIndex + 1] = colour.Green;
            buffer[xIndex + 2] = colour.Blue;
        }

        private void RenderSpritePixel(Span<byte> colours, int xOffset, byte tileX, int lineStride, Palette palette, byte[] buffer, bool transparencyCheck = false, bool spriteBelowBackground = false)
        {
            var paletteColourBytes = palette.ColourByteMapArray;
            var paletteColours = palette.ColourMapArray;

            var startingXIndex = lineStride + (xOffset * 3);
            int xIndex = startingXIndex;

            // for each pixel in row
            if (!(transparencyCheck && colours[tileX] == 0) && !(spriteBelowBackground && buffer[xIndex] != 224))
            {
                var colour = paletteColours[colours[tileX]];

                buffer[xIndex] = colour.Red;
                buffer[xIndex + 1] = colour.Green;
                buffer[xIndex + 2] = colour.Blue;
            }
        }

        private void RenderSprite(byte upperByte, byte lowerByte, byte startingX, int lineStride, Palette palette, byte[] buffer)
        {
            var startingXIndex = lineStride + (startingX * 3);
            int xIndex = startingXIndex;

            // for each pixel in row
            for (byte i = 0; i < 8; i++)
            {
                var colour = palette.GetColour(upperByte, lowerByte, i);

                buffer[xIndex] = colour.Red;
                buffer[xIndex + 1] = colour.Green;
                buffer[xIndex + 2] = colour.Blue;

                xIndex += 3;
            }
        }

        private void RenderTileBuffer1()
        {
            var bgPalette = BgPalette;

            int bgStride = 3 * 8 * Tile.TileByteSize;

            byte line = 0;
            byte x = 0;
            for (byte i = 0; i < 128; i++)
            {
                if (TileSet1.TryGetValue(i, out var tile))
                {
                    var lineStride = bgStride * line;

                    for (byte spriteLine = 0; spriteLine < 8; spriteLine++)
                    {
                        var currentSpriteLineStride = lineStride + (spriteLine * bgStride);

                        var colourBytes = tile.GetRowColourIndexes(spriteLine);

                        RenderSprite(colourBytes, x, currentSpriteLineStride, bgPalette, TileBuffer1);
                    }
                }

                x += 8;

                // increase the line
                if (i > 0 && (i + 1) % Tile.TileByteSize == 0)
                {
                    x = 0;
                    line += 8;
                }
            }
        }

        private void RenderTileBuffer2()
        {
            var tileData1 = Mmu.GetSpan(Mmu.TilePatternTable2);
            int bgStride = 3 * 8 * Tile.TileByteSize;

            byte line = 0;
            byte x = 0;
            for (var i = 0; i < 128; i++)
            {
                var tileData = tileData1.Slice(i * Tile.TileByteSize, Tile.TileByteSize);

                var lineStride = bgStride * line;
                for (var spriteLine = 0; spriteLine < 8; spriteLine++)
                {
                    var currentSpriteLineStride = lineStride + (spriteLine * bgStride);

                    int spriteLineTileDataIndex = spriteLine * 2;
                    RenderSprite(tileData[spriteLineTileDataIndex], tileData[spriteLineTileDataIndex + 1], x, currentSpriteLineStride, BgPalette, TileBuffer2);
                }

                x += 8;

                // increase the line
                if (i > 0 && (i + 1) % Tile.TileByteSize == 0)
                {
                    x = 0;
                    line += 8;
                }
            }
        }

        private void RenderTileBuffer3()
        {
            var tileData1 = Mmu.GetSpan(Mmu.TilePatternTable3);
            int bgStride = 3 * 8 * Tile.TileByteSize;

            byte line = 0;
            byte x = 0;
            for (var i = 0; i < 128; i++)
            {
                var tileData = tileData1.Slice(i * Tile.TileByteSize, Tile.TileByteSize);

                var lineStride = bgStride * line;
                for (var spriteLine = 0; spriteLine < 8; spriteLine++)
                {
                    var currentSpriteLineStride = lineStride + (spriteLine * bgStride);

                    int spriteLineTileDataIndex = spriteLine * 2;
                    RenderSprite(tileData[spriteLineTileDataIndex], tileData[spriteLineTileDataIndex + 1], x, currentSpriteLineStride, BgPalette, TileBuffer3);
                }

                x += 8;

                // increase the line
                if (i > 0 && (i + 1) % Tile.TileByteSize == 0)
                {
                    x = 0;
                    line += 8;
                }
            }
        }

        private void RenderBackground()
        {
            if (!RenderDebugBuffers)
            {
                return;
            }

            RenderTileBuffer1();
            RenderTileBuffer2();
            RenderTileBuffer3();

            var tileSetToUse = GetTileSet();

            var lcdcByte = Mmu.ReadByte((ushort)GraphicsMemoryFlagLocations.Lcdc);
            var useSByte = (lcdcByte & 0x10) == 0;

            var tileMap = GetTileMap();

            var bgPalette = BgPalette;

            // render tiles to the buffer
            int bgStride = 3 * 32 * 8;
            {
                byte line = 0;
                byte x = 0;
                for (var i = 0; i < tileMap.Length; i++)
                {
                    Tile tile;
                    bool tileFound = false;
                    var tileRef = tileMap[i];

                    if (useSByte)
                    {
                        tileFound = tileSetToUse.TryGetValue((sbyte)(tileRef), out tile);
                    }
                    else
                    {
                        tileFound = tileSetToUse.TryGetValue(tileRef, out tile);
                    }

                    if (tileFound)
                    {
                        for (byte spriteLine = 0; spriteLine < 8; spriteLine++)
                        {
                            var lineStride = (bgStride * line) + (spriteLine * bgStride);
                            var colourBytes = tile.GetRowColourIndexes(spriteLine);
                            RenderSprite(colourBytes, x, lineStride, bgPalette, BgBuffer);
                        }
                    }

                    x += 8;

                    // increase the line
                    if (i > 0 && (i + 1) % 32 == 0)
                    {
                        x = 0;
                        line += 8;
                    }
                }
            }
        }

    }
}
