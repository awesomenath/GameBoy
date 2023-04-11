namespace GameBoy.Core.Hardware.MemoryBankControllers
{
    public class NoMbc : BaseMbc
    {
        private const int RomSize = 32 * 1024;
        private const int RamSize = 8 * 1024;

        private readonly MemorySpace RomSpace = new ("Rom", 0, 0x7FFF);
        private readonly MemorySpace RamSpace = new ("Ram", 0xA000, 0xBFFF);

        public NoMbc(byte[] rom)
            : base(RomSize, RamSize, false, null)
        {
            MbcType = "No MBC";

            InitRom(RomSize, rom);
        }

        public override byte ReadByte(ushort address)
        {
            if (RomSpace.AddressContainedWithin(address))
            {
                return base.ReadByte(address);
            }
            else
            {
                return Ram[address - RamSpace.Start];
            }
        }

        public override void WriteByte(ushort address, byte value)
        {
            if (RamSpace.AddressContainedWithin(address))
            {
                var relativeAddress = address - RamSpace.Start;
                Ram[relativeAddress] = value;
            }
        }

        protected override void LoadSavedRam(byte[] savedRam)
        {
            // no saved RAM
        }
    }
}
