using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class Call : OpCode<ushort>
    {
        public Call(Operand<ushort> memoryOperand) 
            : base(0xCD, 3, 24, OpCodeType.JumpCalls)
        {
            LeftOperand = memoryOperand;
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            cpu.StackPointer -= 2;

            mmu.WriteWord(cpu.StackPointer, (ushort)(cpu.ProgramCounter + Length));
            //mmu.WriteWord(cpu.StackPointer, (ushort)(cpu.ProgramCounter));

            cpu.ProgramCounter = LeftOperand.Get();

            return new OpCodeResult(Length, Cycles, false);
        }
    }
}
