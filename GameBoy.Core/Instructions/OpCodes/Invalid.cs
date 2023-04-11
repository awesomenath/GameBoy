using GameBoy.Core.Hardware;
using System;
using System.Diagnostics;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class Invalid : OpCode<object>
    {
        /// <summary>
        /// Invalid OP code, not supported by the CPU
        /// </summary>
        public Invalid(byte id = 0) 
            : base(id, 0, 0, OpCodeType.MiscControl)
        { }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            // Invalid
            throw new NotImplementedException($"Invalid OP Code 0x{Id:X}");
            Debug.WriteLine($"Invalid OP Code 0x{Id:X}");
            return new OpCodeResult { Cycles = 4, Length = 4 };
        }
    }
}
