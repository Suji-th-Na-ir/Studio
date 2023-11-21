using System;

namespace Terra.Studio
{
    [Serializable]
    public class GltfData
    {
        public BufferData[] buffers;
    }


    [Serializable]
    public struct BufferData
    {
        public string uri;
    }
}