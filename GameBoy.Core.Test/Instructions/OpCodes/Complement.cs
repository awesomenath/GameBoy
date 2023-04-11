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
    public class Complement : BaseTest
    {

        [Test]
        public void TestComplement()
        {
            var opCode = new Core.Instructions.OpCodes.Complement();

            this.SetAllCpuFlags();

            this.Cpu.A = 0b0101_0101;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0b1010_1010, this.Cpu.A);
            Assert.IsTrue(this.Cpu.FlagZ);
            Assert.IsTrue(this.Cpu.FlagC);
            Assert.IsTrue(this.Cpu.FlagH);
            Assert.IsTrue(this.Cpu.FlagN);
        }

        [Test]
        public void TestComplement2()
        {
            var opCode = new Core.Instructions.OpCodes.Complement();

            this.SetAllCpuFlags(false);
            
            this.Cpu.A = 0b1010_1010;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0b0101_0101, this.Cpu.A);
            Assert.IsFalse(this.Cpu.FlagZ);
            Assert.IsFalse(this.Cpu.FlagC);
            Assert.IsTrue(this.Cpu.FlagH);
            Assert.IsTrue(this.Cpu.FlagN);
        }

        [Test]
        public void TestComplement_0()
        {
            var opCode = new Core.Instructions.OpCodes.Complement();

            this.SetAllCpuFlags();

            this.Cpu.A = 0;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(255, this.Cpu.A);
            Assert.IsTrue(this.Cpu.FlagZ);
            Assert.IsTrue(this.Cpu.FlagC);
            Assert.IsTrue(this.Cpu.FlagH);
            Assert.IsTrue(this.Cpu.FlagN);
        }

        [Test]
        public void TestComplement_Max()
        {
            var opCode = new Core.Instructions.OpCodes.Complement();

            this.SetAllCpuFlags();

            this.Cpu.A = 255;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0, this.Cpu.A);
            Assert.IsTrue(this.Cpu.FlagZ);
            Assert.IsTrue(this.Cpu.FlagC);
            Assert.IsTrue(this.Cpu.FlagH);
            Assert.IsTrue(this.Cpu.FlagN);
        }
    }
}
