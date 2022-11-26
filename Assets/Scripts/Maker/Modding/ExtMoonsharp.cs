using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;

namespace ExternMaker
{
    public static class ExtMoonSharp
    {
        static bool isInitialized;
        public static void Initialize()
        {
            if (isInitialized) return;
            UserData.RegisterAssembly();
            UserData.RegisterType<Vector3>();
            UserData.RegisterType<Vector2>();
            UserData.RegisterType<Quaternion>();
            UserData.RegisterType<Color>();
            UserData.RegisterType<GameObject>();
            isInitialized = true;
        }
        public static ExtMoonInstance CreateMoonInstance()
        {
            return new ExtMoonInstance();
        }

        /*[MoonSharpUserData]
        public class Vector3
        {
            public float x;
            public float y;
            public float z;

            public Vector3() { }
            public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }

            public Vector3 Lerp(Vector3 a, Vector3 b, float t)
            {
                return ExtUniMoon.Lerp(a, b, t);
            }

            public static implicit operator UnityEngine.Vector3(Vector3 v)
            {
                return new UnityEngine.Vector3(v.x, v.y, v.z);
            }

            public static implicit operator Vector3(UnityEngine.Vector3 v)
            {
                return new Vector3(v.x, v.y, v.z);
            }
        }*/

        [MoonSharpUserData]
        public class Player
        {
            LineMovement mov
            {
                get
                {
                    return ExtCore.instance.lineMovement;
                }
            }

            public float speed
            {
                get => mov.lineSpeed;
                set => mov.lineSpeed = value;
            }

            /*public List<Vector3> turns
            {
                get => ExtUniMoon.ConvertAllArray(mov.turns);
                set
                {
                    var val = ExtUniMoon.ConvertAllArray(value);
                    mov.turns = val;
                }
            }*/

            public List<Vector3> turns
            {
                get => new List<Vector3>(mov.turns);
                set
                {
                    mov.turns = value.ToArray();
                }
            }

            public void Turn()
            {
                mov.TurnLine();
            }

            public void Kill()
            {
                mov.Kill();
            }

            public void Teleport(Vector3 location)
            {
                mov.transform.position = location;
                mov.CreateTail();
            }
        }

        [MoonSharpUserData]
        public class World
        {
            ExtProjectSettings sets
            {
                get => ExtProjectManager.instance.settings;
            }

            public Color backgroundColor
            {
                get => sets.backgroundColor;
                set => sets.backgroundColor = value;
            }

            public Color fogColor
            {
                get => RenderSettings.fogColor;
                set => RenderSettings.fogColor = value;
            }
        }
    }

    public class ExtUniMoon
    {
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            return Vector3.Lerp(a, b, t);
        }

        /*public static List<ExtMoonSharp.Vector3> ConvertAllArray(Vector3[] array)
        {
            var list = new List<ExtMoonSharp.Vector3>();
            foreach (var arr in array) list.Add(arr);
            return list;
        }

        public static Vector3[] ConvertAllArray(List<ExtMoonSharp.Vector3> array)
        {
            var list = new List<Vector3>();
            foreach (var arr in array) list.Add(arr);
            return list.ToArray();
        }*/
    }

    public class ExtMoonInstance
    {
        public Script script;
        public string code;
        public string path;

        public void ReadCode()
        {
            if(Storage.FileExists(path)) code = Storage.ReadAllText(path);
        }

        public void CreateScript()
        {
            script = new Script();

            // Variables
            script.Globals["player"] = new ExtMoonSharp.Player();
            script.Globals["world"] = new ExtMoonSharp.World();

            // Functions
            script.Globals["FindObject"] = (Func<int, GameObject>)((id) => { return ExtCore.GetObject(id).gameObject; });
            script.Globals["FindObject"] = (Func<string, GameObject>)((name) => { return GameObject.Find(name); });

            // Constructors
            script.Globals["GameObject"] = (Func<string, GameObject>)((name) => { return new GameObject(name); });
            script.Globals["Vector3"] = (Func<float, float, float, Vector3>)((x, y, z) => { return new Vector3(x, y, z);  });
            script.Globals["Vector2"] = (Func<float, float, Vector2>)((x, y) => { return new Vector2(x, y); });
            script.Globals["Quaternion"] = (Func<float, float, float, float, Quaternion>)((x, y, z, w) => { return new Quaternion(x, y, z, w); });
            script.Globals["Color"] = (Func<float, float, float, float, Color>)((r, g, b, a) => { return new Color(r, g, b, a); });

            script.DoString(code);
        }

        public void ChangeGlobal(string variableName, DynValue value)
        {
            script.Globals[variableName] = value;
        }

        public void CallGlobal(string functionName, params object[] args)
        {
            if (script.Globals[functionName] == null) return;
            script.Call(script.Globals[functionName], args);
        }
    }
}