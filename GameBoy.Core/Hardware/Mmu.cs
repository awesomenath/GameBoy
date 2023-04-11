using GameBoy.Core.Hardware.MemoryBankControllers;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameBoy.Core.Hardware
{
    public class Mmu
    {
        public MemorySpace RomBank0 = new MemorySpace("Rom Bank 0", 0, 0x3FFF);
        public MemorySpace Bios = new MemorySpace("BIOS", 0, 0x00FF);
        public MemorySpace CartridgeHeader = new MemorySpace("CartridgeHeader", 0x0100, 0x014F);
        public MemorySpace RomBankX = new MemorySpace("Rom Bank X", 0x4000, 0x7FFF);
        public MemorySpace VRam = new MemorySpace("GPU Vram", 0x8000, 0x9FFF);
        public MemorySpace TilePatternTable1 = new MemorySpace("Tile Pattern Table, for Sprites, BG, Window, Tiles #1 0 -> 127", 0x8000, 0x87FF);
        public MemorySpace TilePatternTable1Full = new MemorySpace("Tile Pattern Table, for Sprites, BG, Window, Tiles #1 0 -> 255", 0x8000, 0x8FFF);
        public MemorySpace TilePatternTable2 = new MemorySpace("Tile Pattern Table, for BG, Window, Tiles #1 128 -> 255, #0 -1 -> -128", 0x8800, 0x8FFF);
        public MemorySpace TilePatternTable3 = new MemorySpace("Tile Pattern Table, for BG, Window, Tiles #0 0 -> 127", 0x9000, 0x97FF);
        public MemorySpace TilePatternTable2Full = new MemorySpace("Tile Pattern Table, for BG, Window, Tiles #1 -128 -> 127", 0x8800, 0x97FF);
        public MemorySpace BgTileMap = new MemorySpace("BG Tile Map #0", 0x9800, 0x9BFF);
        public MemorySpace BgTileMap2 = new MemorySpace("BG Tile Map #1", 0x9C00, 0x9FFF);
        public MemorySpace Ram = new MemorySpace("Working RAM", 0xC000, 0xDFFF);
        public MemorySpace ShadowRam = new MemorySpace("Working RAM (Shadow)", 0xE000, 0xFDFF);
        public MemorySpace GraphicSpriteInformation = new MemorySpace("Graphics Sprite Information, OAM - Object Attribute Memory", 0xFE00, 0xFE9F);
        public MemorySpace Io = new MemorySpace("Memory Mapped Io", 0xFF00, 0xFF7F);
        public MemorySpace ZeroPageRam = new MemorySpace("Zero Page Ram", 0xFF80, 0xFFFE);
        public MemorySpace InterruptFlags = new MemorySpace("IF Interrupt Flags", 0xFF0F, 0xFF0F);

        public MemorySpace RamEnable = new MemorySpace("RAM Enable", 0, 0x1FFF);
        public MemorySpace RomBankNumber = new MemorySpace("ROM Bank Number", 0x2000, 0x3FFF);
        public MemorySpace RomRamBankNumber = new MemorySpace("RAM Bank Number or Upper Bits of ROM Bank Number", 0x4000, 0x5FFF);
        public MemorySpace RomRamModeSelect = new MemorySpace("ROM/Ram Mode Select", 0x6000, 0x7FFF);

        public MemorySpace CartridgeMemory = new MemorySpace("Cart memory", 0, 0x7FFF);
        public MemorySpace CartridgeRam = new MemorySpace("Cartridge (External) RAM", 0xA000, 0xBFFF);

        private List<MemorySpace> MemorySpaces;
        private List<MemorySpace> MemorySpacesWithEvents = new List<MemorySpace>();

        public bool InBootRom { get; private set; }

        private byte[] Memory = new byte[ushort.MaxValue + 1];

        // https://github.com/Hacktix/Bootix
        // xxd -g 1 -u -c 256 bootix_dmg.bin
        private readonly byte[] BootRom = new byte[]{
            0x31, 0xFE, 0xFF, 0x21, 0xFF, 0x9F, 0xAF, 0x32, 0xCB, 0x7C, 0x20, 0xFA, 0x0E, 0x11, 0x21, 0x26, 
            0xFF, 0x3E, 0x80, 0x32, 0xE2, 0x0C, 0x3E, 0xF3, 0x32, 0xE2, 0x0C, 0x3E, 0x77, 0x32, 0xE2, 0x11, 
            0x04, 0x01, 0x21, 0x10, 0x80, 0x1A, 0xCD, 0xB8, 0x00, 0x1A, 0xCB, 0x37, 0xCD, 0xB8, 0x00, 0x13, 
            0x7B, 0xFE, 0x34, 0x20, 0xF0, 0x11, 0xCC, 0x00, 0x06, 0x08, 0x1A, 0x13, 0x22, 0x23, 0x05, 0x20, 
            0xF9, 0x21, 0x04, 0x99, 0x01, 0x0C, 0x01, 0xCD, 0xB1, 0x00, 0x3E, 0x19, 0x77, 0x21, 0x24, 0x99, 
            0x0E, 0x0C, 0xCD, 0xB1, 0x00, 0x3E, 0x91, 0xE0, 0x40, 0x06, 0x10, 0x11, 0xD4, 0x00, 0x78, 0xE0, 
            0x43, 0x05, 0x7B, 0xFE, 0xD8, 0x28, 0x04, 0x1A, 0xE0, 0x47, 0x13, 0x0E, 0x1C, 0xCD, 0xA7, 0x00, 
            0xAF, 0x90, 0xE0, 0x43, 0x05, 0x0E, 0x1C, 0xCD, 0xA7, 0x00, 0xAF, 0xB0, 0x20, 0xE0, 0xE0, 0x43, 
            0x3E, 0x83, 0xCD, 0x9F, 0x00, 0x0E, 0x27, 0xCD, 0xA7, 0x00, 0x3E, 0xC1, 0xCD, 0x9F, 0x00, 0x11, 
            0x8A, 0x01, 0xF0, 0x44, 0xFE, 0x90, 0x20, 0xFA, 0x1B, 0x7A, 0xB3, 0x20, 0xF5, 0x18, 0x49, 0x0E, 
            0x13, 0xE2, 0x0C, 0x3E, 0x87, 0xE2, 0xC9, 0xF0, 0x44, 0xFE, 0x90, 0x20, 0xFA, 0x0D, 0x20, 0xF7, 
            0xC9, 0x78, 0x22, 0x04, 0x0D, 0x20, 0xFA, 0xC9, 0x47, 0x0E, 0x04, 0xAF, 0xC5, 0xCB, 0x10, 0x17, 
            0xC1, 0xCB, 0x10, 0x17, 0x0D, 0x20, 0xF5, 0x22, 0x23, 0x22, 0x23, 0xC9, 0x3C, 0x42, 0xB9, 0xA5, 
            0xB9, 0xA5, 0x42, 0x3C, 0x00, 0x54, 0xA8, 0xFC, 0x42, 0x4F, 0x4F, 0x54, 0x49, 0x58, 0x2E, 0x44, 
            0x4D, 0x47, 0x20, 0x76, 0x31, 0x2E, 0x32, 0x00, 0x3E, 0xFF, 0xC6, 0x01, 0x0B, 0x1E, 0xD8, 0x21, 
            0x4D, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3E, 0x01, 0xE0, 0x50
        };

        //private byte[] gameBoyRom;
        private IMbc memoryBankController; 

        public Mmu(Cartridge gameBoyCatridge)
        {
            InBootRom = true;

            memoryBankController = gameBoyCatridge.MemoryBankController;

            // HACK: FF4D KEY1 register, set to 0xFF to bypass speed switch attempts for CGB https://gbdev.gg8.se/wiki/articles/CGB_Registers#FF4D_-_KEY1_-_CGB_Mode_Only_-_Prepare_Speed_Switch
            Memory[0xFF4D] = 0xFF;

            //// Copy the gameboy rom into RomBank0
            //for (var i = 0; i < RomBank0.End; i++)
            //{
            //    Memory[i] = gameBoyRom[i];
            //}

            //for (var i = RomBankX.Start; i < RomBankX.End; i++)
            //{
            //    Memory[i] = gameBoyRom[i];
            //}

            MemorySpaces = new List<MemorySpace>()
            {
                RomBank0,
                Bios,
                CartridgeHeader,
                RomBankX,
                VRam,
                TilePatternTable1,
                TilePatternTable1Full,
                TilePatternTable2,
                TilePatternTable2Full,
                TilePatternTable3,
                BgTileMap,
                BgTileMap2,
                CartridgeRam,
                Ram,
                ShadowRam,
                GraphicSpriteInformation,
                Io,
                ZeroPageRam,
                RamEnable,
                RomBankNumber,
                RomRamBankNumber,
                RomRamModeSelect
            };






        }

        public void LeaveBootRom()
        {
            InBootRom = false;

            Debug.WriteLine("Left boot rom.");
        }

        public byte ReadByte(ushort address)
        {
            if (InBootRom && address < 0x0100)
            {
                return BootRom[address];
            }

            // Forward to cartridge MBC
            if (CartridgeMemory.AddressContainedWithin(address) || CartridgeRam.AddressContainedWithin(address))
            {
                return memoryBankController.ReadByte(address);
            }

            return Memory[address];            
        }

        public Span<byte> GetSpan(MemorySpace memorySpace)
        {
            return Memory.AsSpan(memorySpace.Start, memorySpace.Size);
        }

        public Span<byte> GetSpan(ushort start, ushort length)
        {
            return Memory.AsSpan(start, length);
        }

        public void WriteSection(ushort start, Span<byte> values)
        {
            for (var i = 0; i < values.Length; i++)
            {
                Memory[start + i] = values[i];
            }
        }

        public ushort ReadWord(ushort address)
        {
            return (ushort)((ReadByte((ushort)(address + 1)) << 8) | ReadByte(address));

            //if (InBootRom)
            //{
            //    return (ushort)((BootRom[address + 1] << 8) | BootRom[address]);
            //}
            //else
            //{
            //    //return (ushort)((Memory[address] << 8) | Memory[address + 1]);
            //    return (ushort)((ReadByte((ushort)(address + 1)) << 8) | ReadByte(address));
            //}           
        }

        public void WriteByte(ushort address, byte value)
        {
            // Forward to cartridge MBC
            if (CartridgeMemory.AddressContainedWithin(address) || CartridgeRam.AddressContainedWithin(address))
            {
                memoryBankController.WriteByte(address, value);
            }
            else 
            // Ignore special writes
            //if (!HandleSpecialWrite(address, value))
            {
                Memory[address] = value;
            }

            TriggerWriteEvents(address);
        }

        public void WriteByte(ushort address, byte value, bool skipWriteEvents)
        {
            if (CartridgeMemory.AddressContainedWithin(address) || CartridgeRam.AddressContainedWithin(address))
            {
                memoryBankController.WriteByte(address, value);
            }
            else 
            // Ignore special writes
            //if (!HandleSpecialWrite(address, value))
            {
                Memory[address] = value;
            }

            if (!skipWriteEvents)
            {
                TriggerWriteEvents(address);
            }
        }

        public void WriteWord(ushort address, ushort value)
        {
            //WriteByte(address, (byte)(value >> 8));
            //WriteByte((ushort)(address + 1), (byte)(value));
            WriteByte((ushort)(address + 1), (byte)(value >> 8));
            WriteByte((ushort)(address), (byte)(value));
            //Memory[address] = (byte)(value >> 8);
            //Memory[address + 1] = (byte)(value);

            //TriggerWriteEvents(address);
        }

        private void TriggerWriteEvents(ushort address)
        {
            // TODO: Improve iteration? binary search?
            foreach (var memorySpace in MemorySpacesWithEvents)
            {
                if (memorySpace.AddressContainedWithin(address))
                {
                    memorySpace.TriggerWriteEvent(address);
                }
            }

            // HACK to write serial data
            if (address == 0xFF02 && (Memory[0xFF02] & 0x80) != 0)
            {
                char c = (char)Memory[0xFF01];
                Debug.Write(c);
                Memory[0xFF02] = 0;
                // TODO: Jump to 0x0058
            }
        }

        public void SubscribeToMemorySpaceEvents(MemorySpace memorySpace, MemorySpace.MemoryWriteEvent memoryWriteEvent)
        {
            memorySpace.Subscribe(memoryWriteEvent);

            if (!MemorySpacesWithEvents.Contains(memorySpace))
            {
                MemorySpacesWithEvents.Add(memorySpace);
            }
        }
    }
}
