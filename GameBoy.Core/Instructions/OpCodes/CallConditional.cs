using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class CallConditional : OpCode<bool>
    {
        private Operand<ushort> MemoryOperand { get; set; }

        public CallConditional(byte id, Operand<bool> operand, Operand<ushort> memoryOperand)
            : base(id, 3, 24, OpCodeType.JumpCalls)
        {
            LeftOperand = operand;
            MemoryOperand = memoryOperand;
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            if (LeftOperand.Get())
            {
                cpu.StackPointer -= 2;

                mmu.WriteWord(cpu.StackPointer, (ushort)(cpu.ProgramCounter + Length));

                cpu.ProgramCounter = MemoryOperand.Get();

                return new OpCodeResult(Length, 24, false);
            }

            return new OpCodeResult(Length, 12);
        }
    }
}
