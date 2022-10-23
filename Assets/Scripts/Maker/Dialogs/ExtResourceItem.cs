using UnityEngine;
using UnityEngine.UI;

namespace ExternMaker
{
    public class ExtResourceItem : MonoBehaviour
    {
        public RawImage thumbnail;
        public Text text;

        public LiwCustomObject customObject;
        public string path;
        
        public void Initialize(LiwCustomObject obj)
        {
            customObject = obj;
            path = ExtResourcesManager.GetObjectPath(obj.name + obj.extension);
            text.text = customObject.name;
            if (customObject.sprite != null)
            {
                thumbnail.texture = customObject.tex2D;
            }
            else
            {
                if(thumbnail.texture != null)
                {
                    DestroyImmediate(thumbnail.texture);
                }
                if (customObject.obj != null)
                {
                    customObject.obj.transform.localScale = Vector3.one;
                    thumbnail.texture = RuntimePreviewGenerator.GenerateModelPreview(customObject.obj.transform, 64, 64);
                    customObject.obj.transform.localScale = Vector3.zero;
                }
            }
        }

        public void Create()
        {
            if (customObject.sprite != null)
            {
                var obj = ExtCore.instance.CreateObject(customObject.obj);
                obj.transform.localScale = Vector3.one;
                obj.customObject = customObject;
                obj.objectType = ExtObject.ObjectType.Sprite;
                if (obj.GetComponent<Collider>() == null) obj.gameObject.AddOrGetComponent<BoxCollider>();
            }
            else
            {
                if(customObject.mesh != null)
                {
                    var obj = ExtCore.instance.CreateObject(customObject.obj);
                    obj.transform.localScale = Vector3.one;
                    obj.customObject = customObject;
                    obj.objectType = ExtObject.ObjectType.Custom;
                    obj.gameObject.GetComponent<MeshCollider>().enabled = true;
                    Debug.Log("Is going through enabling mesh collider");
                }
                else
                {
                    ExtActionInspector.Log("There's isn't any mesh or sprite for this object", "Resource Manager", "Filename: " + customObject.name, "Extension: " + customObject.extension);
                }
            }
        }
    }
}