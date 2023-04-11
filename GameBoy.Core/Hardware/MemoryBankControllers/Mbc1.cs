using System;

namespace GameBoy.Core.Hardware.MemoryBankControllers
{
    public class Mbc1 : BaseMbc
    {
        private const int RomSize = 2048 * 1024;
        private const int RamSize = 32 * 1024;

        private readonly MemorySpace RomSpace = new("Rom0", 0, 0x3FFF);
        private readonly MemorySpace RomXSpace = new("RomX", 0x4000, 0x7FFF);
        private readonly MemorySpace RamSpace = new("Ram", 0xA000, 0xBFFF);

        private readonly MemorySpace RamEnableSpace = new("RAM Enable (Write Only)", 0x0000, 0x1FFF);
        private readonly MemorySpace RomBankNumberSpace = new("ROM Bank Number (Write Only)", 0x2000, 0x3FFF);
        private readonly MemorySpace RamBankNumberSpace = new("RAM Bank Number - or - Upper Bits of ROM Bank Number (Write Only))", 0x4000, 0x5FFF);
        private readonly MemorySpace BankModeSelectSpace = new("Banking Mode Select (Write Only)", 0x6000, 0x7FFF);

        private bool ramEnabled = false;
        private bool usingSimpleBankingMode = true;

        private byte _romBankNumberLower;
        private byte _romBankNumberUpper;

        public Mbc1(byte[] rom, bool ram, bool battery, string romName = null)
            : base(RomSize, RamSize, battery, romName)
        {
            MbcType = "Mbc1";

            if (ram)
            {
                MbcType += "+RAM";
            }

            if (battery)
            {
                MbcType += "+BATTERY";
            }

            InitRom(RomSize, rom);

            RamPresent = ram;
            CurrentRomBankNumber = 1;
        }

        private void UpdateRomBank(byte romBankNumberLower, byte romBankNumberUpper)
        {
            _romBankNumberLower = (byte)(romBankNumberLower & 0x1F);
            _romBankNumberUpper = (byte)(romBankNumberUpper & 0x03);

            if (_romBankNumberLower == 0)
            {
                _romBankNumberLower = 1; // If the main 5-bit ROM banking register is 0, it reads the bank as if it was set to 1.
            }

            if (usingSimpleBankingMode)
            {
                CurrentRomBankNumber = (byte)((_romBankNumberUpper << 5) | _romBankNumberLower);
            }
            else
            {
                CurrentRomBankNumber = _romBankNumberLower;
            }

            if (CurrentRomBankNumber == 0x20 || CurrentRomBankNumber == 0x40 || CurrentRomBankNumber == 0x60)
            {
                CurrentRomBankNumber++;
            }

            System.Diagnostics.Debug.WriteLine($"Selected rom bank 0x{CurrentRomBankNumber:x2}, Lower: {_romBankNumberLower:x2}, Upper: {_romBankNumberUpper:x2}");

            CurrentRomBankOffset = RomXSpace.Start * CurrentRomBankNumber;
        }

        private void UpdateRamBank(byte ramBankNumber)
        {
            CurrentRamBankNumber = (byte)(ramBankNumber & 0x3);
            CurrentRamBankOffset = RamSpace.Size * CurrentRamBankNumber;
            System.Diagnostics.Debug.WriteLine($"Selected ram bank 0x{CurrentRamBankNumber:x2}, Lower: {_romBankNumberLower:x2}, Upper: {_romBankNumberUpper:x2}");
        }

        public override byte ReadByte(ushort address)
        {
            if (RomXSpace.AddressContainedWithin(address))
            {
                //int relativeRomAddress;
                ////if (romBankNumberLower == 0 && romBankNumberUpper == 0)
                //{
                //    //relativeRomAddress = RomSpace.Size * 1 + address - RomXSpace.Start;
                //}
                ////else
                //{
                //    relativeRomAddress = CurrentRomBankOffset + address - RomXSpace.Start;
                //}

                //var addressOffset = address - RomXSpace.Start;
                //int relativeRomAddress = CurrentRomBankOffset + addressOffset;
                int relativeRomAddress = (CurrentRomBankOffset | (address & 0x3FFF)) & (Rom.Length - 1);

                return Rom[relativeRomAddress];
            }
            else if (RamSpace.AddressContainedWithin(address))
            {
                if (!ramEnabled)
                {
                    return 0xff;
                }

                int relativeRamAddress;
                if (usingSimpleBankingMode)
                {
                    relativeRamAddress = address - RamSpace.Start;
                }
                else
                {
                    relativeRamAddress = CurrentRamBankOffset + address - RamSpace.Start;
                }

                return Ram[relativeRamAddress];
            }

            return Rom[address];
        }

        public override void WriteByte(ushort address, byte value)
        {
            if (RamEnableSpace.AddressContainedWithin(address))
            {
                ramEnabled = (value & 0x0F) == 0x0A;
                System.Diagnostics.Debug.WriteLine($"Ram enabled: {ramEnabled}, value: {value:x2}");
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
            else if (RomBankNumberSpace.AddressContainedWithin(address))
            {
                UpdateRomBank(value, _romBankNumberUpper);
            }
            else if (RamBankNumberSpace.AddressContainedWithin(address))
            {
                if (usingSimpleBankingMode)
                {
                    // ROM Banking Mode
                    UpdateRomBank(_romBankNumberLower, value);
                }
                else if (value <= 3)
                {
                    UpdateRamBank(value);
                }
            }
            else if (BankModeSelectSpace.AddressContainedWithin(address))
            {
                var newModeValue = value == 0;

                if (newModeValue != usingSimpleBankingMode)
                {
                    //if (newModeValue) // Changing to Rom bank mode
                    //{
                    //    UpdateRamBank(0);
                    //    UpdateRomBank(_romBankNumberLower, _romBankNumberUpper);
                    //}
                    //else // Changing to Ram bank mode
                    //{
                    //    UpdateRomBank(_romBankNumberLower, 0);
                    //    UpdateRamBank((byte)(_romBankNumberUpper & 3));
                    //}

                    usingSimpleBankingMode = newModeValue;
                    System.Diagnostics.Debug.WriteLine($"Rom bank mode: {usingSimpleBankingMode}, value: {value:x2}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Rom bank mode remained unchanged: {usingSimpleBankingMode}, value: {value:x2}");
                }
            }
        }

        protected override void LoadSavedRam(byte[] savedRam)
        {
            Array.Copy(savedRam, Ram, Math.Min(savedRam.Length, Ram.Length));
        }
    }
}
