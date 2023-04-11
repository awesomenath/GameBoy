using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class Push : OpCode<ushort>
    {
        public Push(byte id, Operand<ushort> leftOperand) 
            : base(id, 1, 16, OpCodeType.WordLoad)
        {
            LeftOperand = leftOperand;
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            cpu.StackPointer -= 2;

            mmu.WriteWord(cpu.StackPointer, LeftOperand.Get());

            return base.Execute(cpu, mmu);
        }
    }
}
