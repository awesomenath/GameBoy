using GameBoy.Core.Hardware;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class Return : OpCode<object>
    {
        public Return() 
            : base(0xC9, 1, 16, OpCodeType.JumpCalls)
        { }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            var result = mmu.ReadWord(cpu.StackPointer);

            cpu.StackPointer += 2;

            cpu.ProgramCounter = result;

            return new OpCodeResult(Length, Cycles, false);
        }
    }
}
