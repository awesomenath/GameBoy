using System;

namespace GameBoy.Core.Instructions.Operands
{
    public class Operand<T>
    {
        public string Name { get; }
        protected Func<T> Getter { get; }
        protected Action<T> Setter { get; }

        public Operand(string name, Func<T> getter, Action<T> setter)
        {
            Name = name;
            Getter = getter;
            Setter = setter;
        }

        public T Get()
        {
            return Getter();
        }

        public void Set(T value)
        {
            Setter(value);
        }
    }
}
