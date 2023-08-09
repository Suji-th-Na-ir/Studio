using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[System.Serializable]
public class ResourceItem
{
    public enum Type
    {
        Unknown = 0,
        Any = 0,
        Folder = 1,
        Asset = 2,
    }

    [SerializeField]
    private string name;
    [SerializeField]
    private string ext;
    [SerializeField]
    private string path;
    [SerializeField]
    private Type type = Type.Unknown;
    [SerializeField]
    private string objectTypeName;
    private System.Type objectType;
    private ResourceItem parent = null;
    [SerializeField]internal Dictionary<string, ResourceItem> childs = null;

    public string Name { get { return name; } }
    public string Ext { get { return ext; } }
    public string Path { get { return path; } }
    public string ResourcesPath { get { return string.IsNullOrEmpty(path) ? name : path + "/" + name; } }
    public Type ResourcesType { get { return type; } }
    public ResourceItem Parent { get { return parent; } }

    public ResourceItem()
    {
        if (type == Type.Folder)
            childs = new Dictionary<string, ResourceItem>();
    }

    public ResourceItem(string aFileName, string aPath, Type aType, string aObjectType)
    {
        var index = aFileName.LastIndexOf(".");
        if (index > 0)
        {
            name = aFileName.Substring(0, index);
            ext = aFileName.Substring(index + 1);
        }
        else
        {
            name = aFileName;
            ext = "";
        }
        path = aPath;
        type = aType;
        objectTypeName = aObjectType;
        objectType = System.Type.GetType(objectTypeName);
        if (type == Type.Folder)
            childs = new Dictionary<string, ResourceItem>();
    }

    public T Load<T>() where T : UnityEngine.Object
    {
        //Debug.Log("Load: " + ResourcesPath + " / " + typeof(T).Name);
        return Resources.Load<T>(ResourcesPath);
    }

}

public class ResourceDB : ScriptableObject, ISerializationCallbackReceiver
{
    private static ResourceDB m_Instance = null;
    public static ResourceDB FindInstance()
    {
        return Resources.Load<ResourceDB>("ResourceDB");
    }

    public ResourceDB()
    {
        m_Instance = this;
    }

    public static ResourceDB Instance
    {
        get
        {
            if (m_Instance != null)
                return m_Instance;
            m_Instance = FindInstance();
            if (m_Instance != null)
                return m_Instance;
            m_Instance = CreateInstance<ResourceDB>();
#if UNITY_EDITOR
            var resDir = new DirectoryInfo(Path.Combine(Application.dataPath, "Resources"));
            if (!resDir.Exists)
                UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
            UnityEditor.AssetDatabase.CreateAsset(m_Instance, "Assets/Resources/ResourceDB.asset");
            m_Instance = FindInstance();
#endif
            return m_Instance;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Update ResourceDB")]
    internal static void TriggerUpdate()
    {
        Instance.UpdateDB();
    }
#endif

    [SerializeField]
    internal List<ResourceItem> items = new List<ResourceItem>();
    [SerializeField, HideInInspector]
    private int m_FileCount = 0;
    [SerializeField, HideInInspector]
    private int m_FolderCount = 0;
    [SerializeField, HideInInspector]
    public bool UpdateAutomatically = false;
 
    public int FileCount { get { return m_FileCount; } }
    public int FolderCount { get { return m_FolderCount; } }

    public static ResourceItem GetStudioAsset(string aName)
    {
        return Instance.GetItem(aName);
    }

    public static string ConvertPath(string aPath)
    {
        return aPath.Replace("\\", "/");
    }

#if UNITY_EDITOR
    void ScanFolder(DirectoryInfo aFolder, List<DirectoryInfo> aList, bool aOnlyTopFolders)
    {
        string n = aFolder.Name.ToLower();
        if (n == "editor") // ignore folders
            return;
        if (n == "resources")
        {
            aList.Add(aFolder);
            if (aOnlyTopFolders)
                return;
        }
        foreach (var dir in aFolder.GetDirectories())
        {
            ScanFolder(dir, aList, aOnlyTopFolders);
        }
    }

    List<DirectoryInfo> FindResourcesFolders(bool aOnlyTopFolders)
    {
        var assets = new DirectoryInfo(Application.dataPath);
        var list = new List<DirectoryInfo>();
        ScanFolder(assets, list, aOnlyTopFolders);
        return list;
    }

    void AddFileList(DirectoryInfo aFolder, int aPrefix)
    {
        string relFolder = aFolder.FullName;
        if (relFolder.Length < aPrefix)
            relFolder = "";
        else
            relFolder = relFolder.Substring(aPrefix);
        relFolder = ConvertPath(relFolder);
        foreach (var folder in aFolder.GetDirectories())
        {
            //items.Add(new ResourceItem(folder.Name, relFolder, ResourceItem.Type.Folder, ""));
            AddFileList(folder, aPrefix);
        }

        foreach (var file in aFolder.GetFiles())
        {
            string ext = file.Extension.ToLower();
            if (ext == ".meta")
                continue;
            string assetPath = "assets/" + file.FullName.Substring(Application.dataPath.Length + 1);
            assetPath = ConvertPath(assetPath);
            var obj = UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
            if (obj == null)
            {
                Debug.LogWarning("ResourceDB: File at path " + assetPath + " couldn't be loaded and is ignored. Probably not an asset?!");
                continue;
            }
            string type = obj.GetType().AssemblyQualifiedName;
            items.Add(new ResourceItem(file.Name, relFolder+"/"+ Path.GetFileNameWithoutExtension(file.Name), ResourceItem.Type.Asset, type));
        }
        Resources.UnloadUnusedAssets();
    }

    public void UpdateDB(bool aSetDirty = false)
    {
        items.Clear();
        var topFolders = FindResourcesFolders(true);

        foreach (var folder in topFolders)
        {
            string path = folder.FullName;
            int prefix = path.Length;
            if (!path.EndsWith("/"))
                prefix++;
            AddFileList(folder, prefix);
        }
        m_FolderCount = 0;
        m_FileCount = 0;
        foreach (var item in items)
        {
            if (item.ResourcesType == ResourceItem.Type.Folder)
                m_FolderCount++;
            else if (item.ResourcesType == ResourceItem.Type.Asset)
                m_FileCount++;
        }
        if (aSetDirty)
        {
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
        }
    }
#endif

    public ResourceItem GetItem(string aName)
    {
        bool checkName = !string.IsNullOrEmpty(aName);
        foreach (var item in items)
        {      
            if (checkName && aName != item.Name)
                continue;
           return item;
        }
        return null;
    }

    public void OnBeforeSerialize()
    {
#if UNITY_EDITOR
        if (items == null || items.Count == 0)
        {
            UpdateDB();
        }
#endif
    }

    public void OnAfterDeserialize()
    {
    }
}


#if UNITY_EDITOR

public class ResourceDBPostprocessor : UnityEditor.AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        if (ResourceDB.FindInstance() == null)
            return;
        if (!ResourceDB.Instance.UpdateAutomatically)
            return;
        var files = importedAssets.Concat(deletedAssets).Concat(movedAssets).Concat(movedFromAssetPaths);
        bool update = false;
        foreach (var file in files)
        {
            var fn = file.ToLower();
            if (!fn.Contains("resourcedb.asset") && fn.Contains("/resources/"))
            {
                update = true;
                break;
            }
        }
        if (update)
        {
            ResourceDB.Instance.UpdateDB();
        }
    }
}
#endif
