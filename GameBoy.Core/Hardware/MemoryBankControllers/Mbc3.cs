using System;

namespace GameBoy.Core.Hardware.MemoryBankControllers
{
    public class Mbc3 : BaseMbc
    {
        private const int RomSize = 2048 * 1024;
        private const int RamSize = 32 * 1024;

        private readonly MemorySpace RomSpace = new("Rom0", 0, 0x3FFF);
        private readonly MemorySpace RomXSpace = new("RomX", 0x4000, 0x7FFF);
        private readonly MemorySpace RamSpace = new("Ram", 0xA000, 0xBFFF);

        private readonly MemorySpace RomBankNumberSpace = new("ROM Bank Number (Write Only)", 0x2000, 0x3FFF);
        private readonly MemorySpace RamBankNumberSpace = new("RAM Bank Number - or - RTC Register Select (Write Only)", 0x4000, 0x5FFF);

        public Mbc3(byte[] rom, bool timer, bool ram, bool battery, string romName = null)
            : base(RomSize, RamSize, battery, romName)
        {
            MbcType = "MBC3";

            if (timer)
            {
                MbcType += "+TIMER";
            }

            if (ram)
            {
                MbcType += "+RAM";
            }

            if (battery)
            {
                MbcType += "+BATTERY";
            }

            InitRom(RomSize, rom);

            TimerPresent = timer;
            RamPresent = ram;
        }

        private void UpdateRomBank(byte romBankNumber)
        {
            CurrentRomBankNumber = (byte)(romBankNumber & 0x7F);
            CurrentRomBankOffset = RomSpace.Size * CurrentRomBankNumber;
        }

        private void UpdateRamBank(byte ramBankNumber)
        {
            CurrentRamBankNumber = (byte)(ramBankNumber & 0x3);
            CurrentRamBankOffset = RamSpace.Size * CurrentRamBankNumber;
        }

        public override byte ReadByte(ushort address)
        {
            if (RomXSpace.AddressContainedWithin(address))
            {
                var relativeRomAddress = CurrentRomBankOffset + address - RomXSpace.Start;

                return Rom[relativeRomAddress];
            }
            else if (RamSpace.AddressContainedWithin(address))
            {
                var relativeRamAddress = CurrentRamBankOffset + address - RamSpace.Start;

                return Ram[relativeRamAddress];
            }

            return Rom[address];
        }

        public override void WriteByte(ushort address, byte value)
        {
            if (RamSpace.AddressContainedWithin(address))
            {
                lock (RamLock)
                {
                    var relativeAddress = CurrentRamBankOffset + address - RamSpace.Start;
                    Ram[relativeAddress] = value;
                    RamDirty = true;
                }
            }
            else if (RomBankNumberSpace.AddressContainedWithin(address))
            {
                UpdateRomBank(value);
            }
            else if (RamBankNumberSpace.AddressContainedWithin(address))
            {
                if (value <= 3)
                {
                    UpdateRamBank(value);
                }
                else if (value >= 0x08 && value >= 0x0C)
                {
                    //TODO: Map RTC register
                }
            }
        }

        protected override void LoadSavedRam(byte[] savedRam)
        {
            Array.Copy(savedRam, Ram, Math.Min(savedRam.Length, Ram.Length));
        }
    }
}
