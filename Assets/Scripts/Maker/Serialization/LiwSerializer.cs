using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;
using Newtonsoft.Json;
using ExternMaker;
using ExternMaker.Serialization;

public static class LiwSerializer
{
    private static Deserializer m_yamlDeserializer;
    public static Deserializer yamlDeserializer
    {
        get
        {
            if (m_yamlDeserializer == null) m_yamlDeserializer = new Deserializer();
            return m_yamlDeserializer;
        }
        private set
        {
            m_yamlDeserializer = value;
        }
    }

    private static Serializer m_yamlSerializer;
    public static Serializer yamlSerializer
    {
        get
        {
            if (m_yamlSerializer == null) m_yamlSerializer = new Serializer();
            return m_yamlSerializer;
        }
        private set
        {
            m_yamlSerializer = value;
        }
    }

    public static LiwProject GetProject(string directory)
    {
        var path = Path.Combine(directory, "Project.liw");
        if (File.Exists(path))
        {
            return yamlDeserializer.Deserialize<LiwProject>(File.ReadAllText(path));
        }
        return null;
    }

    public static bool CreateProject(string directory, LiwProject project)
    {
        try
        {
            CheckDirectory(directory);
            var res = Path.Combine(directory, "Resources");
            CheckDirectory(res);
            foreach (var file in Directory.GetFiles(res))
            {
                if (!file.Contains(".meta"))
                {
                    LiwCustomObject.RegisterMeta(file);
                }
            }
            project.info.gameVersion = "LW" + Application.version;
            File.WriteAllText(Path.Combine(directory, "Project.liw"), yamlSerializer.Serialize(project));
            File.WriteAllText(Path.Combine(directory, "ProjectInfo.liwf"), yamlSerializer.Serialize(project.info));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return false;
        }
    }

    public static string CheckDirectory(string path)
    {
        //if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        Storage.CheckDirectory(path);
        return path;
    }

    public static Type[] serializables = {
        typeof(Serializables.SerializedMesh),
        typeof(Serializables.SerializedLine),
        typeof(Serializables.SerializedTrigger),
        typeof(Serializables.SerializedSpriteRenderer),
        typeof(Serializables.SerializedModTrigger),
        typeof(Serializables.SerializedCamera)
    };

    public static LiwGameObject SerializeGameObject(ExtObject obj)
    {
        var cls = new LiwGameObject();

        cls.name = obj.name;
        cls.tag = obj.tag;
        cls.instanceID = obj.instanceID;
        cls.transform = new LiwTransform(obj.transform);
        cls.groupIDs = obj.groupID;

        foreach(var t in serializables)
        {
            var comp = TrySerialize(obj, t);
            if (comp != null)
            {
                comp.TakeValues(obj.gameObject);
                cls.components.Add(comp.GetData());
            }
        }

        var b = obj.GetComponent<SerializableTrigger>();
        if (b != null)
        {
            b.serializer.TakeValuesFromComponent(b.GetType(), b);
            cls.components.Add(b.serializer.GetData());
        }

        return cls;
    }

    public static ExtSerializables TrySerialize(ExtObject obj, Type type)
    {
        var serials = (ExtSerializables)Activator.CreateInstance(type);
        return serials.IsObjectSupported(obj.gameObject) ? serials : null;
    }

    public static ExtObject SpawnObject(LiwGameObject source)
    {
        var obj = new GameObject();
        obj.name = source.name;
        obj.tag = source.tag;

        var src = obj.AddComponent<ExtObject>();
        src.Initialize(source.instanceID);
        src.groupID = source.groupIDs;

        source.transform.ApplyTo(obj.transform);

        foreach(var component in source.components)
        {
            component.ApplyTo(src);
        }

        return src;
    }

    public static void ApplyTo(ExtObject obj, LiwGameObject source, bool now = false)
    {
        obj.name = source.name;
        obj.tag = source.tag;

        source.transform.ApplyTo(obj.transform);

        foreach (var component in source.components)
        {
            component.ApplyTo(obj, now);
        }
    }

    public static LiwComponent SerializeComponent(object obj)
    {
        var cls = new LiwComponent();
        var type = obj.GetType();
        var data = JsonConvert.SerializeObject(obj);
        cls.fullName = type.FullName;
        cls.data = data;
        return cls;
    }
}

[Serializable]
public class LiwProject
{
    public LiwProjectInfo info = new LiwProjectInfo();

    public Serializables.SerializedRenderSettings renderSettings = new Serializables.SerializedRenderSettings();
    public LiwGameObject lineInfo = new LiwGameObject();
    public LiwGameObject cameraInfo = new LiwGameObject();

    public List<LiwGameObject> gameObjects = new List<LiwGameObject>();
}

