using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions
{
    public abstract class OpCode<T> : IOpCode
    {
        public byte Id { get; }
        public byte Cycles { get; }
        public byte Length { get; }
        public OpCodeType OpCodeType { get; }
        protected Operand<T> LeftOperand { get; set; }
        protected Operand<T> RightOperand { get; set; }

        private string CalculatedToString { get; set; }

        protected OpCode(byte id, byte length, byte cycles, OpCodeType opCodeType)
        {
            Id = id;
            Length = length;
            Cycles = cycles;
            OpCodeType = opCodeType;
        }

        public virtual OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            cpu.MostRecentOpCode = this;

            return new OpCodeResult(Length, Cycles);
        }

        public override string ToString()
        {
            if (CalculatedToString != null)
            {
                return CalculatedToString;
            }

            var retVal = $"{Id:X} {GetType().Name}";

            if (LeftOperand != null)
            {
                retVal += $" {LeftOperand.Name}";
            }

            if (RightOperand != null)
            {
                retVal += $" {RightOperand.Name}";
            }

            CalculatedToString = retVal;

            return retVal;
        }
    }
}
