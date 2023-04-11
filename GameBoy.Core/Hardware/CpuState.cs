namespace GameBoy.Core.Hardware
{
    public struct CpuState
    {
        public byte A { get; set; }
        public byte B { get; set; }
        public byte D { get; set; }

        public byte F { get; set; }
        public byte C { get; set; }
        public byte E { get; set; }

        public byte H { get; set; }
        public byte L { get; set; }

        public ushort StackPointer { get; set; }
        public ushort ProgramCounter { get; set; }

        public Instructions.IOpCode OpCode { get; set; }

        public ushort AF
        {
            get
            {
                return (ushort)((A << 8) | F);
            }
            set
            {
                A = (byte)(value >> 8);
                F = (byte)(value);
            }
        }

        public ushort BC
        {
            get
            {
                return (ushort)((B << 8) | C);
            }
            set
            {
                B = (byte)(value >> 8);
                C = (byte)(value);
            }
        }

        public ushort DE
        {
            get
            {
                return (ushort)((D << 8) | E);
            }
            set
            {
                D = (byte)(value >> 8);
                E = (byte)(value);
            }
        }

        public ushort HL
        {
            get
            {
                return (ushort)((H << 8) | L);
            }
            set
            {
                H = (byte)(value >> 8);
                L = (byte)(value);
            }
        }

        public override string ToString()
        {
            return OpCode.ToString();
        }
    }
}
