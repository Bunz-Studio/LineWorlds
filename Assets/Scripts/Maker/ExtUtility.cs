using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using System.Runtime.InteropServices;
using System.IO.Compression;
using System.Text;

using Object = UnityEngine.Object;

namespace ExternMaker
{
    public static class ExtUtility
    {
	    [DllImport("user32.dll")]
	    public static extern bool SetCursorPos(int X, int Y);
        public static T GetStaticInstance<T>(T cache) where T : Component
        {
            return cache == null ? (T)(object)Object.FindObjectOfType(typeof(T)) : cache;
        }

        public static void InvokeOnExist<T>(this Action<T> action, T obj)
        {
            if (action != default) action.Invoke(obj);
        }

        public static void InvokeOnExist<T1, T2>(this Action<T1, T2> action, T1 obj1, T2 obj2)
        {
            if (action != default) action.Invoke(obj1, obj2);
        }

        public static void InvokeOnExist(this Action action)
        {
            if (action != default) action.Invoke();
        }

        public static bool DoesObjectHaveComponent<T>(this GameObject obj)
        {
            return obj.GetComponent(typeof(T)) != null;
        }

        public static T AddOrGetComponent<T>(this GameObject obj)
        {
            var comp = obj.GetComponent(typeof(T));
            return comp == null ? (T)(object)obj.AddComponent(typeof(T)) : (T)(object)comp;
        }

        public static object AddOrGetComponent(this GameObject obj, Type type)
        {
            var comp = obj.GetComponent(type);
            return comp == null ? obj.AddComponent(type) : comp;
        }

        public static List<object> WrapInList(this object obj)
        {
            return new List<object>(new object[] { obj });
        }

        // Avoid changing the original object's shared material
        // I'm using sharedMaterial to improve performance and reduce memory usage
        public static void ReplaceWithNewMaterials(MeshRenderer rend)
        {
            List<Material> mats = new List<Material>();
            foreach (var mtrl in rend.sharedMaterials)
            {
                if(mtrl != null) mats.Add(new Material(mtrl));
            }
            rend.sharedMaterials = mats.ToArray();
        }

        public static Object FindObjectFromInstanceID(int iid)
		{
			var t = typeof(Object)
			     .GetMethod("FindObjectFromInstanceID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
			     .Invoke(null, new object[] { iid });
			return (Object)t;
		}
		
		public static bool IsValidShortcutKey()
		{
			var obj = EventSystem.current.currentSelectedGameObject;
			bool res = obj == null || obj.GetComponent<InputField>() == null;
			return res && !Input.GetMouseButton(0) && !Input.GetMouseButton(1);
        }
        public static GameObject GetSelectedUI()
        {
            return EventSystem.current.currentSelectedGameObject;
        }

        public static string GetRidOfLastPath(string dir)
        {
            var e = dir.Split(Path.DirectorySeparatorChar);
            var y = dir.Remove(dir.Length - e[e.Length - 1].Length, e[e.Length - 1].Length);
            if (y.EndsWith("\\", StringComparison.CurrentCulture) || y.EndsWith("/", StringComparison.CurrentCulture))
            {
                y = y.Remove(y.Length - 1, 1);
            }
            return y;
        }

        public static void RecursiveDelete(DirectoryInfo baseDir)
        {
            if (!baseDir.Exists)
                return;

            foreach (var dir in baseDir.EnumerateDirectories())
            {
                RecursiveDelete(dir);
            }
            baseDir.Delete(true);
        }
        
        public static Texture2D LoadImage(string path)
        {
            if (File.Exists(path))
            {
                byte[] bytes = File.ReadAllBytes(path);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                tex.LoadImage(bytes);
                return tex;
            }
            return null;
        }

        public static Sprite LoadSprite(string path)
        {
            var image = LoadImage(path);
            Sprite sprite = Sprite.Create(image, new Rect(0.0f, 0.0f, image.width,
            image.height), new Vector2(0.5f, 0.5f), 100.0f);
            return sprite;
        }

        public static Sprite LoadSprite(Texture2D image)
        {
            Sprite sprite = Sprite.Create(image, new Rect(0.0f, 0.0f, image.width,
            image.height), new Vector2(0.5f, 0.5f), 100.0f);
            return sprite;
        }
        
        public static GameObject Instantiate(this GameObject gameObject, Transform parent)
        {
        	var obj = Object.Instantiate(gameObject, parent);
        	obj.name = gameObject.name;
        	return obj;
        }
        
        public static bool IsInputHoveringUI()
        {
        	foreach (Touch touch in Input.touches)
			{
			    int id = touch.fingerId;
			    if (EventSystem.current.IsPointerOverGameObject(id)) return true;
 			}
        	return EventSystem.current.IsPointerOverGameObject();
        }
        /// <summary>
        /// Compresses the string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The compressed string!</returns>
        public static string CompressString(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            var memoryStream = new MemoryStream();
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gZipStream.Write(buffer, 0, buffer.Length);
            }

            memoryStream.Position = 0;

            var compressedData = new byte[memoryStream.Length];
            memoryStream.Read(compressedData, 0, compressedData.Length);

            var gZipBuffer = new byte[compressedData.Length + 4];
            Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
            return Convert.ToBase64String(gZipBuffer);
        }

        /// <summary>
        /// Decompresses the string.
        /// </summary>
        /// <param name="compressedText">The compressed text.</param>
        /// <returns>The decompressed string!</returns>
        public static string DecompressString(string compressedText)
        {
            byte[] gZipBuffer = Convert.FromBase64String(compressedText);
            using (var memoryStream = new MemoryStream())
            {
                int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

                var buffer = new byte[dataLength];

                memoryStream.Position = 0;
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }


        public static byte[] Compress(this byte[] data)
        {
            var output = new MemoryStream();
            using (var dstream = new DeflateStream(output, System.IO.Compression.CompressionLevel.Optimal))
            {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        public static byte[] Decompress(this byte[] data)
        {
            var input = new MemoryStream(data);
            var output = new MemoryStream();
            using (var dstream = new DeflateStream(input, CompressionMode.Decompress))
            {
                dstream.CopyTo(output);
            }
            return output.ToArray();
        }
    }
}
