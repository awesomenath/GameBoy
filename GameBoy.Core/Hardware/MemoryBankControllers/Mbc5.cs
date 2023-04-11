using System;

namespace GameBoy.Core.Hardware.MemoryBankControllers
{
    public class Mbc5 : BaseMbc
    {
        private const int RomSize = 8192 * 1024;
        private const int RamSize = 128 * 1024;

        private readonly MemorySpace RomSpace = new("Rom0", 0, 0x3FFF);
        private readonly MemorySpace RomXSpace = new("RomX", 0x4000, 0x7FFF);
        private readonly MemorySpace RamSpace = new("Ram", 0xA000, 0xBFFF);

        private readonly MemorySpace RamEnableSpace = new("RAM Enable (Write Only)", 0x0000, 0x1FFF);
        private readonly MemorySpace RomBankNumberLowerSpace = new("ROM Bank Number (Write Only)", 0x2000, 0x2FFF);
        private readonly MemorySpace RomBankNumberUpperSpace = new("ROM Bank Number (Write Only)", 0x3000, 0x3FFF);
        private readonly MemorySpace RamBankNumberSpace = new("RAM Bank Number - or - Upper Bits of ROM Bank Number (Write Only))", 0x4000, 0x5FFF);

        private bool ramEnabled = false;

        private byte _romBankNumberLower;
        private byte _romBankNumberUpper;
        private bool _rumble;

        public Mbc5(byte[] rom, bool ram, bool battery, bool rumble, string romName = null)
            : base(RomSize, RamSize, battery, romName)
        {
            MbcType = "Mbc5";

            if (rumble)
            {
                MbcType += "+RUMBLE";
            }

            if (ram)
            {
                MbcType += "+RAM";
            }

            if (battery)
            {
                MbcType += "+BATTERY";
            }

            _rumble = rumble;

            InitRom(RomSize, rom);

            RamPresent = ram;
            CurrentRomBankNumber = 1;
        }

        private void SetRomBank()
        {
            int upper =_romBankNumberUpper << 8;
            ushort newRomBankNumber = (ushort)(upper | _romBankNumberLower);
            //var newRomBankNumber = _romBankNumberLower;

            if (newRomBankNumber != CurrentRomBankNumber)
            {
                CurrentRomBankNumber = newRomBankNumber;
                CurrentRomBankOffset = RomSpace.Size * CurrentRomBankNumber;
                //System.Diagnostics.Debug.WriteLine($"Selected rom bank 0x{newRomBankNumber:x2}, Lower: {_romBankNumberLower:x2}, Upper: {_romBankNumberUpper:x2}");
            }
        }

        private void UpdateRamBank(byte ramBankNumber)
        {
            CurrentRamBankNumber = (byte)(ramBankNumber & 0x0F);
            CurrentRamBankOffset = RamSpace.Size * CurrentRamBankNumber;
            //System.Diagnostics.Debug.WriteLine($"Selected ram bank 0x{CurrentRamBankNumber:x2}, Lower: {_romBankNumberLower:x2}, Upper: {_romBankNumberUpper:x2}");
        }

        public override byte ReadByte(ushort address)
        {
            if (RomXSpace.AddressContainedWithin(address))
            {
                var addressOffset = address - RomXSpace.Start;
                int relativeRomAddress = CurrentRomBankOffset + addressOffset;
                //int relativeRomAddress = (CurrentRomBankOffset | (address & 0x3FFF)) & (Rom.Length - 1);

                while (relativeRomAddress >= Rom.Length)
                {
                    relativeRomAddress -= Rom.Length;
                }

                return Rom[relativeRomAddress];
            }
            else if (RamSpace.AddressContainedWithin(address))
            {
                if (!ramEnabled)
                {
                    return 0xFF;
                }

                int relativeRamAddress = CurrentRamBankOffset + address - RamSpace.Start;                

                return Ram[relativeRamAddress];
            }

            return Rom[address];
        }

        public override void WriteByte(ushort address, byte value)
        {
            if (RamEnableSpace.AddressContainedWithin(address))
            {
                ramEnabled = (value & 0x0F) == 0x0A;
                //System.Diagnostics.Debug.WriteLine($"Ram enabled: {ramEnabled}, value: {value:x2}");
            }
            else if (ramEnabled && RamSpace.AddressContainedWithin(address))
            {
                lock (RamLock)
                {
                    var relativeAddress = CurrentRamBankOffset + address - RamSpace.Start;
                    Ram[relativeAddress] = value;
                    RamDirty = true;
                }
            }
            else if (RomBankNumberLowerSpace.AddressContainedWithin(address))
            {
                _romBankNumberLower = value;
                SetRomBank();
            }
            else if (RomBankNumberUpperSpace.AddressContainedWithin(address))
            {
                _romBankNumberUpper = (byte)(value & 1);
                SetRomBank();
            }
            else if (RamBankNumberSpace.AddressContainedWithin(address))
            {
                UpdateRamBank(value);
            }
        }

        protected override void LoadSavedRam(byte[] savedRam)
        {
            Array.Copy(savedRam, Ram, Math.Min(savedRam.Length, Ram.Length));
        }
    }
}
