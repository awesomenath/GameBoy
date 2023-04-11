using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions;
using GameBoy.Core.Instructions.Operands;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameBoy.Core.Test
{
    public abstract class BaseTest
    {
        protected Cpu Cpu { get; private set; }
        protected Cartridge Cartridge { get; private set; }
        protected Mmu Mmu { get; private set; }
        protected Gpu Gpu { get; private set; }
        protected RegisterOperandManager RegisterOperandManager { get; private set; }
        protected MemoryOperandManager MemoryOperandManager { get; private set; }
        protected OpCodeManager OpCodeManager { get; private set; }

        [SetUp]
        protected void SetUp()
        {
            Cpu = new Cpu();
            Cartridge = new Cartridge("Dummy.gb");
            Mmu = new Mmu(Cartridge);
            Gpu = new Gpu(Mmu, null);

            RegisterOperandManager = new RegisterOperandManager(this.Cpu);
            MemoryOperandManager = new MemoryOperandManager(this.Cpu, this.Mmu);
            OpCodeManager = new OpCodeManager(this.Cpu, this.RegisterOperandManager, this.MemoryOperandManager);

            Mmu.LeaveBootRom();
        }

        protected void SetAllCpuFlags(bool value = true)
        {
            Cpu.FlagZ = value;
            Cpu.FlagC = value;
            Cpu.FlagN = value;
            Cpu.FlagH = value;
        }
    }
}
