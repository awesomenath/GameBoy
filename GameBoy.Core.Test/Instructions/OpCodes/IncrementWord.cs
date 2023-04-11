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
    public class IncrementWord : BaseTest
    {

        [Test]
        public void TestIncrementWord()
        {
            var opCode = new Core.Instructions.OpCodes.IncrementWord(0x03, 1, 8, OpCodeType.WordMathLogic, RegisterOperandManager.RegisterBC);

            this.SetAllCpuFlags();

            this.Cpu.BC = 0b0000_0000_0110_0010;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0b0110_0011, this.Cpu.BC);
            Assert.IsTrue(this.Cpu.FlagZ);
            Assert.IsTrue(this.Cpu.FlagC);
            Assert.IsTrue(this.Cpu.FlagH);
            Assert.IsTrue(this.Cpu.FlagN);
        }

        [Test]
        public void TestIncrementWord_ZeroResult()
        {
            var opCode = new Core.Instructions.OpCodes.IncrementWord(0x03, 1, 8, OpCodeType.WordMathLogic, RegisterOperandManager.RegisterBC);

            this.SetAllCpuFlags(false);

            this.Cpu.BC = 0b1111_1111_1111_1111;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0, this.Cpu.BC);
            Assert.IsFalse(this.Cpu.FlagZ);
            Assert.IsFalse(this.Cpu.FlagC);
            Assert.IsFalse(this.Cpu.FlagH);
            Assert.IsFalse(this.Cpu.FlagN);
        }
    }
}