[Serializable]
public class LiwCustomObject
{
    public int id;
    public string name;
    public string extension;

    public bool isInvalid;
    public Texture2D tex2D;
    public Texture texture;
    public Sprite sprite;
    public GameObject obj;
    public Mesh mesh;

    static List<string> pictureExtensions = new List<string>(new []{
        ".png",
        ".jpg",
        ".jpeg"
    });

    public void LoadObject(string path)
    {
        if (ExtProjectManager.instance.resourcesFromLiwb)
            LoadObjectInternally(path);
        else
            LoadObjectExternally(path);
    }

    public void LoadObjectExternally(string path)
    {
        var info = RegisterMeta(path);
        name = info.name;
        id = info.id;

        extension = Path.GetExtension(path);

        if (extension.Contains(".obj"))
        {
            try
            {
                mesh = ExternMaker.ObjectSerializer.ImportFile(path);
                mesh.name = info.name;
                obj = ExtCore.instance.configurations[0].Instantiate(ExtCore.instance.globalParent);
                obj.GetComponent<MeshFilter>().sharedMesh = mesh;
                obj.name = mesh.name;
                UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
                var meshCol = obj.AddOrGetComponent<MeshCollider>();
                meshCol.sharedMesh = mesh;
                meshCol.enabled = false;
                obj.transform.localScale = Vector3.zero;
            }
            catch (Exception e)
            {
                ExtActionInspector.Log("Object failed to load...", "Resource Manager", "Name: " + name, "Path: " + path, "Message: " + e.Message, "StackTrace: " + e.StackTrace);
                isInvalid = true;
            }
        }
        else
        {
            if (pictureExtensions.Contains(extension.ToLower()))
            {
                tex2D = ExtMonoUtility.ConvertByteToTexture(File.ReadAllBytes(path));
                texture = tex2D;
                sprite = ExtUtility.LoadSprite(tex2D);
                sprite.name = name + extension;
                tex2D.name = name + extension;
                texture.name = name + extension;
                if (sprite != null)
                {
                    obj = new GameObject(name);
                    var spriteRend = obj.AddComponent<SpriteRenderer>();
                    spriteRend.sprite = sprite;
                    obj.transform.localScale = Vector3.zero;
                }
                else
                {
                    ExtActionInspector.Log("Sprite failed to load...", "Resource Manager", "Name: " + name, "Path: " + path);
                }
            }
            else
            {
                ExtActionInspector.Log("Is an invalid object");
                isInvalid = true;
            }
        }
    }
    
    public void LoadObjectInternally(string path)
    {
        var project = ExtProjectManager.instance.liwbProject;
        var file = project.FindResource(path);
        if (file != null) LoadObject(file);
    }

    public void LoadObject(LiwbFile file)
    {
        extension = Path.GetExtension(file.path);

        if (extension.Contains(".obj"))
        {
            try
            {
                mesh = ExternMaker.ObjectSerializer.ImportFileFromData(System.Text.Encoding.UTF8.GetString(file.GetBytes()));
                mesh.name = Path.GetFileNameWithoutExtension(file.path);
                obj = ExtCore.instance.configurations[0].Instantiate(ExtCore.instance.globalParent);
                obj.name = mesh.name;
                obj.GetComponent<MeshFilter>().sharedMesh = mesh;
                var meshCol = obj.GetComponent<MeshCollider>();
                meshCol.sharedMesh = mesh;
                meshCol.enabled = false;
                obj.transform.localScale = Vector3.zero;
            }
            catch (Exception e)
            {
                ExtActionInspector.Log("Object failed to load...", "Resource Manager", "Name: " + name, "Path: " + file.path, "Message: " + e.Message, "StackTrace: " + e.StackTrace);
                isInvalid = true;
            }
        }
        else
        {
            if (pictureExtensions.Contains(extension.ToLower()))
            {
                tex2D = ExtMonoUtility.ConvertByteToTexture(file.GetBytes());
                texture = tex2D;
                sprite = ExtUtility.LoadSprite(tex2D);
                sprite.name = Path.GetFileNameWithoutExtension(file.path) + extension;
                tex2D.name = Path.GetFileNameWithoutExtension(file.path) + extension;
                texture.name = Path.GetFileNameWithoutExtension(file.path) + extension;
                if (sprite != null)
                {
                    obj = new GameObject(name);
                    var spriteRend = obj.AddComponent<SpriteRenderer>();
                    spriteRend.sprite = sprite;
                    obj.transform.localScale = Vector3.zero;
                }
                else
                {
                    ExtActionInspector.Log("Sprite failed to load...", "Resource Manager", "Name: " + name, "Path: " + file.path);
                }
            }
            else
            {
                ExtActionInspector.Log("Is an invalid object");
                isInvalid = true;
            }
        }
    }

