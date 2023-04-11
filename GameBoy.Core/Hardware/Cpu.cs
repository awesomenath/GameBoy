namespace GameBoy.Core.Hardware
{
    public class Cpu
    {
        public const byte CFlag = 0x10;
        public const byte HFlag = 0x20;
        public const byte NFlag = 0x40;
        public const byte ZFlag = 0x80;
        public const byte NotCFlag = 0xE0;
        public const byte NotHFlag = 0xD0;
        public const byte NotNFlag = 0xB0;
        public const byte NotZFlag = 0x70;

        public byte A { get; set; }
        public byte B { get; set; }
        public byte D { get; set; }

        public byte F { get; set; }
        public byte C { get; set; }
        public byte E { get; set; }

        public byte H { get; set; }
        public byte L { get; set; }

        public Instructions.IOpCode MostRecentOpCode { get; set; }

        public bool InterruptsEnabled { get; set; }
        public bool IsHalted { get; set; }

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

        public ushort StackPointer { get; set; }
        public ushort ProgramCounter { get; set; }

        /// <summary>
        /// Math Operation resulted in zero
        /// </summary>
        public bool FlagZ
        {
            get
            {
                return (F & 0x80) != 0;
            }
            set
            {
                if (value)
                {
                    F = (byte)(F | ZFlag);
                }
                else
                {
                    F = (byte)(F & NotZFlag);
                }
            }
        }

        /// <summary>
        /// Math Operation used subtraction
        /// </summary>
        public bool FlagN
        {
            get
            {
                return (F & 0x40) != 0;
            }
            set
            {
                if (value)
                {
                    F = (byte)(F | NFlag);
                }
                else
                {
                    F = (byte)(F & NotNFlag);
                }
            }
        }

        /// <summary>
        /// Math Operation raised half carry
        /// </summary>
        public bool FlagH
        {
            get
            {
                return (F & 0x20) != 0;
            }
            set
            {
                if (value)
                {
                    F = (byte)(F | HFlag);
                }
                else
                {
                    F = (byte)(F & NotHFlag);
                }
            }
        }

        /// <summary>
        /// Math Operation raised carry
        /// </summary>
        public bool FlagC
        {
            get
            {
                return (F & 0x10) != 0;
            }
            set
            {
                if (value)
                {
                    F = (byte)(F | CFlag);
                }
                else
                {
                    F = (byte)(F & NotCFlag);
                }
            }
        }

        public Cpu()
        {
            StackPointer = 0xFFFE;
        }

        public void IncrementProgramCounter()
        {
            ProgramCounter++;
        }

        public CpuState GetState()
        {
            var retVal = new CpuState()
            {
                AF = AF,
                BC = BC,
                DE = DE,
                HL = HL,
                ProgramCounter = ProgramCounter,
                StackPointer = StackPointer,
                OpCode = MostRecentOpCode
            };

            return retVal;
        }
    }
}
