using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameBoy.Core.Test.Instructions.OpCodes
{
    public class JumpByteConditional : BaseTest
    {

        [Test]
        public void TestJumpByteConditional_ZFalse()
        {
            var opCode = new Core.Instructions.OpCodes.JumpByteConditional(0x20, RegisterOperandManager.FlagZNot, MemoryOperandManager.ImmediateSignedByte);
            this.Mmu.WriteByte((ushort)(this.Cpu.ProgramCounter + 1), 0x0F);

            this.SetAllCpuFlags(false);

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0x0F, this.Cpu.ProgramCounter);
            Assert.IsFalse(this.Cpu.FlagZ);
            Assert.IsFalse(this.Cpu.FlagC);
            Assert.IsFalse(this.Cpu.FlagH);
            Assert.IsFalse(this.Cpu.FlagN);
        }

        [Test]
        public void TestJumpByteConditional_ZFalse2()
        {
            var opCode = new Core.Instructions.OpCodes.JumpByteConditional(0x20, RegisterOperandManager.FlagZNot, MemoryOperandManager.ImmediateSignedByte);
            this.Cpu.ProgramCounter = 0xFF;

            this.Mmu.WriteByte((ushort)(this.Cpu.ProgramCounter + 1), 0x0F);

            this.SetAllCpuFlags(false);

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0x10E, this.Cpu.ProgramCounter);
            Assert.IsFalse(this.Cpu.FlagZ);
            Assert.IsFalse(this.Cpu.FlagC);
            Assert.IsFalse(this.Cpu.FlagH);
            Assert.IsFalse(this.Cpu.FlagN);
        }

        [Test]
        public void TestJumpByteConditional_ZFalse_NoJump()
        {
            var opCode = new Core.Instructions.OpCodes.JumpByteConditional(0x20, RegisterOperandManager.FlagZNot, MemoryOperandManager.ImmediateSignedByte);
            this.Mmu.WriteByte((ushort)(this.Cpu.ProgramCounter + 1), 0x0F);

            this.SetAllCpuFlags(true);

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0, this.Cpu.ProgramCounter);
            Assert.IsTrue(this.Cpu.FlagZ);
            Assert.IsTrue(this.Cpu.FlagC);
            Assert.IsTrue(this.Cpu.FlagH);
            Assert.IsTrue(this.Cpu.FlagN);
        }

        [Test]
        public void TestJumpByteConditional_ZTrue()
        {
            var opCode = new Core.Instructions.OpCodes.JumpByteConditional(0x28, RegisterOperandManager.FlagZ, MemoryOperandManager.ImmediateSignedByte);
            this.Mmu.WriteByte((ushort)(this.Cpu.ProgramCounter + 1), 0x0F);

            this.SetAllCpuFlags(true);

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0x0F, this.Cpu.ProgramCounter);
            Assert.IsTrue(this.Cpu.FlagZ);
            Assert.IsTrue(this.Cpu.FlagC);
            Assert.IsTrue(this.Cpu.FlagH);
            Assert.IsTrue(this.Cpu.FlagN);
        }

        [Test]
        public void TestJumpByteConditional_ZTrue2()
        {
            var opCode = new Core.Instructions.OpCodes.JumpByteConditional(0x28, RegisterOperandManager.FlagZ, MemoryOperandManager.ImmediateSignedByte);
            this.Cpu.ProgramCounter = 0xFF;

            this.Mmu.WriteByte((ushort)(this.Cpu.ProgramCounter + 1), 0x0F);

            this.SetAllCpuFlags(true);

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0x10E, this.Cpu.ProgramCounter);
            Assert.IsTrue(this.Cpu.FlagZ);
            Assert.IsTrue(this.Cpu.FlagC);
            Assert.IsTrue(this.Cpu.FlagH);
            Assert.IsTrue(this.Cpu.FlagN);
        }

        [Test]
        public void TestJumpByteConditional_ZTrue_NoJump()
        {
            var opCode = new Core.Instructions.OpCodes.JumpByteConditional(0x28, RegisterOperandManager.FlagZ, MemoryOperandManager.ImmediateSignedByte);
            this.Cpu.ProgramCounter = 0xFF;

            this.Mmu.WriteByte((ushort)(this.Cpu.ProgramCounter + 1), 0x0F);

            this.SetAllCpuFlags(false);

            opCode.Execute(this.Cpu, this.Mmu);

            Assert.AreEqual(0xFF, this.Cpu.ProgramCounter);
            Assert.IsFalse(this.Cpu.FlagZ);
            Assert.IsFalse(this.Cpu.FlagC);
            Assert.IsFalse(this.Cpu.FlagH);
            Assert.IsFalse(this.Cpu.FlagN);
        }
    }
}
