using System;

namespace GameBoy.Core.Hardware
{
    public class MemorySpace
    {
        public ushort Start { get; }
        public ushort End { get; }
        public ushort Size { get; }
        public string Name { get; }

        private event MemoryWriteEvent MemoryWriteEvents;

        public delegate void MemoryWriteEvent(ushort address);
        public bool HasEventHandlers { get; private set; }

        public MemorySpace(string name, ushort start, ushort end)
        {
            if (start > end)
            {
                throw new ArgumentException($"{nameof(start)} cannot be larger than {nameof(end)}", nameof(start));
            }

            Name = name;
            Start = start;
            End = end;
            Size = (ushort)(end + 1 - start);
        }

        public bool AddressContainedWithin(ushort address)
        {
            return address >= Start && address <= End;
        }

        public void Subscribe(MemoryWriteEvent memoryWriteEvent)
        {
            MemoryWriteEvents += memoryWriteEvent;

            HasEventHandlers = true;
        }

        public void Unsubscribe(MemoryWriteEvent memoryWriteEvent)
        {
            MemoryWriteEvents -= memoryWriteEvent;

            if (MemoryWriteEvents.GetInvocationList().Length == 0)
            {
                HasEventHandlers = false;
            }
        }

        public void TriggerWriteEvent(ushort address)
        {
            MemoryWriteEvents?.Invoke(address);
        }

    }
}
