using System;

namespace Terra.Studio
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class AuthorAttribute : Attribute
    {
        private readonly string authorTarget;
        public string AuthorTarget { get { return authorTarget; } }

        public AuthorAttribute(string authorTarget)
        {
            this.authorTarget = authorTarget;
        }
    }
}
