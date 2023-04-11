namespace GameBoy.Core.Hardware.MemoryBankControllers
{
    public interface IMbc
    {
        string MbcType { get; }

        byte ReadByte(ushort address);
        void WriteByte(ushort address, byte value);
        ushort ReadWord(ushort address);
    }
}
