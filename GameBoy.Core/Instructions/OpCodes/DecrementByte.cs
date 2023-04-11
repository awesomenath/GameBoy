using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class DecrementByte : OpCode<byte>
    {
        public DecrementByte(byte id, byte length, byte cycles, OpCodeType opCodeType, Operand<byte> leftOperand)
            : base(id, length, cycles, opCodeType)
        {
            LeftOperand = leftOperand;
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            var value = LeftOperand.Get();

            var newValue = value - 1;
            var newValueByte = (byte)newValue;

            cpu.FlagZ = newValueByte == 0;

            cpu.FlagN = true;

            // Set if NO borrow from bit 4
            //if ((value & 0x10) == 0 || ((value & 0x10) == 1 && (newValueByte & 0x10) == 1))
            if ((newValueByte & 0x0F) == 0x0F)
            {
                cpu.FlagH = true;
            }
            else
            {
                cpu.FlagH = false;
            }

            // Set
            LeftOperand.Set(newValueByte);

            return base.Execute(cpu, mmu);
        }
    }
}
