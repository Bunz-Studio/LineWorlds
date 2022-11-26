using System.IO;
using System.Collections.Generic;
using UnityEngine;

using SimpleFileBrowser;

namespace ExternMaker
{
    public class ExtResourcesManager : MonoBehaviour
    {
        private static ExtResourcesManager p_instance;
        public static ExtResourcesManager instance
        {
            get
            {
                p_instance = ExtUtility.GetStaticInstance(p_instance);
                return p_instance;
            }
        }

        public Transform instancesParent;
        public GameObject instanceObject;

        public List<ExtResourceItem> instanceCollection = new List<ExtResourceItem>();

        private void Start()
        {
            p_instance = this;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            p_instance = null;
        }

        public void RefreshResourceList(bool refreshFromDirectory = true)
        {
            if(refreshFromDirectory) RefreshResources();
            foreach(var a in customObjects)
            {
                if (!a.isInvalid)
                {
                    AddResourceList(a);
                }
            }
        }

        public void AddResourceList(LiwCustomObject obj)
        {
            var item = GetListItem(obj);
            if(item != null)
            {
                item.Initialize(obj);
            }
            else
            {
                var inst = Instantiate(instanceObject, instancesParent);
                item = inst.GetComponent<ExtResourceItem>();
                item.Initialize(obj);
                instanceCollection.Add(item);
            }
        }

        public void RemoveResourceList(LiwCustomObject obj)
        {
            var item = GetListItem(obj);
            if (item != null)
            {
                instanceCollection.Remove(item);
                DestroyImmediate(item.gameObject);
            }
        }

        public void ClearResourceList()
        {
            foreach(var inst in instanceCollection)
            {
                DestroyImmediate(inst.gameObject);
            }
            instanceCollection.Clear();
        }
        public ExtResourceItem GetListItem(LiwCustomObject obj)
        {
            foreach(var l in instanceCollection)
            {
                var result = l.customObject.id == obj.id;
                if (result) return l;
            }
            return null;
        }

        public LiwCustomObject FindSprite(Sprite sprite)
        {
            foreach(var c in customObjects)
            {
                if (c.sprite == sprite) return c;
            }
            return null;
        }

        public LiwCustomObject FindObject(string name)
        {
            foreach (var c in customObjects)
            {
                if (c.name + c.extension == name) return c;
            }
            return null;
        }

        public List<LiwCustomObject> customObjects = new List<LiwCustomObject>();
        public int lastCustomObjectID;

        public void RefreshResources()
        {
            var res = LiwSerializer.CheckDirectory(Path.Combine(ExtProjectManager.instance.directory, "Resources"));
            // Get the files and register all of the resources
            // Also check if there's some new objects
            // Now loads the meta first
            var loadLater = new List<string>();
            foreach (var file in Directory.GetFiles(res))
            {
                if (file.Contains(".meta"))
                {
                    var objFile = file.Replace(".meta", "");
                    var info = LiwCustomObject.RegisterMeta(objFile);
                    var obj = GetObject(info);

                    if(obj != null)
                        obj.name = info.name;
                    else
                        RegisterObject(objFile);
                }
                else
                {
                    loadLater.Add(file);
                }
            }
            foreach (var file in loadLater)
            {
                var info = LiwCustomObject.RegisterMeta(file);
                var obj = GetObject(info);

                if (obj != null)
                    obj.name = info.name;
                else
                    RegisterObject(file);
            }

            // Run a check based on the resources file if something is missing
            foreach (var obj in customObjects)
            {
                var path = GetObjectPath(obj.name + obj.extension);
                if(!File.Exists(path))
                {
                    RemoveResourceList(obj);
                    ExtActionInspector.Log("An object is missing from resources", "Resource Manager", "Object ID: " + obj.id, "Object Name: " + obj.name, "Object File: " + obj.name + obj.extension);
                }
            }
        }

        public LiwCustomObject GetObject(LiwCustomObject.Info info)
        {
            foreach(var obj in customObjects)
            {
                if (obj.id == info.id) return obj;
            }
            return null;
        }

        public LiwCustomObject GetObject(int index)
        {
            foreach (var obj in customObjects)
            {
                Debug.Log(obj.id + " - " + obj.name);
                if (obj.id == index) return obj;
            }
            return null;
        }

        public void RegisterObject(string path)
        {
            var obj = new LiwCustomObject();
            obj.LoadObject(path);
            if (obj.id > lastCustomObjectID) lastCustomObjectID = obj.id;
            customObjects.Add(obj);
        }

        public void RegisterObject(LiwbFile file)
        {
            var obj = new LiwCustomObject();
            obj.LoadObject(file);
            obj.id = file.instanceID;
            if (obj.id > lastCustomObjectID) lastCustomObjectID = obj.id;
            customObjects.Add(obj);
        }

        public void RemoveObject(LiwCustomObject obj)
        {
            DestroyImmediate(obj.obj);
            customObjects.Remove(obj);
        }

        public void ClearObjects()
        {
            lastCustomObjectID = 0;
            foreach (var a in customObjects)
            {
                DestroyImmediate(a.obj);
            }
            customObjects.Clear();
        }

        public static string GetObjectPath(string path)
        {
            return Path.Combine(ExtProjectManager.instance.directory, "Resources", path);
        }

        public UISkin browserSkin;
        public void BrowseAndAddItem()
        {
            var filter = new FileBrowser.Filter("Picture files", ".png", ".jpg", ".jpeg");
            FileBrowser.Skin = browserSkin;
            FileBrowser.SetFilters(true, filter);
            FileBrowser.SetDefaultFilter(".png");
            FileBrowser.ShowLoadDialog(
                (val) =>
                {
                    foreach(var str in val)
                    {
                        AddFile(str);
                    }
                    RefreshResourceList();
                },
                () =>
                {
                },
                FileBrowser.PickMode.Files,
                true,
                null,
                "",
                "Select Resource File"
            );
        }

        public void AddFile(string path)
        {
            var bytes = Storage.ReadAllBytes(path);
            WriteDataToResource(Path.GetFileName(path), bytes);
            LiwCustomObject.RegisterMeta(path);
        }

        public static byte[] GetDataFromResource(string path)
        {
            return Storage.ReadAllBytes(GetObjectPath(path));
        }

        public static void WriteDataToResource(string path, byte[] data)
        {
            Storage.WriteAllBytes(GetObjectPath(path), data);
        }

        public static string GetStringFromResource(string path)
        {
            return Storage.ReadAllText(GetObjectPath(path));
        }

        public static void WriteStringToResource(string path, string content)
        {
            Storage.WriteAllText(GetObjectPath(path), content);
        }

        public void CopyResourcesTo(string folderPath)
        {
            var resPath = Path.Combine(ExtProjectManager.instance.directory, "Resources");
            var files = Storage.GetFiles(resPath);
            foreach(var file in files)
            {
                if (!file.Contains(".meta"))
                {
                    var fileName = Path.GetFileName(file);
                    var targetPath = Path.Combine(folderPath, fileName);
                    Storage.WriteAllBytes(targetPath, Storage.ReadAllBytes(file));
                }
            }
        }
    }
}
