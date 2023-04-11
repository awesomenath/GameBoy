using GameBoy.Core.Hardware;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class Complement : OpCode<object>
    {
        public Complement() 
            : base(0x2F, 1, 4, OpCodeType.ByteMathLogic)
        { }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            cpu.A =(byte) (~cpu.A);

            // Set the flags
            cpu.FlagN = true;
            cpu.FlagH = true; 

            return base.Execute(cpu, mmu);
        }
    }
}
