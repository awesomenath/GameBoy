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
    public class CompareByte : BaseTest
    {

        [Test]
        public void TestCompareByte_ZeroResult()
        {
            var opCode = new Core.Instructions.OpCodes.CompareByte(0xB8, 1, 4, OpCodeType.ByteMathLogic, RegisterOperandManager.RegisterB);

            this.SetAllCpuFlags();

            this.Cpu.A = 0b0000_0011; // 3
            this.Cpu.B = 0b0000_0011; // 3

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0b0000_0011, this.Cpu.A);
            Assert.IsTrue(this.Cpu.FlagZ);
            Assert.IsTrue(this.Cpu.FlagN);
            Assert.IsTrue(this.Cpu.FlagH);
            Assert.IsTrue(this.Cpu.FlagC);
        }

        [Test]
        public void TestCompareByte_BorrowFromBit4()
        {
            var opCode = new Core.Instructions.OpCodes.CompareByte(0xB8, 1, 4, OpCodeType.ByteMathLogic, RegisterOperandManager.RegisterB);

            this.SetAllCpuFlags(false);

            this.Cpu.A = 0b0001_0000;
            this.Cpu.B = 0b0000_0001;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0b0001_0000, this.Cpu.A);
            Assert.IsFalse(this.Cpu.FlagZ);
            Assert.IsTrue(this.Cpu.FlagN);
            Assert.IsFalse(this.Cpu.FlagH);
            Assert.IsTrue(this.Cpu.FlagC);
        }

        [Test]
        public void TestCompareByte_Borrow()
        {
            var opCode = new Core.Instructions.OpCodes.CompareByte(0xB8, 1, 4, OpCodeType.ByteMathLogic, RegisterOperandManager.RegisterB);

            this.SetAllCpuFlags(false);

            this.Cpu.A = 0b0000_0000;
            this.Cpu.B = 0b0000_0001;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0b0000_0000, this.Cpu.A);
            Assert.IsFalse(this.Cpu.FlagZ);
            Assert.IsTrue(this.Cpu.FlagN);
            Assert.IsTrue(this.Cpu.FlagH);
            Assert.IsFalse(this.Cpu.FlagC);
        }
    }
}
