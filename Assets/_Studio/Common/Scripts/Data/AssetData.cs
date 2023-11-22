using System;

namespace Terra.Studio
{
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
        public string[] flags;
    }
}