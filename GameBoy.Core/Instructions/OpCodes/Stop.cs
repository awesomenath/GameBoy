using GameBoy.Core.Hardware;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class Stop : OpCode<object>
    {
        public Stop() 
            : base(0x10, 2, 4, OpCodeType.MiscControl)
        { }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            // Stop
            var retVal = base.Execute(cpu, mmu);
            
            retVal.StopRequested = true;
            cpu.IsHalted = true;

            return retVal;
        }
    }
}
