using System;

namespace Terra.Studio
{
    public interface IObscurer
    {
        public object Getter();
        public void Setter(object obj);
        public Type ObscureType { get; }
        public Type DeclaredType { get; }
    }
}