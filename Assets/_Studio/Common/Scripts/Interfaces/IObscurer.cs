using System;

namespace Terra.Studio
{
    public interface IObscurer
    {
        public object Get();
        public void Set(object obj);
        public Type ObscureType { get; }
        public Type DeclaredType { get; }
    }
}