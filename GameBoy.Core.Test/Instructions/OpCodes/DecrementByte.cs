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
    public class DecrementByte : BaseTest
    {

        [Test]
        public void TestDecrementByte()
        {
            var opCode = new Core.Instructions.OpCodes.DecrementByte(0x05, 1, 4, OpCodeType.ByteMathLogic, RegisterOperandManager.RegisterB);

            this.SetAllCpuFlags();

            this.Cpu.A = 0b1010_1010;
            this.Cpu.B = 0b0110_0010;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0b0110_0001, this.Cpu.B);
            Assert.IsFalse(this.Cpu.FlagZ);
            Assert.IsTrue(this.Cpu.FlagC);
            Assert.IsTrue(this.Cpu.FlagH);
            Assert.IsTrue(this.Cpu.FlagN);
        }

        [Test]
        public void TestDecrementByte_ZeroResult()
        {
            var opCode = new Core.Instructions.OpCodes.DecrementByte(0x05, 1, 4, OpCodeType.ByteMathLogic, RegisterOperandManager.RegisterB);

            this.SetAllCpuFlags();

            this.Cpu.B = 0b0000_0001;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0, this.Cpu.B);
            Assert.IsTrue(this.Cpu.FlagZ);
            Assert.IsTrue(this.Cpu.FlagC);
            Assert.IsTrue(this.Cpu.FlagH);
            Assert.IsTrue(this.Cpu.FlagN);
        }

        [Test]
        public void TestDecrementByte_Zero()
        {
            var opCode = new Core.Instructions.OpCodes.DecrementByte(0x05, 1, 4, OpCodeType.ByteMathLogic, RegisterOperandManager.RegisterB);

            this.SetAllCpuFlags();

            this.Cpu.B = 0b0000_0000;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0b1111_1111, this.Cpu.B);
            Assert.IsFalse(this.Cpu.FlagZ);
            Assert.IsTrue(this.Cpu.FlagC);
            Assert.IsTrue(this.Cpu.FlagH);
            Assert.IsTrue(this.Cpu.FlagN);
        }

        [Test]
        public void TestDecrementByte_BorrowFromBit4()
        {
            var opCode = new Core.Instructions.OpCodes.DecrementByte(0x05, 1, 4, OpCodeType.ByteMathLogic, RegisterOperandManager.RegisterB);

            this.SetAllCpuFlags();

            this.Cpu.B = 0b0111_0000;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0b0110_1111, this.Cpu.B);
            Assert.IsFalse(this.Cpu.FlagZ);
            Assert.IsTrue(this.Cpu.FlagC);
            Assert.IsFalse(this.Cpu.FlagH);
            Assert.IsTrue(this.Cpu.FlagN);
        }
    }
}
