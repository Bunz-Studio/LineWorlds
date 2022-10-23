using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace ExternMaker.Serialization
{
    [Serializable]
    public class ExtSerializables
    {
        public ExtSerializables()
        {

        }

        public virtual void Start()
        {

        }

        public virtual string Serialize()
        {
            /*return JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });*/
            return JsonUtility.ToJson(this);
        }

        public virtual void ApplyTo(GameObject obj, bool now = false)
        {

        }

        public virtual bool IsObjectSupported(GameObject obj)
        {
            return true;
        }

        public virtual LiwComponent GetData()
        {
            var cls = new LiwComponent();
            cls.fullName = GetType().FullName;
            cls.data = Serialize();
            return cls;
        }

        public virtual void TakeValues(GameObject obj)
        {
        }
    }

    public static class Serializables
    {
        [Serializable]
        public class SerializedMesh : ExtSerializables
        {
            public bool isCustom;
            public int meshInstanceID;
            public string meshResource;
            public LiwAlias.Color meshColor;
            public LiwAlias.Color[] meshColors;

            public List<LiwAlias.Material> materials = new List<LiwAlias.Material>();

            public override void ApplyTo(GameObject obj, bool now = false)
            {
                var renderer = obj.AddOrGetComponent<MeshRenderer>();
                var collectedMaterials = new List<Material>();

                if (meshColors != null && meshColors.Length > 0)
                {
                    for (int i = 0; i < meshColors.Length; i++)
                    {
                        var mat = new Material(ExtCore.instance.defaultMaterial);
                        mat.color = meshColors[i];
                        collectedMaterials.Add(mat);
                    }
                    renderer.sharedMaterials = collectedMaterials.ToArray();
                }
                else if (materials.Count > 0)
                {
                    foreach(var mat in materials)
                    {
                        var matr = new Material(ExtInspector.instance.materialSelector.shaders[mat.shaderIndex]);
                        matr.color = mat.color;
                        if (matr.HasProperty("_Glossiness"))
                        {
                            matr.SetFloat("_Glossiness", 0);
                        }
                        collectedMaterials.Add(matr);
                    }
                    renderer.sharedMaterials = collectedMaterials.ToArray();
                }
                else
                {
                    var mat = new Material(ExtCore.instance.defaultMaterial);
                    mat.color = meshColor;
                    renderer.sharedMaterials = new Material[] { mat };
                }

                Mesh mesh = null;
                if (string.IsNullOrWhiteSpace(meshResource))
                {
                    mesh = GetMesh();
                }
                else
                {
                    var customObj = ExtResourcesManager.instance.FindObject(meshResource);
                    if (customObj != null)
                    {
                        var extObj = obj.GetComponent<ExtObject>();
                        extObj.customObject = customObj;
                        extObj.objectType = ExtObject.ObjectType.Custom;
                        mesh = customObj.mesh;
                    }
                }
                if(mesh != null)
                {
                    var filter = obj.AddOrGetComponent<MeshFilter>();
                    var mCollide = obj.AddOrGetComponent<MeshCollider>();
                    mCollide.sharedMesh = mesh;
                    mCollide.convex = true;
                    filter.sharedMesh = mesh;
                }
            }

            public override void TakeValues(GameObject obj)
            {
                var rend = obj.GetComponent<MeshRenderer>();
                var filter = obj.GetComponent<MeshFilter>();
                var extObj = obj.GetComponent<ExtObject>();
                if (extObj.objectType == ExtObject.ObjectType.Custom)
                {
                    isCustom = true;
                    meshResource = extObj.customObject.name + extObj.customObject.extension;
                }
                else
                {
                    meshInstanceID = ExtCore.GetMeshIndex(filter.sharedMesh);
                }
                var mats = new List<LiwAlias.Material>();
                foreach (var a in rend.sharedMaterials)
                {
                    if(a != null)
                    {
                        try
                        {
                            if (a.HasProperty("_Color"))
                            {
                                var mat = new LiwAlias.Material();
                                var shader = ExtInspector.instance.materialSelector.shaders.FindIndex(val => val.name == a.shader.name);
                                if(shader > -1)
                                {
                                    mat.shaderIndex = shader;
                                }
                                mat.color = a.color;
                                mats.Add(mat);
                            }
                            else
                            {
                                ExtActionInspector.Log("Can't serialize a material: Doesn't have the property _Color", "Project Cacher", "Object: " + obj.name);
                            }
                        }
                        catch
                        {
                            ExtActionInspector.Log("Can't serialize a material...", "Project Cacher", "Object: " + obj.name);
                        }
                    }
                }
                materials = mats;
                //meshColor = rend.sharedMaterial.color;
            }

            public override bool IsObjectSupported(GameObject obj)
            {
                var rend = obj.GetComponent<MeshRenderer>();
                if (rend == null) return false;
                var filter = obj.GetComponent<MeshFilter>();
                return filter != null;
            }

            public Mesh GetMesh()
            {
                UnityEngine.Object mesh = null;
                var def = ExtCore.instance.configurations[0].GetComponent<MeshFilter>().sharedMesh;
                try
                {
                    mesh = ExtCore.GetMesh(meshInstanceID);
                    return mesh == null ? def : (Mesh)mesh;
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed getting mesh (use cube as default) [" + (mesh != null ? ((object)mesh).GetType().FullName : "No instance found") + "] : " + e.Message);
                    return def;
                }
            }
        }

        [Serializable]
        public class SerializedLine : ExtSerializables
        {
            public float lineSpeed = 15;

            public SerializedLine()
            {

            }

            public SerializedLine(LineMovement mov)
            {
                lineSpeed = mov.lineSpeed;
            }

            public override bool IsObjectSupported(GameObject obj)
            {
                var mov = obj.GetComponent<LineMovement>();
                return mov != null;
            }

            public override void ApplyTo(GameObject obj, bool now = false)
            {
                var mov = obj.AddOrGetComponent<LineMovement>();
                mov.lineSpeed = lineSpeed;
            }

            public override void TakeValues(GameObject obj)
            {
                var mov = obj.GetComponent<LineMovement>();
                lineSpeed = mov.lineSpeed;
            }
        }

        [Serializable]
        public class SerializedCamera : ExtSerializables
        {
            public Vector3 rotation;
            public Vector3 offset;
            public float distance;
            public float SmoothTime;
            public float SmoothFactor;
            public float needtime;

            public SerializedCamera()
            {

            }

            public SerializedCamera(BetterCamera cam)
            {
                TakeValues(cam.gameObject);
            }

            public override bool IsObjectSupported(GameObject obj)
            {
                var cam = obj.GetComponent<BetterCamera>();
                return cam != null;
            }

            public override void ApplyTo(GameObject obj, bool now = false)
            {
                var cam = obj.AddOrGetComponent<BetterCamera>();
                cam.TargetDistance = distance;
                cam.SmoothFactor = SmoothFactor;
                cam.SmoothTime = SmoothTime;
                cam.needtime = needtime;
                cam.targetX = rotation.x;
                cam.targetY = rotation.y;
                cam.targetZ = rotation.z;
            }

            public override void TakeValues(GameObject obj)
            {
                var cam = obj.GetComponent<BetterCamera>();
                distance = cam.TargetDistance;
                SmoothTime = cam.SmoothTime;
                SmoothFactor = cam.SmoothFactor;
                needtime = cam.needtime;
                rotation = new Vector3(cam.targetX, cam.targetY, cam.targetZ);
                offset = cam.pivotOffset;
            }
        }

        [Serializable]
        public class SerializedModTrigger : ExtSerializables
        {
            public string actualType;
            public List<ModField> modFields = new List<ModField>();
            public static ConverterMatch[] converters = {
                new ConverterMatch(typeof(float), typeof(FloatConverter)),
                new ConverterMatch(typeof(int), typeof(IntConverter)),
                new ConverterMatch(typeof(string), typeof(StringConverter)),
                new ConverterMatch(typeof(Enum), typeof(EnumConverter)),
                new ConverterMatch(typeof(GameObject), typeof(GameObjectConverter)),
                new ConverterMatch(typeof(UnityEngine.Object), typeof(UnityObjectConverter))
            };
            public SerializedModTrigger()
            {

            }

            public SerializedModTrigger(LineWorldsMod.ModTrigger trig)
            {
                actualType = trig.GetType().FullName;
                foreach (var field in trig.GetType().GetFields())
                {
                    var cls = new ModField();
                    cls.name = field.Name;
                    cls.type = field.FieldType.FullName;
                    try
                    {
                        var m = FindConverter(field.FieldType);
                        if (m == null)
                            cls.value = JsonUtility.ToJson(field.GetValue(trig));
                        else
                            cls.value = ((ModFieldConverter)Activator.CreateInstance(m.converter)).GetJsonValue(field.GetValue(trig));
                        modFields.Add(cls);
                    }
                    catch
                    {
                        Debug.LogWarning("Can't serialize " + field.Name + " in " + actualType);
                    }
                }
            }

            public override bool IsObjectSupported(GameObject obj)
            {
                var trig = obj.GetComponent<LineWorldsMod.ModTrigger>();
                return trig != null;
            }

            public override void ApplyTo(GameObject obj, bool now = false)
            {
                var type = ExtCompiler.TryGetType(actualType);
                if (type != null)
                {
                    var trig = obj.AddOrGetComponent(type);
                    foreach (var field in modFields)
                    {
                        var fieldP = type.GetField(field.name);
                        try
                        {
                            var m = FindConverter(fieldP.FieldType);
                            if (m == null)
                                fieldP.SetValue(trig, JsonConvert.DeserializeObject(field.value, fieldP.FieldType));
                            else
                            {
                                var conv = (ModFieldConverter)Activator.CreateInstance(m.converter);
                                var objV = conv.GetValidObject(field.value);
                                if (objV != null)
                                {
                                    fieldP.SetValue(trig, objV);
                                }
                                /*else
                                {
                                    ExtActionInspector.Log("Can't somehow find the type for " + fieldP.Name + " in " + actualType, "ExtDeserializer");
                                }*/
                            }
                        }
                        catch (Exception e)
                        {
                            ExtActionInspector.Log("Can't deserialize " + fieldP.Name + " in " + actualType + " => " + e.Message, "ExtDeserializer", e.StackTrace);
                        }
                    }
                    obj.GetComponent<MeshCollider>().isTrigger = true;
                }
                else
                {
                    ExtActionInspector.Log("Failed to add component " + actualType + " to " + obj.name, "ExtModTrigger", "Type might not exist");
                }
            }

            public override void TakeValues(GameObject obj)
            {
                var trig = obj.GetComponent<LineWorldsMod.ModTrigger>();
                actualType = trig.GetType().FullName;
                foreach (var field in trig.GetType().GetFields())
                {
                    var cls = new ModField();
                    cls.name = field.Name;
                    cls.type = field.FieldType.FullName;
                    try
                    {
                        var m = FindConverter(field.FieldType);
                        if (m == null)
                            cls.value = JsonUtility.ToJson(field.GetValue(trig));
                        else
                            cls.value = ((ModFieldConverter)Activator.CreateInstance(m.converter)).GetJsonValue(field.GetValue(trig));
                        modFields.Add(cls);
                    }
                    catch (Exception e)
                    {
                        ExtActionInspector.Log("Can't serialize " + field.Name + " in " + actualType + " => " + e.Message, "ExtSerializer", e.StackTrace);
                    }
                }
            }

            public static ConverterMatch FindConverter(Type type)
            {
                foreach(var match in converters)
                {
                    if (match.type == type) return match;
                    if (type.IsSubclassOf(match.type)) return match;
                }
                return null;
            }

            [Serializable]
            public class ModField
            {
                public string name;
                public string type;
                public string value;
            }

            [Serializable]
            public class ConverterMatch
            {
                public Type type;
                public Type converter;

                public ConverterMatch(Type type, Type converter)
                {
                    this.type = type;
                    this.converter = converter;
                }
            }

            public class ModFieldConverter
            {
                public virtual string type
                {
                    get
                    {
                        return null;
                    }
                }

                public virtual string GetJsonValue(object obj)
                {
                    return null;
                }

                public virtual object GetValidObject(string json)
                {
                    return null;
                }
            }

            public class StringConverter : ModFieldConverter
            {
                public override string type => "System.String";
                public override string GetJsonValue(object obj)
                {
                    return JsonUtility.ToJson(new Wrapper { str = (string)obj });
                }
                public override object GetValidObject(string json)
                {
                    return JsonUtility.FromJson<Wrapper>(json).str;
                }

                [Serializable]
                public class Wrapper
                {
                    public string str;
                }
            }

            public class FloatConverter : ModFieldConverter
            {
                public override string type => "System.Single";
                public override string GetJsonValue(object obj)
                {
                    return JsonUtility.ToJson(new Wrapper { num = (float)obj });
                }
                public override object GetValidObject(string json)
                {
                    return JsonUtility.FromJson<Wrapper>(json).num;
                }

                [Serializable]
                public class Wrapper
                {
                    public float num;
                }
            }

            public class IntConverter : ModFieldConverter
            {
                public override string type => "System.Single";
                public override string GetJsonValue(object obj)
                {
                    return JsonUtility.ToJson(new Wrapper { num = (int)obj });
                }
                public override object GetValidObject(string json)
                {
                    return JsonUtility.FromJson<Wrapper>(json).num;
                }

                [Serializable]
                public class Wrapper
                {
                    public int num;
                }
            }

            public class EnumConverter : ModFieldConverter
            {
                public override string type => "System.Enum";
                public override string GetJsonValue(object obj)
                {
                    return JsonUtility.ToJson(new Wrapper { type = obj.GetType().FullName, name = ((Enum)obj).ToString() });
                }
                public override object GetValidObject(string json)
                {
                    var cls = JsonUtility.FromJson<Wrapper>(json);
                    return Enum.Parse(Type.GetType(cls.type), cls.name);
                }

                [Serializable]
                public class Wrapper
                {
                    public string type;
                    public string name;
                }
            }

            public class GameObjectConverter : ModFieldConverter
            {
                public override string type => "UnityEngine.GameObject";
                public override string GetJsonValue(object obj)
                {
                    try
                    {
                        if (obj != null)
                        {
                            var ext = ((GameObject)obj).GetComponent<ExtObject>().instanceID;
                            return JsonUtility.ToJson(new Wrapper { instanceID = ext });
                        }
                        else
                        {
                            return JsonUtility.ToJson(new Wrapper { instanceID = -1 });
                        }
                    }
                    catch
                    {
                        return JsonUtility.ToJson(new Wrapper { instanceID = -1 });
                    }
                }
                public override object GetValidObject(string json)
                {
                    try
                    {
                        var cls = JsonUtility.FromJson<Wrapper>(json);
                        if (cls.instanceID > -1)
                        {
                            return ExtCore.GetObject(cls.instanceID).gameObject;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    catch
                    {
                        return null;
                    }
                }

                [Serializable]
                public class Wrapper
                {
                    public int instanceID;
                }
            }
            public class UnityObjectConverter : ModFieldConverter
            {
                public override string type => "UnityEngine.Object";
                public override string GetJsonValue(object obj)
                {
                    if (obj != null)
                    {
                        var ext = ((MonoBehaviour)obj).GetComponent<ExtObject>().instanceID;
                        return JsonUtility.ToJson(new Wrapper { targetType = obj.GetType().FullName, instanceID = ext });
                    }
                    else
                    {
                        return JsonUtility.ToJson(new Wrapper { targetType = null, instanceID = -1 });
                    }
                }
                public override object GetValidObject(string json)
                {
                    var cls = JsonUtility.FromJson<Wrapper>(json);
                    var type = ExtCompiler.TryGetType(cls.targetType);
                    if (cls.instanceID > -1)
                    {
                        return type == null ? null : ExtCore.GetObject(cls.instanceID).GetComponent(type);
                    }
                    else
                    {
                        return null;
                    }
                }

                [Serializable]
                public class Wrapper
                {
                    public string targetType;
                    public int instanceID;
                }
            }
        }

        [Serializable]
        public class SerializedTrigger : ExtSerializables
        {
            public TriggerCollection.TrigType type;
            public LiwAlias.Vector3 TargetAngleRotation;
            public LiwAlias.Vector3 TargetPivotOffset;
            public float TargetCamDistance;
            public float TargetSmoothing;
            public float TargetRotationSmoothing;
            public float TargetFactor;
            public bool ChangeTargetObject;
            public int TargetObjectToSee;

            // Jump
            public float HighJump;

            // Speed
            public float TargetSpeed;

            // Fog
            public float DensityTo;
            public LiwAlias.Color ColorTo;
            public float FogTime;
            // Camera Background
            public LiwAlias.Color camBG_Color;
            public float camBG_Time;

            // StopAWhile
            public float Duration;

            // Shake
            public float ShakeDuration;
            public float ShakeStrength;

            // Move Rotate Scale
            public LiwAlias.Vector3 MRSTargetVector;
            public LeanTweenType MRSEaseAnimation;
            public int MRSTargetObject;
            public bool MRSisUseGroup;
            public List<int> MRSTargetGroup = new List<int>();
            public float MRSTargetTime;
            
            // Teleport
            public int TLPTargetObject;

            // Activate
            public List<int> ActivateTargetGroup = new List<int>();

            // UTurn
            public float targetBlock1;
            public float targetBlock2;

            // FOV
            public float targetFOV;
            public float FOVTime;
            public LeanTweenType FOVAnimation;

            // Color
            public float ColorTime;
            public int ColorTarget;
            public bool ColorUseGroup;
            public List<int> ColorGroup = new List<int>();
            public LeanTweenType ColorAnimation;

            // AnalogGlitch
            /*public LeanTweenType GlitchAnimationType;
            public float ScanLineJitter;
            public float VerticalJump;
            public float HorizontalShake;
            public float ColorDrift;
            public float GlitchTime;*/

            // Arphros Support
            [JsonIgnore]
            public string targetObjectName;
            public void FindObject(List<LiwGameObject> objects)
            {
            	if(targetObjectName == "Player") 
            	{
                    switch (type)
                    {
                        case TriggerCollection.TrigType.Camera:
                            TargetObjectToSee = 1;
                            break;
                        case TriggerCollection.TrigType.Move:
                        case TriggerCollection.TrigType.Rotate:
                        case TriggerCollection.TrigType.Scale:
                            MRSTargetObject = 1;
                            break;
                        case TriggerCollection.TrigType.Teleport:
                            TLPTargetObject = 1;
                            break;
                        case TriggerCollection.TrigType.Color:
                            ColorTarget = 1;
                            break;
                    }
                    return;
            	}
            	if(targetObjectName == "Road") 
            	{
                    switch (type)
                    {
                        case TriggerCollection.TrigType.Move:
                        case TriggerCollection.TrigType.Rotate:
                        case TriggerCollection.TrigType.Scale:
		            		MRSisUseGroup = true;
		            		MRSTargetGroup.Add(69420);
                            break;
                        case TriggerCollection.TrigType.Color:
                            ColorUseGroup = true;
		            		ColorGroup.Add(69420);
                            break;
                    }
                    return;
            	}
                foreach (var obj in objects)
                {
                    if (obj.name == targetObjectName)
                    {
                        switch (type)
                        {
                            case TriggerCollection.TrigType.Camera:
                                TargetObjectToSee = obj.instanceID;
                                break;
                            case TriggerCollection.TrigType.Move:
                            case TriggerCollection.TrigType.Rotate:
                            case TriggerCollection.TrigType.Scale:
                                MRSTargetObject = obj.instanceID;
                                break;
                            case TriggerCollection.TrigType.Teleport:
                                TLPTargetObject = obj.instanceID;
                                break;
                            case TriggerCollection.TrigType.Color:
                                ColorTarget = obj.instanceID;
                                break;
                        }
                        return;
                    }
                }
            }

            public SerializedTrigger()
            {

            }

            public SerializedTrigger(GameObject trigger)
            {
                TakeValues(trigger.gameObject);
            }

            public override bool IsObjectSupported(GameObject obj)
            {
                var trig = obj.GetComponent<TriggerCollection>();
                return trig != null;
            }

            public override void ApplyTo(GameObject obj, bool now = false)
            {
                var trig = obj.AddOrGetComponent<TriggerCollection>();
                obj.GetComponent<Collider>().isTrigger = true;
                trig.TriggerTypes = type;
                trig.TargetAngleRotation = TargetAngleRotation;
                trig.TargetPivotOffset = TargetPivotOffset;
                trig.TargetCamDistance = TargetCamDistance;
                trig.TargetSmoothing = TargetSmoothing;
                trig.TargetRotationSmoothing = TargetRotationSmoothing;
                trig.TargetFactor = TargetFactor;
                trig.ChangeTargetObject = ChangeTargetObject;

                // Jump
                trig.HighJump = HighJump;

                // Speed
                trig.TargetSpeed = TargetSpeed;

                // Fog
                trig.DensityTo = DensityTo;
                trig.ColorTo = ColorTo;
                trig.FogTime = FogTime;

                // Camera Background
                trig.camBG_Color = camBG_Color;
                trig.camBG_Time = camBG_Time;

                // StopAWhile
                trig.Duration = Duration;

                // Shake
                trig.ShakeDuration = ShakeDuration;
                trig.ShakeStrength = ShakeStrength;

                // Move Rotate Scale
                trig.MRSTargetVector = MRSTargetVector;
                trig.MRSEaseAnimation = MRSEaseAnimation;
                if(MRSTargetObject > 0)
                {
                    if (now)
                    {
                        TryApplyMRSObject(trig);
                    }
                    else
                    {
                        ExtProjectManager.OnAfterLoadingProject += () =>
                        {
                            TryApplyMRSObject(trig);
                        };
                    }
                }
                trig.MRSisUseGroup = MRSisUseGroup;
                trig.MRSTargetGroup = MRSTargetGroup;
                trig.MRSTargetTime = MRSTargetTime;

                // Camera Assignment
                if (TargetObjectToSee > 0)
                {
                    if (now)
                    {
                        TryApplyObject(trig);
                    }
                    else
                    {
                        ExtProjectManager.OnAfterLoadingProject += () =>
                        {
                            TryApplyObject(trig);
                        };
                    }
                }

                // Teleport assignment
                if (TLPTargetObject > 0)
                {
                    if (now)
                    {
                        var target = ExtCore.GetObject(TLPTargetObject);
                        if (target != null)
                        {
                            trig.TLPTargetObject = target.gameObject;
                        }
                        else
                        {
                        	ExtActionInspector.Log("Can't find object", "ExtCore", "Target: " + TLPTargetObject, "Trigger: " + trig.name);
                        }
                    }
                    else
                    {
                        ExtProjectManager.OnAfterLoadingProject += () =>
                        {
                            var target = ExtCore.GetObject(TLPTargetObject);
                            if (target != null)
                            {
                                trig.TLPTargetObject = target.gameObject;
                            }
	                        else
	                        {
	                        	ExtActionInspector.Log("Can't find object", "ExtCore", "Target: " + TLPTargetObject, "Trigger: " + trig.name);
	                        }
                        };
                    }
                }

                // Activate group
                trig.ActivateTargetGroup = ActivateTargetGroup;

                // UTurn
                trig.targetBlock1 = targetBlock1;
                trig.targetBlock2 = targetBlock2;

                // FOV
                trig.targetFOV = targetFOV;
                trig.FOVTime = FOVTime;
                trig.FOVAnimation = FOVAnimation;

                // Color
                trig.ColorTime = ColorTime;
                // Color target assignment
                if (ColorTarget > 0)
                {
                    if (now)
                    {
                        var target = ExtCore.GetObject(ColorTarget);
                        if (target != null)
                        {
                            trig.ColorTarget = target.gameObject;
                        }
                        else
                        {
                        	ExtActionInspector.Log("Can't find object", "ExtCore", "Target: " + ColorTarget, "Trigger: " + trig.name);
                        }
                    }
                    else
                    {
                        ExtProjectManager.OnAfterLoadingProject += () =>
                        {
                            var target = ExtCore.GetObject(ColorTarget);
                            if (target != null)
                            {
                                trig.ColorTarget  = target.gameObject;
                            }
	                        else
	                        {
	                        	ExtActionInspector.Log("Can't find object", "ExtCore", "Target: " + ColorTarget, "Trigger: " + trig.name);
	                        }
                        };
                    }
                }
                trig.ColorUseGroup = ColorUseGroup;
                trig.ColorGroup = ColorGroup;
                trig.ColorAnimation = ColorAnimation;

                // Analog Glitch
                /*trig.GlitchAnimationType = GlitchAnimationType;
                trig.ScanLineJitter = ScanLineJitter;
                trig.VerticalJump = VerticalJump;
                trig.HorizontalShake = HorizontalShake;
                trig.ColorDrift = ColorDrift;
                trig.GlitchTime = GlitchTime;*/
            }

            public void TryApplyObject(TriggerCollection trig)
            {
                var target = ExtCore.GetObject(TargetObjectToSee);
                if (target != null)
                {
                    trig.TargetObjectToSee = target.gameObject;
                }
                else
                {
                	ExtActionInspector.Log("Can't find object", "ExtCore", "Target: " + TargetObjectToSee, "Trigger: " + trig.name);
                }
            }

            public void TryApplyMRSObject(TriggerCollection trig)
            {
                var target = ExtCore.GetObject(MRSTargetObject);
                if (target != null)
                {
                    trig.MRSTargetObject = target.gameObject;
                }
                else
                {
                	ExtActionInspector.Log("Can't find object", "ExtCore", "Target: " + MRSTargetObject, "Trigger: " + trig.name);
                }
            }

            public override void TakeValues(GameObject obj)
            {
                var trig = obj.GetComponent<TriggerCollection>();
                type = trig.TriggerTypes;
                TargetAngleRotation = trig.TargetAngleRotation;
                TargetPivotOffset = trig.TargetPivotOffset;
                TargetCamDistance = trig.TargetCamDistance;
                TargetSmoothing = trig.TargetSmoothing;
                TargetRotationSmoothing = trig.TargetRotationSmoothing;
                TargetFactor = trig.TargetFactor;
                ChangeTargetObject = trig.ChangeTargetObject;
                TargetObjectToSee = trig.TargetObjectToSee == null ? -1 : trig.TargetObjectToSee.GetComponent<ExtObject>().instanceID;

                // Jump
                HighJump = trig.HighJump;

                // Speed
                TargetSpeed = trig.TargetSpeed;

                // Fog
                DensityTo = trig.DensityTo;
                ColorTo = trig.ColorTo;
                FogTime = trig.FogTime;

                // Camera Background
                camBG_Color = trig.camBG_Color;
                camBG_Time = trig.camBG_Time;

                // StopAWhile
                Duration = trig.Duration;

                // Shake
                ShakeDuration = trig.ShakeDuration;
                ShakeStrength = trig.ShakeStrength;

                // Move Rotate Scale
                MRSTargetVector = trig.MRSTargetVector;
                MRSEaseAnimation = trig.MRSEaseAnimation;
                MRSTargetObject = trig.MRSTargetObject == null ? -1 : trig.MRSTargetObject.GetComponent<ExtObject>().instanceID;
                MRSisUseGroup = trig.MRSisUseGroup;
                MRSTargetGroup = trig.MRSTargetGroup;
                MRSTargetTime = trig.MRSTargetTime;

                // Teleport
                TLPTargetObject = trig.TLPTargetObject == null ? -1 : trig.TLPTargetObject.GetComponent<ExtObject>().instanceID;

                // Activate group
                ActivateTargetGroup = trig.ActivateTargetGroup;

                // UTurn
                targetBlock1 = trig.targetBlock1;
                targetBlock2 = trig.targetBlock2;
                
                // FOV
                targetFOV = trig.targetFOV;
                FOVTime = trig.FOVTime;
                FOVAnimation = trig.FOVAnimation;

                // Color
                ColorTime = trig.ColorTime;
                ColorTarget = trig.ColorTarget == null ? -1 : trig.ColorTarget.GetComponent<ExtObject>().instanceID;
                ColorUseGroup = trig.ColorUseGroup;
                ColorGroup = trig.ColorGroup;
                ColorAnimation = trig.ColorAnimation;

                // Analog Glitch
                /*GlitchAnimationType = trig.GlitchAnimationType;
                ScanLineJitter = trig.ScanLineJitter;
                VerticalJump = trig.VerticalJump;
                HorizontalShake = trig.HorizontalShake;
                ColorDrift = trig.ColorDrift;
                GlitchTime = trig.GlitchTime;*/
            }
        }

        [Serializable]
        public class SerializedRenderSettings : ExtSerializables
        {
            public LiwAlias.Color backgroundColor;
            public bool fog;
            public float fogDensity;
            public LiwAlias.Color fogColor;

            public SerializedRenderSettings()
            {

            }

            public SerializedRenderSettings(GameObject source)
            {
                TakeValues(source);
            }

            public override void ApplyTo(GameObject obj, bool now = false)
            {
                var settings = obj.GetComponent<ExtProjectSettings>();
                settings.backgroundColor = backgroundColor;
                RenderSettings.fog = fog;
                RenderSettings.fogDensity = fogDensity;
                RenderSettings.fogColor = fogColor;
            }

            public override void TakeValues(GameObject obj)
            {
                var settings = obj.GetComponent<ExtProjectSettings>();
                backgroundColor = settings.backgroundColor;
                fog = RenderSettings.fog;
                fogDensity = RenderSettings.fogDensity;
                fogColor = RenderSettings.fogColor;
            }
        }

        [Serializable]
        public class SerializedSpriteRenderer : ExtSerializables
        {
            public int spriteID = -1;

            public SerializedSpriteRenderer()
            {

            }

            public SerializedSpriteRenderer(GameObject source)
            {
                TakeValues(source);
            }

            public override void ApplyTo(GameObject obj, bool now = false)
            {
                var spriteRend = obj.AddOrGetComponent<SpriteRenderer>();
                Debug.Log("Trying to get customObj: " + spriteID);
                var customObj = ExtResourcesManager.instance.GetObject(spriteID);
                Debug.Log("Is custom Obj null: " + (customObj == null));
                if (customObj != null)
                    spriteRend.sprite = customObj.sprite;
            }

            public override void TakeValues(GameObject obj)
            {
                var spriteRend = obj.GetComponent<SpriteRenderer>();
                var customObj = ExtResourcesManager.instance.FindSprite(spriteRend.sprite);
                if (customObj != null)
                {
                    obj.GetComponent<ExtObject>().customObject = customObj;
                    spriteID = customObj.id;
                }
            }

            public override bool IsObjectSupported(GameObject obj)
            {
                return obj.GetComponent<SpriteRenderer>() != null;
            }
        }
    }
}
 
 