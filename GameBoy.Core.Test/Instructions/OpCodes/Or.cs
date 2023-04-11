﻿using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions;
using GameBoy.Core.Instructions.OpCodes;
using GameBoy.Core.Instructions.Operands;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameBoy.Core.Test.Instructions.OpCodes
{
    public class Or : BaseTest
    {
        [Test]
        public void TestOr()
        {
            var opCode = new Core.Instructions.OpCodes.Or(0xB0, 1, 4, OpCodeType.ByteMathLogic, RegisterOperandManager.RegisterB);

            this.SetAllCpuFlags();

            this.Cpu.A = 0b1010_1010;
            this.Cpu.B = 0b0101_0101;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0b1111_1111, this.Cpu.A);
            Assert.IsFalse(this.Cpu.FlagZ);
            Assert.IsFalse(this.Cpu.FlagC);
            Assert.IsFalse(this.Cpu.FlagH);
            Assert.IsFalse(this.Cpu.FlagN);
        }

        [Test]
        public void TestOr2()
        {
            var opCode = new Core.Instructions.OpCodes.Or(0xB0, 1, 4, OpCodeType.ByteMathLogic, RegisterOperandManager.RegisterB);

            this.SetAllCpuFlags();

            this.Cpu.A = 0;
            this.Cpu.B = 0;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0, this.Cpu.A);
            Assert.IsTrue(this.Cpu.FlagZ);
            Assert.IsFalse(this.Cpu.FlagC);
            Assert.IsFalse(this.Cpu.FlagH);
            Assert.IsFalse(this.Cpu.FlagN);
        }

    }
}
