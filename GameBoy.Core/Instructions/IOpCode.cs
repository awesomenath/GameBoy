using GameBoy.Core.Hardware;

namespace GameBoy.Core.Instructions
{
    public interface IOpCode
    {
        byte Id { get; }
        byte Cycles { get; }
        byte Length { get; }
        OpCodeResult Execute(Cpu cpu, Mmu mmu);
    }
}