using GameBoy.Core.Hardware;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class DecimalAdjust : OpCode<object>
    {
        public DecimalAdjust() 
            : base(0x27, 1, 4, OpCodeType.ByteMathLogic)
        { }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            // https://forums.nesdev.com/viewtopic.php?t=15944
            if (!cpu.FlagN)
            {
                // after an addition, adjust if (half-)carry occurred or if result is out of bounds
                if (cpu.FlagC || cpu.A > 0x99)
                {
                    cpu.A += 0x60;
                    cpu.FlagC = true;
                }

                if (cpu.FlagH || (cpu.A & 0x0F) > 0x09)
                {
                    cpu.A += 0x06;
                }
            }
            else
            {
                // after a subtraction, only adjust if (half-)carry occurred
                if (cpu.FlagC)
                {
                    cpu.A -= 0x60;
                }
                
                if (cpu.FlagH)
                {
                    cpu.A -= 0x06;
                }
            }

            cpu.FlagZ = cpu.A == 0;

            cpu.FlagH = false;

            return base.Execute(cpu, mmu);
        }
    }
}
