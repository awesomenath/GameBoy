using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameBoy.Core.Test.Instructions.OpCodes
{
    public class Push : BaseTest
    {
        [Test]
        public void TestPush()
        {
            var opCode = new Core.Instructions.OpCodes.Push(0xC5, RegisterOperandManager.RegisterBC);
            var originalStackPointer = this.Cpu.StackPointer;

            this.SetAllCpuFlags(false);

            this.Cpu.BC = 0xD5E2;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(originalStackPointer - 2, this.Cpu.StackPointer);
            Assert.AreEqual(0xD5E2, this.Cpu.BC);
            Assert.AreEqual(0xD5E2, this.Mmu.ReadWord(this.Cpu.StackPointer));
            Assert.IsFalse(this.Cpu.FlagZ);
            Assert.IsFalse(this.Cpu.FlagC);
            Assert.IsFalse(this.Cpu.FlagH);
            Assert.IsFalse(this.Cpu.FlagN);
        }

        [Test]
        public void TestPush_FlagUnchanged()
        {
            var opCode = new Core.Instructions.OpCodes.Push(0xC5, RegisterOperandManager.RegisterBC);
            var originalStackPointer = this.Cpu.StackPointer;

            this.SetAllCpuFlags(true);

            this.Cpu.BC = 0xD5E2;

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(originalStackPointer - 2, this.Cpu.StackPointer);
            Assert.AreEqual(0xD5E2, this.Cpu.BC);
            Assert.AreEqual(0xD5E2, this.Mmu.ReadWord(this.Cpu.StackPointer));
            Assert.IsTrue(this.Cpu.FlagZ);
            Assert.IsTrue(this.Cpu.FlagC);
            Assert.IsTrue(this.Cpu.FlagH);
            Assert.IsTrue(this.Cpu.FlagN);
        }
    }
}
