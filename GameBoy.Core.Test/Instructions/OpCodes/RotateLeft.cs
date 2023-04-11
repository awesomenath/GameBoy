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
    public class RotateLeft : BaseTest
    {

        [Test]
        public void TestRotateLeft_NoCarry()
        {
            var opCode = new Core.Instructions.OpCodes.RotateLeft(0x10, 2, 8, RegisterOperandManager.RegisterB);

            this.SetAllCpuFlags(false);

            this.Cpu.A = 0b1010_1010;
            this.Cpu.B = 0b0110_0010;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0b1100_0100, this.Cpu.B);
            Assert.IsFalse(this.Cpu.FlagZ);
            Assert.IsFalse(this.Cpu.FlagC);
            Assert.IsFalse(this.Cpu.FlagH);
            Assert.IsFalse(this.Cpu.FlagN);
        }

        [Test]
        public void TestRotateLeft_AllZero()
        {
            var opCode = new Core.Instructions.OpCodes.RotateLeft(0x10, 2, 8, RegisterOperandManager.RegisterB);

            this.SetAllCpuFlags(false);

            this.Cpu.A = 0;
            this.Cpu.B = 0;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0, this.Cpu.B);
            Assert.IsTrue(this.Cpu.FlagZ);
            Assert.IsFalse(this.Cpu.FlagC);
            Assert.IsFalse(this.Cpu.FlagH);
            Assert.IsFalse(this.Cpu.FlagN);
        }

        [Test]
        public void TestRotateLeft_Carry()
        {
            var opCode = new Core.Instructions.OpCodes.RotateLeft(0x10, 2, 8, RegisterOperandManager.RegisterB);

            this.SetAllCpuFlags(false);

            this.Cpu.A = 0b1010_1010;
            this.Cpu.B = 0b1110_0010;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0b1100_0100, this.Cpu.B);
            Assert.IsFalse(this.Cpu.FlagZ);
            Assert.IsTrue(this.Cpu.FlagC);
            Assert.IsFalse(this.Cpu.FlagH);
            Assert.IsFalse(this.Cpu.FlagN);
        }

        [Test]
        public void TestRotateLeft_CarryFromFlag()
        {
            var opCode = new Core.Instructions.OpCodes.RotateLeft(0x10, 2, 8, RegisterOperandManager.RegisterB);

            this.SetAllCpuFlags(false);
            Cpu.FlagC = true;

            this.Cpu.A = 0b1010_1010;
            this.Cpu.B = 0b1110_0010;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0b1100_0101, this.Cpu.B);
            Assert.IsFalse(this.Cpu.FlagZ);
            Assert.IsTrue(this.Cpu.FlagC);
            Assert.IsFalse(this.Cpu.FlagH);
            Assert.IsFalse(this.Cpu.FlagN);
        }

    }
}
