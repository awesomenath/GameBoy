using GameBoy.Core.Hardware;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class Restart : OpCode<byte>
    {
        private byte Address { get; set; }

        public Restart(byte id, byte address) 
            : base(id, 1, 16, OpCodeType.JumpCalls)
        {
            Address = address;
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            cpu.StackPointer -= 2;

            mmu.WriteWord(cpu.StackPointer, (ushort)(cpu.ProgramCounter + Length));

            cpu.ProgramCounter = Address;

            return new OpCodeResult(Length, Cycles, false);
        }
    }
}
