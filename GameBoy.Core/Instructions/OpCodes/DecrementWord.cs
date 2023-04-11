using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class DecrementWord : OpCode<ushort>
    {
        public DecrementWord(byte id, byte length, byte cycles, OpCodeType opCodeType, Operand<ushort> leftOperand) 
            : base(id, length, cycles, opCodeType)
        {
            LeftOperand = leftOperand;
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            // Decrement
            var val = LeftOperand.Get();

            var newVal = (ushort)((val == 0) ? ushort.MaxValue : (val - 1));

            LeftOperand.Set(newVal);

            return base.Execute(cpu, mmu);
        }
    }
}
