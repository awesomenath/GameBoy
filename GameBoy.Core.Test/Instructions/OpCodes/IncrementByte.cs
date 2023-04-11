using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions;
using GameBoy.Core.Instructions.OpCodes;
using GameBoy.Core.Instructions.Operands;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameBoy.Core.Test.Instructions.OpCodes
{
    public class IncrementByte : BaseTest
    {

        [Test]
        public void TestIncrementByte()
        {
            var opCode = new Core.Instructions.OpCodes.IncrementByte(0x0C, 1, 4, OpCodeType.ByteMathLogic, RegisterOperandManager.RegisterB);

            this.SetAllCpuFlags();

            this.Cpu.A = 0b1010_1010;
            this.Cpu.B = 0b0110_0010;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0b0110_0011, this.Cpu.B);
            Assert.IsFalse(this.Cpu.FlagZ);
            Assert.IsTrue(this.Cpu.FlagC);
            Assert.IsFalse(this.Cpu.FlagH);
            Assert.IsFalse(this.Cpu.FlagN);
        }

        [Test]
        public void TestIncrementByte_ZeroResult()
        {
            var opCode = new Core.Instructions.OpCodes.IncrementByte(0x0C, 1, 4, OpCodeType.ByteMathLogic, RegisterOperandManager.RegisterB);

            this.SetAllCpuFlags();

            this.Cpu.B = 0b1111_1111;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0, this.Cpu.B);
            Assert.IsTrue(this.Cpu.FlagZ);
            Assert.IsTrue(this.Cpu.FlagC);
            Assert.IsFalse(this.Cpu.FlagH);
            Assert.IsFalse(this.Cpu.FlagN);
        }

        [Test]
        public void TestIncrementByte_CarryFromBit3()
        {
            var opCode = new Core.Instructions.OpCodes.IncrementByte(0x0C, 1, 4, OpCodeType.ByteMathLogic, RegisterOperandManager.RegisterB);

            this.SetAllCpuFlags();

            this.Cpu.B = 0b0000_1111;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0b0001_0000, this.Cpu.B);
            Assert.IsFalse(this.Cpu.FlagZ);
            Assert.IsTrue(this.Cpu.FlagC);
            Assert.IsTrue(this.Cpu.FlagH);
            Assert.IsFalse(this.Cpu.FlagN);
        }
    }
}
