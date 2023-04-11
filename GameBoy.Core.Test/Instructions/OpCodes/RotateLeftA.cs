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
    public class RotateLeftA : BaseTest
    {

        [Test]
        public void TestRotateLeftA_NoCarry()
        {
            var opCode = new Core.Instructions.OpCodes.RotateLeftA();

            this.SetAllCpuFlags(false);

            this.Cpu.A = 0b0110_0010;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0b1100_0100, this.Cpu.A);
            Assert.IsFalse(this.Cpu.FlagZ);
            Assert.IsFalse(this.Cpu.FlagC);
            Assert.IsFalse(this.Cpu.FlagH);
            Assert.IsFalse(this.Cpu.FlagN);
        }

        [Test]
        public void TestRotateLeftA_AllZero()
        {
            var opCode = new Core.Instructions.OpCodes.RotateLeftA();

            this.SetAllCpuFlags(false);

            this.Cpu.A = 0;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0, this.Cpu.A);
            Assert.IsTrue(this.Cpu.FlagZ);
            Assert.IsFalse(this.Cpu.FlagC);
            Assert.IsFalse(this.Cpu.FlagH);
            Assert.IsFalse(this.Cpu.FlagN);
        }

        [Test]
        public void TestRotateLeftA_Carry()
        {
            var opCode = new Core.Instructions.OpCodes.RotateLeftA();

            this.SetAllCpuFlags(false);

            this.Cpu.A = 0b1110_0010;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0b1100_0100, this.Cpu.A);
            Assert.IsFalse(this.Cpu.FlagZ);
            Assert.IsTrue(this.Cpu.FlagC);
            Assert.IsFalse(this.Cpu.FlagH);
            Assert.IsFalse(this.Cpu.FlagN);
        }

        [Test]
        public void TestRotateLeftA_CarryFromFlag()
        {
            var opCode = new Core.Instructions.OpCodes.RotateLeftA();

            this.SetAllCpuFlags(false);
            Cpu.FlagC = true;

            this.Cpu.A = 0b1110_0010;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0b1100_0101, this.Cpu.A);
            Assert.IsFalse(this.Cpu.FlagZ);
            Assert.IsTrue(this.Cpu.FlagC);
            Assert.IsFalse(this.Cpu.FlagH);
            Assert.IsFalse(this.Cpu.FlagN);
        }

    }
}
