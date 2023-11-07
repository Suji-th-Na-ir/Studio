using System;

namespace Terra.Studio
{
    public class RequestValidator
    {
        private WorldData _worldData;
        private Action _onComplete;
        public void Bla(ref WorldData bla, Action onComplete)
        {
            _worldData = bla;
            _onComplete = onComplete;
            return;
        }
    }
}