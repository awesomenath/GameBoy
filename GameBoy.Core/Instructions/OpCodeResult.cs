namespace GameBoy.Core.Instructions
{
    public struct OpCodeResult
    {
        public byte Cycles { get; set; }
        public byte Length { get; set; }
        public bool MoveProgramCounter { get; set; }
        public bool StopRequested { get; set; }

        public OpCodeResult(byte length, byte cycles)
        {
            Length = length;
            Cycles = cycles;
            MoveProgramCounter = true;
            StopRequested = false;
        }

        public OpCodeResult(byte length, byte cycles, bool moveProgramCounter)
            : this(length, cycles)
        {
            MoveProgramCounter = moveProgramCounter;
        }
    }
}
