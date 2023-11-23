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

    [Serializable]
    public class AssetsCategoriesData
    {
        public CategoryData[] categoriesData;
    }

    [Serializable]
    public class CategoryData
    {
        public string category;
        public string[] subCategory;
        public string[] tags;
    }
}