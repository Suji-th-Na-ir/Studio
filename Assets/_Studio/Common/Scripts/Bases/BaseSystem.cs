using System;
using System.Collections.Generic;

namespace Terra.Studio
{
    public abstract class BaseSystem
    {
        public abstract Dictionary<int, Action<object>> IdToConditionalCallback { get; set; }
    }
}