    [Serializable]
    public class Info
    {
        public int id;
        public string name;

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var info = obj as Info;
            return id == info.id && name == info.name;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public static Info RegisterMeta(string path)
    {
        var metaPath = path + ".meta";
        if(File.Exists(metaPath))
        {
            var info = LiwSerializer.yamlDeserializer.Deserialize<Info>(File.ReadAllText(metaPath));
            return info;
        }
        else
        {
            var info = new Info();
            ExtResourcesManager.instance.lastCustomObjectID++;
            info.id = ExtResourcesManager.instance.lastCustomObjectID;
            info.name = Path.GetFileNameWithoutExtension(path);
            File.WriteAllText(metaPath, LiwSerializer.yamlSerializer.Serialize(info));
            return info;
        }
    }

    public static Info CreateMeta(string path, int instanceID)
    {
        var metaPath = path + ".meta";
        var info = new Info();
        info.id = instanceID;
        info.name = Path.GetFileNameWithoutExtension(path);
        File.WriteAllText(metaPath, LiwSerializer.yamlSerializer.Serialize(info));
        return info;
    }
}

[Serializable]
public class LiwProjectInfo
{
    public string levelName = "Untitled";

    public string description;

    public string authorName;

    public string musicName = "Untitled";
    public string musicArtist = "Unknown";
    public string musicFile = "audio.ogg";

    public int levelID;
    public int authorID;

    public int levelVersion = 1;
    public string gameVersion = "Liw1.0";

    public LiwProjectInfo()
    {
        System.Random rand = new System.Random();
        levelID = rand.Next(0000001, 9999999);
    }
}

[Serializable]
public class LiwGameObject
{
    public int instanceID;

    public string name = "GameObject";
    public string tag = "Untagged";

    public List<int> groupIDs = new List<int>();

    public LiwTransform transform = new LiwTransform();

    public List<LiwComponent> components = new List<LiwComponent>();
}

[Serializable]
public class LiwTransform
{
    public LiwAlias.Vector3 position;
    public LiwAlias.Vector3 rotation;
    public LiwAlias.Vector3 scale = new LiwAlias.Vector3(1, 1, 1);

    public LiwTransform()
    {

    }

    public LiwTransform(Transform tr = null)
    {
        if(tr != null)
        {
            position = tr.position;
            rotation = tr.eulerAngles;
            scale = tr.localScale;
        }
    }

    public void ApplyTo(Transform tr)
    {
        tr.position = position;
        tr.eulerAngles = rotation;
        tr.localScale = scale;
    }
}

[Serializable]
public class LiwComponent
{
    public string fullName;
    public string data;

    public void ApplyTo(ExtObject obj, bool now = false)
    {
        var cls = JsonUtility.FromJson(data, Type.GetType(fullName));
        var inherit = (ExtSerializables)cls;
        inherit.ApplyTo(obj.gameObject, now);
    }
}

public static class LiwAlias
{
    [Serializable]
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float x = 0, float y = 0, float z = 0)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3(UnityEngine.Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public UnityEngine.Vector3 ToVector()
        {
            return new UnityEngine.Vector3(x, y, z);
        }
        
        public static implicit operator Vector3(UnityEngine.Vector3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static implicit operator UnityEngine.Vector3(Vector3 v)
        {
            return new UnityEngine.Vector3(v.x, v.y, v.z);
        }
    }

    [Serializable]
    public class Material
    {
        public int shaderIndex;
        public Color color = UnityEngine.Color.white;
    }

    [Serializable]
    public struct Vector2
    {
        public float x;
        public float y;

        public Vector2(float x = 0, float y = 0)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2(UnityEngine.Vector2 vector)
        {
            x = vector.x;
            y = vector.y;
        }

        public UnityEngine.Vector2 ToVector()
        {
            return new UnityEngine.Vector2(x, y);
        }

        public static implicit operator Vector2(UnityEngine.Vector2 v)
        {
            return new Vector2(v.x, v.y);
        }

        public static implicit operator UnityEngine.Vector2(Vector2 v)
        {
            return new UnityEngine.Vector2(v.x, v.y);
        }
    }

    [Serializable]
    public struct Color
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public Color(float r = 0, float g = 0, float b = 0, float a = 0)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public Color(UnityEngine.Color color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }

        public UnityEngine.Color ToColor()
        {
            return new UnityEngine.Color(r, g, b, a);
        }
        
        public static implicit operator Color(UnityEngine.Color v)
        {
            return new Color(v.r, v.g, v.b, v.a);
        }

        public static implicit operator UnityEngine.Color(Color v)
        {
            return new UnityEngine.Color(v.r, v.g, v.b, v.a);
        }
    }
}
