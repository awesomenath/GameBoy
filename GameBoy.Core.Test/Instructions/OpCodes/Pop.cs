using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameBoy.Core.Test.Instructions.OpCodes
{
    public class Pop : BaseTest
    {
        [Test]
        public void TestPop()
        {
            var opCode = new Core.Instructions.OpCodes.Pop(0xC1, RegisterOperandManager.RegisterBC);
            var originalStackPointer = this.Cpu.StackPointer;

            this.SetAllCpuFlags(false);

            this.Mmu.WriteWord(0xFFFE, 0xD5E2);

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual((ushort)(originalStackPointer + 2), this.Cpu.StackPointer);
            Assert.AreEqual(0xD5E2, this.Cpu.BC);
            Assert.IsFalse(this.Cpu.FlagZ);
            Assert.IsFalse(this.Cpu.FlagC);
            Assert.IsFalse(this.Cpu.FlagH);
            Assert.IsFalse(this.Cpu.FlagN);
        }

        [Test]
        public void TestPop_FlagUnchanged()
        {
            var opCode = new Core.Instructions.OpCodes.Pop(0xC1, RegisterOperandManager.RegisterBC);
            var originalStackPointer = this.Cpu.StackPointer;

            this.SetAllCpuFlags(true);

            this.Mmu.WriteWord(0xFFFE, 0xD5E2);

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual((ushort)(originalStackPointer + 2), this.Cpu.StackPointer);
            Assert.AreEqual(0xD5E2, this.Cpu.BC);
            Assert.IsTrue(this.Cpu.FlagZ);
            Assert.IsTrue(this.Cpu.FlagC);
            Assert.IsTrue(this.Cpu.FlagH);
            Assert.IsTrue(this.Cpu.FlagN);
        }
    }
}
