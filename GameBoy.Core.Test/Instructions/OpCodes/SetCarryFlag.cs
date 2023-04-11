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
    public class SetCarryFlag : BaseTest
    {

        [Test]
        public void TestSetCarryFlag()
        {
            var opCode = new Core.Instructions.OpCodes.SetCarryFlag();

            this.SetAllCpuFlags();

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.IsTrue(this.Cpu.FlagZ);
            Assert.IsTrue(this.Cpu.FlagC);
            Assert.IsFalse(this.Cpu.FlagH);
            Assert.IsFalse(this.Cpu.FlagN);
        }

        [Test]
        public void TestSetCarryFlag2()
        {
            var opCode = new Core.Instructions.OpCodes.SetCarryFlag();

            this.SetAllCpuFlags(false);

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.IsFalse(this.Cpu.FlagZ);
            Assert.IsTrue(this.Cpu.FlagC);
            Assert.IsFalse(this.Cpu.FlagH);
            Assert.IsFalse(this.Cpu.FlagN);
        }

    }
}
