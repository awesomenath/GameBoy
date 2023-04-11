using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class CompareByte : OpCode<byte>
    {
        public CompareByte(byte id, byte length, byte cycles, OpCodeType opCodeType, Operand<byte> leftOperand) 
            : base(id, length, cycles, opCodeType)
        {
            LeftOperand = leftOperand;
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            // Subtract
            var operandValue = LeftOperand.Get();

            if (cpu.A < operandValue)
            {
                cpu.FlagC = true; // Made Carry
            }
            else
            {
                cpu.FlagC = false; // Did not make Carry
            }

            // Set if NO borrow from bit 4
            //if ((cpu.A & 0x10) == 0 || ((cpu.A & 0x10) == 1 && (result & 0x10) == 1))
            if ((cpu.A & 0xF) < (operandValue & 0xF))
            {
                cpu.FlagH = true;
            }
            else
            {
                cpu.FlagH = false;
            }

            if (operandValue == cpu.A)
            {
                cpu.FlagZ = true;
            }
            else
            {
                cpu.FlagZ = false;
            }

            cpu.FlagN = true; // Subtract

            //LeftOperand.Set(resultByte);

            return base.Execute(cpu, mmu);
        }
    }
}
