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
    public class And : BaseTest
    {

        [Test]
        public void TestAnd()
        {
            var opCode = new Core.Instructions.OpCodes.And(0xA0, 1, 4, OpCodeType.ByteMathLogic, RegisterOperandManager.RegisterB);

            this.SetAllCpuFlags();

            this.Cpu.A = 0b1010_1010;
            this.Cpu.B = 0b0110_0010;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0b0010_0010, this.Cpu.A);
            Assert.IsFalse(this.Cpu.FlagZ);
            Assert.IsFalse(this.Cpu.FlagC);
            Assert.IsTrue(this.Cpu.FlagH);
            Assert.IsFalse(this.Cpu.FlagN);
        }

        [Test]
        public void TestAnd2()
        {
            var opCode = new Core.Instructions.OpCodes.And(0xA0, 1, 4, OpCodeType.ByteMathLogic, RegisterOperandManager.RegisterB);

            this.SetAllCpuFlags();

            this.Cpu.A = 0;
            this.Cpu.B = 0;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0, this.Cpu.A);
            Assert.IsTrue(this.Cpu.FlagZ);
            Assert.IsFalse(this.Cpu.FlagC);
            Assert.IsTrue(this.Cpu.FlagH);
            Assert.IsFalse(this.Cpu.FlagN);
        }

    }
}
