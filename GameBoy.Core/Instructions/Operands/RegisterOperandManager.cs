using GameBoy.Core.Hardware;

namespace GameBoy.Core.Instructions.Operands
{
    public class RegisterOperandManager
    {
        public Operand<byte> RegisterA { get; private set; }
        public Operand<byte> RegisterB { get; private set; }
        public Operand<byte> RegisterD { get; private set; }

        public Operand<byte> RegisterF { get; private set; }
        public Operand<byte> RegisterC { get; private set; }
        public Operand<byte> RegisterE { get; private set; }

        public Operand<byte> RegisterH { get; private set; }
        public Operand<byte> RegisterL { get; private set; }

        public Operand<ushort> RegisterAF { get; private set; }
        public Operand<ushort> RegisterBC { get; private set; }
        public Operand<ushort> RegisterDE { get; private set; }
        public Operand<ushort> RegisterHL { get; private set; }

        public Operand<ushort> StackPointer { get; private set; }
        public Operand<ushort> ProgramCounter { get; private set; }

        public Operand<bool> FlagZ { get; private set; }
        public Operand<bool> FlagZNot { get; private set; }

        public Operand<bool> FlagC { get; private set; }
        public Operand<bool> FlagCNot { get; private set; }

        public RegisterOperandManager(Cpu cpu)
        {
            InitialiseOperands(cpu);
        }

        private void InitialiseOperands(Cpu cpu)
        {
            RegisterA = new Operand<byte>("Register A", () => cpu.A, val => cpu.A = val);
            RegisterB = new Operand<byte>("Register B", () => cpu.B, val => cpu.B = val);
            RegisterD = new Operand<byte>("Register D", () => cpu.D, val => cpu.D = val);

            RegisterF = new Operand<byte>("Register F", () => cpu.F, val => cpu.F = val);
            RegisterC = new Operand<byte>("Register C", () => cpu.C, val => cpu.C = val);
            RegisterE = new Operand<byte>("Register E", () => cpu.E, val => cpu.E = val);

            RegisterH = new Operand<byte>("Register H", () => cpu.H, val => cpu.H = val);
            RegisterL = new Operand<byte>("Register L", () => cpu.L, val => cpu.L = val);

            RegisterAF = new Operand<ushort>("Register AF", () => cpu.AF, val => cpu.AF = (ushort)((val & 0xFFF0)));
            RegisterBC = new Operand<ushort>("Register BC", () => cpu.BC, val => cpu.BC = val);
            RegisterDE = new Operand<ushort>("Register DE", () => cpu.DE, val => cpu.DE = val);
            RegisterHL = new Operand<ushort>("Register HL", () => cpu.HL, val => cpu.HL = val);

            StackPointer = new Operand<ushort>("StackPointer", () => cpu.StackPointer, val => cpu.StackPointer = val);
            ProgramCounter = new Operand<ushort>("ProgramCounter", () => cpu.ProgramCounter, val => cpu.ProgramCounter = val);

            FlagZ = new Operand<bool>("Flag Z Set", () => cpu.FlagZ, val => cpu.FlagZ = true);
            FlagZNot = new Operand<bool>("Flag Z Not Set", () => !cpu.FlagZ, val => cpu.FlagZ = false);

            FlagC = new Operand<bool>("Flag C Set", () => cpu.FlagC, val => cpu.FlagC = true);
            FlagCNot = new Operand<bool>("Flag C Not Set", () => !cpu.FlagC, val => cpu.FlagC = false);
        }
    }
}
