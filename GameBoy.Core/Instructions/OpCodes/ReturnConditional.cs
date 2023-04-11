using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class ReturnConditional : OpCode<bool>
    {
        public ReturnConditional(byte id, Operand<bool> operand)
            : base(id, 1, 8, OpCodeType.JumpCalls)
        {
            LeftOperand = operand;
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            if (LeftOperand.Get())
            {
                var result = mmu.ReadWord(cpu.StackPointer);

                cpu.StackPointer += 2;

                cpu.ProgramCounter = result;

                return new OpCodeResult(Length, 20, false);
            }

            return new OpCodeResult(Length, Cycles);
        }
    }
}
