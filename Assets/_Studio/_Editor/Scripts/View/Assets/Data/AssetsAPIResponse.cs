using System;

namespace Terra.Studio.Data
{
    [Serializable]
    public class AssetsAPIResponse
    {
        public bool success;
        public string message;
        public AssetData[] data;
    }

    [Serializable]
    public class AssetData
    {
        public string _id;
        public string display_name;
        public string unique_name;
        public string[] bin;
        public string[] gltf;
        public string[] textures;
        public string[] thumbnails;
        public string[] category;
        public bool terras3;
        public int asset_id;
    }
}