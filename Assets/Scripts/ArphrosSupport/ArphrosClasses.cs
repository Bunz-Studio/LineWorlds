using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using ExternMaker;
using ExternMaker.Serialization;

public static class LevelConverter
{
    // WIP
    public static Levelprop.LevelMainClass ConvertGameObjectIntoLevel(GameObject obj, LineMovement line, LevelManager manager, ArphrosInformation info)
    {
        return null;
    }

    public static Levelprop.GameProperties GetProperties(ArphrosInformation info)
    {
        // Game properties
        var gp = new Levelprop.GameProperties();
        gp.Speed = ExtCore.instance.lineMovement.lineSpeed;
        gp.levelname = ExtProjectManager.instance.project.info.levelName;
        gp.authorname = info.authorname;
        gp.description = ExtProjectManager.instance.project.info.description;
        gp.id = 0;
        gp.EditedTime = DateTime.UtcNow.ToLongDateString();
        gp.GameVersion = "LineWorlds" + Application.version;
        gp.Skybox = "Default";
        gp.playerCol = ExtCore.instance.lineMovement.GetComponent<MeshRenderer>().sharedMaterial.color;
        gp.musicName = ExtProjectManager.instance.project.info.musicName;
        gp.musicAuthor = ExtProjectManager.instance.project.info.musicArtist;
        var directionalLight = GetDirectionalLight();
        gp.lightRotate = directionalLight.transform.eulerAngles;
        gp.lightColor = directionalLight.color;
        gp.LevelVersion = ExtProjectManager.instance.project.info.levelVersion;

        // There is no road here...
        gp.roadCol = Color.white;
        gp.diff = info.diff;
        gp.theme = info.theme;
        gp.genre = info.genre;
        gp.socialLink = info.socialLink;
        return gp;
    }

    [System.Serializable]
    public class ArphrosInformation
    {
        public string authorname;
        public string diff;
        public string theme;
        public string genre;
        public string socialLink;
    }

    public static Light GetDirectionalLight()
    {
        var lights = UnityEngine.Object.FindObjectsOfType<Light>();
        foreach (var light in lights)
        {
            if (light.type == LightType.Directional) return light;
        }
        return null;
    }

    public static void SaveLevel(string mainPath, ArphrosInformation info)
    {
        if (ExtCore.playState != EditorPlayState.Stopped)
        {
            ExtDialogManager.Alert("You can't export while playing!");
            return;
        }

        Levelprop.LevelMainClass level = new Levelprop.LevelMainClass();
        level.level = GetProperties(info);
        level.level.GameVersion = Application.version;
        level.level.LevelVersion = level.level.LevelVersion + 1;

        var gm = ExtCore.instance.levelManager;
        level.level.Speed = ExtCore.instance.lineMovement.lineSpeed;
        var directionalLight = GetDirectionalLight();
        level.level.lightColor = directionalLight.color;
        level.level.lightRotate = directionalLight.transform.eulerAngles;
        level.level.playerCol = ExtCore.instance.lineMovement.GetComponent<MeshRenderer>().sharedMaterial.color;

        // Objects
        level.objects = new List<Levelprop.ObjInformation>();
        level.triggers = new List<Levelprop.TriggerInformation>();
        level.resources = new List<Levelprop.Resources>();


        foreach (ExtObject allObjects in UnityEngine.Object.FindObjectsOfType<ExtObject>())
        {
            if (allObjects.tag != "Player")
            {
                var trig = allObjects.GetComponent<TriggerCollection>();
                if (trig == null)
                {
                    if (allObjects.objectType == ExtObject.ObjectType.Sprite)
                    {
                        // Sprites
                        var gameObj = allObjects.gameObject;
                        var re = new Levelprop.Resources();
                        re.color = gameObj.GetComponent<SpriteRenderer>().color;
                        re.GroupID = allObjects.groupID;
                        re.isObstacle = false;
                        re.objName = gameObj.name + allObjects.instanceID;
                        re.pos = gameObj.transform.position;
                        re.scale = gameObj.transform.localScale;
                        re.rotate = gameObj.transform.eulerAngles;
                        re.src = gameObj.GetComponent<SpriteRenderer>().sprite.name;
                        level.resources.Add(re);
                    }
                    else if (allObjects.objectType == ExtObject.ObjectType.Custom)
                    {
                        // Custom Meshes
                        var gameObj = allObjects.gameObject;
                        var re = new Levelprop.Resources();
                        re.color = gameObj.GetComponent<Renderer>().material.color;
                        re.GroupID = allObjects.groupID;
                        re.isObstacle = gameObj.tag == "Obstacle";
                        re.objName = gameObj.name + allObjects.instanceID;
                        re.pos = gameObj.transform.position;
                        re.scale = gameObj.transform.localScale;
                        re.rotate = gameObj.transform.eulerAngles;
                        re.src = allObjects.customObject.name + allObjects.customObject.extension;
                        level.resources.Add(re);
                    }
                    else
                    {
                        // Object
                        var gameObj = allObjects.gameObject;
                        var obj = new Levelprop.ObjInformation();
                        obj.GroupID = allObjects.groupID;
                        if (gameObj.GetComponent<MeshRenderer>())
                        {
                            obj.colour = gameObj.GetComponent<MeshRenderer>().material.color;
                            obj.isObstacle = gameObj.tag == "Obstacle";
                            var meshname = gameObj.GetComponent<MeshFilter>().mesh.name.Split(' ')[0];
                            obj.ObjectType = meshname;

                            // Half Pyramid support coming soon

                            /*if (gameObj.GetComponent<ObjectTagger>())
                            {
                                if (gameObj.GetComponent<ObjectTagger>().tagName == "HalfPyramid")
                                {
                                    obj.ObjectType = "HalfPyramid";
                                }
                            }*/
                        }
                        else if (gameObj.GetComponent<Light>() && gameObj.GetComponent<Light>().type == LightType.Point)
                        {
                            obj.ObjectType = "PointLight";
                            obj.colour = gameObj.GetComponent<Light>().color;
                            obj.isObstacle = false;
                        }

                        obj.ObjectName = gameObj.name + allObjects.instanceID;

                        obj.position = gameObj.transform.position;
                        obj.rotate = gameObj.transform.rotation;
                        obj.scale = gameObj.transform.localScale;
                        level.objects.Add(obj);
                    }
                }
                else
                {
                    // Trigger

                    var t = new Levelprop.TriggerInformation();
                    t.TrigType = Enum.GetName(typeof(TriggerCollection.TrigType), trig.TriggerTypes);
                    t.TrigName = trig.gameObject.name + allObjects.instanceID;
                    switch (trig.TriggerTypes)
                    {
                        case TriggerCollection.TrigType.Jump:
                            t.dataFloat1 = trig.HighJump;
                            break;
                        case TriggerCollection.TrigType.StopAWhile:
                            t.dataFloat1 = trig.Duration;
                            break;
                        case TriggerCollection.TrigType.Speed:
                            t.dataFloat1 = trig.TargetSpeed;
                            break;
                        case TriggerCollection.TrigType.Finish:
                            break;

                        // Sadly unsupported triggers

                        /*case TriggerCollection.TrigType.Teleport:
                            t.dataString1 = trig.TLPTargetObject;
                            break;
                        case TriggerCollection.TrigType.Activate:
                            t.TrigGroup = trig.ActivateTargetGroup;
                            break;
                        case TriggerCollection.TrigType.UTurn:
                            t.dataVec1 = trig.targetBlock1;
                            t.dataVec2 = trig.targetBlock2;
                            break;*/

                        case TriggerCollection.TrigType.Move:
                        case TriggerCollection.TrigType.Scale:
                        case TriggerCollection.TrigType.Rotate:
                            if (trig.MRSisUseGroup)
                            {
                                t.isUseGroup = true;
                                t.TrigGroup = trig.MRSTargetGroup;
                            }
                            t.dataString2 = trig.MRSEaseAnimation.ToString();
                            t.dataString1 = trig.MRSTargetObject.name + trig.MRSTargetObject.GetComponent<ExtObject>().instanceID;
                            t.dataFloat1 = trig.MRSTargetTime;
                            t.dataVec1 = trig.MRSTargetVector;
                            break;
                        case TriggerCollection.TrigType.Camera:
                            if (trig.ChangeTargetObject)
                            {
                                t.dataString1 = trig.TargetObjectToSee.name + trig.TargetObjectToSee.GetComponent<ExtObject>().instanceID;
                            }
                            t.dataVec1 = trig.TargetAngleRotation;
                            t.dataVec2 = trig.TargetPivotOffset;
                            t.dataFloat1 = trig.TargetCamDistance;
                            t.dataFloat2 = trig.TargetSmoothing;
                            t.dataFloat3 = trig.TargetRotationSmoothing;
                            t.dataFloat4 = trig.TargetFactor;
                            break;

                        /*case TriggerCollection.TrigType.Code:
                            t.dataString1 = trig.codeFileName;
                            break;*/

                        case TriggerCollection.TrigType.ShakeCam:
                            t.dataFloat1 = trig.ShakeDuration;
                            t.dataFloat2 = trig.ShakeStrength;
                            break;
                        case TriggerCollection.TrigType.Fog:
                            t.dataFloat1 = trig.DensityTo;
                            t.dataFloat2 = trig.FogTime;
                            t.dataColor1 = trig.ColorTo;
                            break;

                        /*case TriggerCollection.TrigType.Fov:
                            t.dataFloat1 = trig.FOVTime;
                            t.dataFloat2 = trig.targetFOV;
                            t.dataString1 = trig.FOVAnimation;
                            break;

                        case TriggerCollection.TrigType.Color:
                            t.dataColor1 = trig.ColorTo;
                            t.dataFloat1 = trig.ColorTime;
                            t.dataString1 = trig.ColorAnimation;
                            t.dataString2 = trig.ColorTarget;
                            break;*/
                        default:
                            ExtActionInspector.Log("Trigger unsupported in Arphros", "Arphros Exporter", "Trigger Name: " + trig.name, "Trigger Type: " + trig.TriggerTypes.ToString("g"));
                            break;
                    }
                    t.scale = trig.transform.localScale;
                    t.rotate = trig.transform.rotation;
                    t.position = trig.transform.position;
                    t.ObjectType = "Trigger";
                    t.GroupID = trig.GetComponent<ExtObject>().groupID;

                    level.triggers.Add(t);
                }
            }

        }
        //To File
        string json = JsonUtility.ToJson(level);
        Debug.Log("File saved at: " + Path.Combine(mainPath, "level.popyl"));
        File.WriteAllText(Path.Combine(mainPath, "level.popyl"), json);
    }
}

public class Level
{
    [Serializable]
    public class levels
    {
        public List<level> lvls;
        public int maxPage;
    }
    [Serializable]
    public class level
    {
        public int id;
        public string name;
        public string description;
        public int authorid;
        public string authorname;
        public bool verify;
        public string diff;
        public int version;
        public string sociallink;
        public string theme;
        public string genre;
        public int coinsrate;
        public int time;
        public int downloadcount;
        public string musicname;
        public string musicauthor;
        public string bg64;
        public string avatar64;
        public string songPreview;
    }

    [Serializable]
    public class PublishData
    {
        public int id;
        public string fileZip64;
        public string banner64;
        public string levelName;
        public int authorId;
        public string description;
        public string difficulty;
        public int version;
        public string socialLink;
        public string theme;
        public string genre;
        public string musicName;
        public string musicAuthor;
    }
    
    [Serializable]
    public class OldObjectsInfo
    {
        // Use for make ctts levels supports arphros only!!!!!!!!!!!!!!!!!!!!!!!!
        [Serializable]
        public class LevelMainClass
        {
            public Levelprop.GameProperties level;
            public List<ObjInformation> objects;
            public List<TriggerInformation> triggers;
            public List<ResourcesImg> imgs;
            public List<ResourcesObj> cobjs;
        }
        [Serializable]
        public class ObjInformation
        {
            public Vector3 position;
            public Vector3 scale;
            public Quaternion rotate;
            public string ObjectName;
            public string ObjectType;
            public bool isObstacle;
            public Color colour;
            public int GroupID;

        }
        [Serializable]
        public class TriggerInformation
        {
            public Vector3 position;
            public Vector3 scale;
            public Quaternion rotate;

            // data stocks
            public Vector3 dataVec1;
            public Vector3 dataVec2;

            public float dataFloat1;
            public float dataFloat2;
            public float dataFloat3;
            public float dataFloat4;

            public string dataString1;
            public string dataString2;

            public Color dataColor1;
            //

            public string ObjectType;
            public string TrigType;

            public int GroupID;
            public string TrigName;

            public bool isUseGroup;
            public int TrigGroup;
        }
        [Serializable]
        public class ResourcesImg
        {
            public string objName;
            public int GroupID;
            public string imgName;
            public Vector3 pos;
            public Vector3 scale;
            public Vector3 rotate;
            public string lvlname;
        }
        [Serializable]
        public class ResourcesObj
        {
            public string objName;
            public int GroupID;
            public string src;
            public Vector3 pos;
            public Vector3 scale;
            public Vector3 rotate;
            public string lvlname;
            public Color color;
            public bool isObstacle;
        }
    }

    public static void ConvertClvobiToPopy(string mainPath, level lvlinf)
    {
        // MAKE IT SUPPORTS AND DON'T DARE TO FIX THIS AGAIN PLS OwO
        Levelprop.LevelMainClass mainClass = new Levelprop.LevelMainClass();
        mainClass.level = new Levelprop.GameProperties();

        mainClass.objects = new List<Levelprop.ObjInformation>();
        mainClass.resources = new List<Levelprop.Resources>();
        mainClass.triggers = new List<Levelprop.TriggerInformation>();

        if (File.Exists(Path.Combine(mainPath, "level.ctts")))
        {
            var cttsPath = Path.Combine(mainPath, "level.ctts");
            OldObjectsInfo.LevelMainClass lvl = JsonUtility.FromJson<OldObjectsInfo.LevelMainClass>(File.ReadAllText(cttsPath));
            mainClass.level = lvl.level;
            mainClass.level.levelname = lvlinf.name;
            mainClass.level.authorname = lvlinf.authorname;
            mainClass.level.description = lvlinf.description;
            mainClass.level.diff = lvlinf.diff;
            mainClass.level.genre = lvlinf.genre;
            mainClass.level.theme = lvlinf.theme;

            mainClass.objects = new List<Levelprop.ObjInformation>();
            foreach (OldObjectsInfo.ObjInformation soOLD in lvl.objects)
            {
                //Objects
                Levelprop.ObjInformation obj = new Levelprop.ObjInformation();

                obj.colour = new Color(soOLD.colour.r, soOLD.colour.g, soOLD.colour.b, 1);
                obj.isObstacle = soOLD.isObstacle;
                obj.ObjectName = soOLD.ObjectName;
                obj.ObjectType = soOLD.ObjectType;
                obj.position = soOLD.position;
                obj.rotate = soOLD.rotate;
                obj.scale = soOLD.scale;

                obj.GroupID.Add(soOLD.GroupID);
                
                mainClass.objects.Add(obj);
            }
            mainClass.resources = new List<Levelprop.Resources>();
            foreach (OldObjectsInfo.ResourcesObj soOLD in lvl.cobjs)
            {
                Levelprop.Resources r = new Levelprop.Resources();

                r.objName = soOLD.objName;
                r.pos = soOLD.pos;
                r.rotate = soOLD.rotate;
                r.scale = soOLD.scale;
                r.src = soOLD.src;
                r.color = new Color(soOLD.color.r, soOLD.color.g, soOLD.color.b, 1);
                r.isObstacle = soOLD.isObstacle;
                r.GroupID.Add(soOLD.GroupID);
                mainClass.resources.Add(r);
            }
            foreach (OldObjectsInfo.ResourcesImg soOLD in lvl.imgs)
            {
                //Resources Objects
                Levelprop.Resources r = new Levelprop.Resources();
                
                r.objName = soOLD.objName;
                r.pos = soOLD.pos;
                r.rotate = soOLD.rotate;
                r.scale = soOLD.scale;
                r.src = soOLD.imgName;
                r.GroupID.Add(soOLD.GroupID);
                mainClass.resources.Add(r);
            }
            mainClass.triggers = new List<Levelprop.TriggerInformation>();
            foreach (OldObjectsInfo.TriggerInformation soOLD in lvl.triggers)
            {
                //Triggers
                Levelprop.TriggerInformation trg = new Levelprop.TriggerInformation();
                
                trg.dataColor1 = soOLD.dataColor1;
                trg.dataFloat1 = soOLD.dataFloat1;
                trg.dataFloat2 = soOLD.dataFloat2;
                trg.dataFloat3 = soOLD.dataFloat3;
                trg.dataFloat4 = soOLD.dataFloat4;
                trg.dataString1 = soOLD.dataString1;
                trg.dataString2 = soOLD.dataString2;
                trg.dataVec1 = soOLD.dataVec1;
                trg.dataVec2 = soOLD.dataVec2;
                trg.isUseGroup = soOLD.isUseGroup;
                trg.ObjectType = soOLD.ObjectType;
                trg.position = soOLD.position;
                trg.rotate = soOLD.rotate;
                trg.scale = soOLD.scale;
                trg.TrigType = soOLD.TrigType;
                trg.TrigName = soOLD.TrigName;

                int groupID = soOLD.TrigGroup;
                int targetGroup = soOLD.GroupID;
                trg.TrigGroup.Add(targetGroup);
                trg.GroupID.Add(groupID);
                mainClass.triggers.Add(trg);
            }
            
            mainClass.level.GameVersion = "ctts";
            File.Delete(Path.Combine(mainPath, "level.ctts"));
            mainClass.level.GameVersion = "ctts";
            mainClass.level.id = lvlinf.id;
            mainClass.level.LevelVersion = lvlinf.version;
            mainClass.level.authorid = lvlinf.authorid;
            File.WriteAllText(Path.Combine(mainPath, "level.popy"), JsonUtility.ToJson(mainClass));
        }
        else
        {
            if (File.Exists(Path.Combine(mainPath, "level.popy")))
            {
                Levelprop.LevelMainClass lvl = JsonUtility.FromJson<Levelprop.LevelMainClass>(File.ReadAllText(Path.Combine(mainPath, "level.popy")));
                mainClass = lvl;
                mainClass.level.id = lvlinf.id;
                File.WriteAllText(Path.Combine(mainPath, "level.popy"), JsonUtility.ToJson(mainClass));
            }
            if (File.Exists(Path.Combine(mainPath, "level.prop")))
            {
                Levelprop.GameProperties levelProperties = JsonUtility.FromJson<Levelprop.GameProperties>(File.ReadAllText(Path.Combine(mainPath, "level.prop")));
                mainClass.level = levelProperties;
                mainClass.level.levelname = lvlinf.name;
                mainClass.level.authorname = lvlinf.authorname;
                mainClass.level.description = lvlinf.description;
                mainClass.level.diff = lvlinf.diff;
                mainClass.level.genre = lvlinf.genre;
                mainClass.level.theme = lvlinf.theme;
                File.Delete(Path.Combine(mainPath, "level.prop"));
                string[] clvs = Directory.GetFiles(mainPath, "*.clv");
                foreach (string clv in clvs)
                {
                    //Objects
                    string json = File.ReadAllText(clv);
                    Levelprop.ObjInformation obj = new Levelprop.ObjInformation();
                    OldObjectsInfo.ObjInformation soOLD = JsonUtility.FromJson<OldObjectsInfo.ObjInformation>(json);

                    obj.colour = soOLD.colour;
                    obj.isObstacle = soOLD.isObstacle;
                    obj.ObjectName = soOLD.ObjectName;
                    obj.ObjectType = soOLD.ObjectType;
                    obj.position = soOLD.position;
                    obj.rotate = soOLD.rotate;
                    obj.scale = soOLD.scale;

                    obj.GroupID.Add(soOLD.GroupID);
                    
                    mainClass.objects.Add(obj);
                    File.Delete(clv);
                }
                string[] obis = Directory.GetFiles(mainPath, "*.obi");
                foreach (string obi in obis)
                {
                    //Resources Objects
                    string json = File.ReadAllText(obi);
                    Levelprop.Resources resource = new Levelprop.Resources();
                    OldObjectsInfo.ResourcesObj oldResource = JsonUtility.FromJson<OldObjectsInfo.ResourcesObj>(json);

                    resource.objName = oldResource.objName;
                    resource.pos = oldResource.pos;
                    resource.rotate = oldResource.rotate;
                    resource.scale = oldResource.scale;
                    resource.src = oldResource.src;
                    resource.color = oldResource.color;
                    resource.isObstacle = oldResource.isObstacle;
                    resource.GroupID.Add(oldResource.GroupID);
                    mainClass.resources.Add(resource);
                    File.Delete(obi);
                }
                string[] sprs = Directory.GetFiles(mainPath, "*.spr");
                foreach (string spr in sprs)
                {
                    //Resources Objects
                    string json = File.ReadAllText(spr);
                    Levelprop.Resources r = new Levelprop.Resources();
                    OldObjectsInfo.ResourcesImg soOLD = JsonUtility.FromJson<OldObjectsInfo.ResourcesImg>(json);
                    
                    r.objName = soOLD.objName;
                    r.pos = soOLD.pos;
                    r.rotate = soOLD.rotate;
                    r.scale = soOLD.scale;
                    r.src = soOLD.imgName;
                    r.GroupID.Add(soOLD.GroupID);
                    mainClass.resources.Add(r);
                    File.Delete(spr);
                }
                string[] ctgs = Directory.GetFiles(mainPath, "*.ctg");
                foreach (string ctg in ctgs)
                {
                    //Triggers
                    string json = File.ReadAllText(ctg);
                    OldObjectsInfo.TriggerInformation soOLD = JsonUtility.FromJson<OldObjectsInfo.TriggerInformation>(json);
                    Levelprop.TriggerInformation trg = new Levelprop.TriggerInformation();
                    
                    trg.dataColor1 = soOLD.dataColor1;
                    trg.dataFloat1 = soOLD.dataFloat1;
                    trg.dataFloat2 = soOLD.dataFloat2;
                    trg.dataFloat3 = soOLD.dataFloat3;
                    trg.dataFloat4 = soOLD.dataFloat4;
                    trg.dataString1 = soOLD.dataString1;
                    trg.dataString2 = soOLD.dataString2;
                    trg.dataVec1 = soOLD.dataVec1;
                    trg.dataVec2 = soOLD.dataVec2;
                    trg.isUseGroup = soOLD.isUseGroup;
                    trg.ObjectType = soOLD.ObjectType;
                    trg.position = soOLD.position;
                    trg.rotate = soOLD.rotate;
                    trg.scale = soOLD.scale;
                    trg.TrigType = soOLD.TrigType;
                    trg.TrigName = soOLD.TrigName;

                    int groupID = soOLD.TrigGroup;
                    int targetGroup = soOLD.GroupID;
                    trg.TrigGroup.Add(targetGroup);
                    trg.GroupID.Add(groupID);
                    mainClass.triggers.Add(trg);
                    File.Delete(ctg);
                }
                mainClass.level.GameVersion = "ctts";
                mainClass.level.id = lvlinf.id;
                mainClass.level.LevelVersion = lvlinf.version;
                mainClass.level.authorid = lvlinf.authorid;
                File.WriteAllText(Path.Combine(mainPath, "level.popy"), JsonUtility.ToJson(mainClass));
            }
        }
    }
}

public class Levelprop
{
    [Serializable]
    public class LevelMainClass
    {
        [NonSerialized]
        public string popylPath;

        public GameProperties level;
        public List<ObjInformation> objects;
        public List<TriggerInformation> triggers;
        public List<Resources> resources;

        public void ToLiwb(string path)
        {
            var liwb = new LiwbProject();

            // Informations
            liwb.project.info.authorID = level.authorid;
            liwb.project.info.description = level.description;
            liwb.project.info.levelName = level.levelname;
            liwb.project.info.levelID = level.id;
            liwb.project.info.levelVersion = level.LevelVersion;
            liwb.project.info.gameVersion = "Arphros";

            liwb.project.cameraInfo.instanceID = 2;
            
            // Fog
            liwb.project.renderSettings.fog = true;

            // Line
            List<LiwGameObject> registeredObjects = new List<LiwGameObject>();
            var s_line = new Serializables.SerializedLine();
            s_line.lineSpeed = level.Speed;
            var s_lineRenderer = new Serializables.SerializedMesh();
            s_lineRenderer.meshInstanceID = 0;
            s_lineRenderer.meshColor = level.playerCol;
            liwb.project.lineInfo.components.Add(s_line.GetData());
            liwb.project.lineInfo.components.Add(s_lineRenderer.GetData());
            liwb.project.lineInfo.instanceID = 1;
            liwb.project.lineInfo.name = "Player";
            liwb.project.lineInfo.tag = "Player";
            liwb.project.lineInfo.transform.position = new Vector3(0, 1, 0);
            registeredObjects.Add(liwb.project.lineInfo);
            int lastInstance = 2;

            // Light? (Unfortunately this isn't supported yet
            /*var light = LevelConverter.GetDirectionalLight();
            light.color = level.lightColor;
            light.transform.eulerAngles = level.lightRotate;*/

            // Objects
            foreach(var obj in objects)
            {
                lastInstance++;
                var gameObj = new LiwGameObject();
                gameObj.instanceID = lastInstance;
                gameObj.groupIDs = obj.GroupID;
                gameObj.tag = obj.isObstacle ? "Obstacle" : "Untagged";
                gameObj.transform.position = obj.position;
                gameObj.transform.rotation = obj.rotate.eulerAngles;
                gameObj.transform.scale = obj.scale;
                gameObj.name = obj.ObjectName;
                if(obj.ObjectType == "Road") 
                {
                	gameObj.groupIDs.Add(69420);
                }
                var s_objRenderer = GetMesh(obj);
	            gameObj.components.Add(s_objRenderer.GetData());
                liwb.project.gameObjects.Add(gameObj);
                registeredObjects.Add(gameObj);
            }

            // Triggers
            var triggersRegistered = new List<TriggerTemp>();
            foreach (var trig in triggers)
            {
                lastInstance++;

                var temp = new TriggerTemp();
                var obj = new LiwGameObject();
                obj.instanceID = lastInstance;

                temp.gameObject = obj;

                obj.transform.position = trig.position;
                obj.transform.scale = trig.scale;
                obj.transform.rotation = trig.rotate.eulerAngles;
                obj.name = trig.TrigName;

                var g = new Serializables.SerializedTrigger();
                obj.groupIDs = trig.GroupID;

                var s_objRenderer = new Serializables.SerializedMesh();
                s_objRenderer.meshInstanceID = 0;
                obj.components.Add(s_objRenderer.GetData());

                var t = g;
                t.type = TriggerInformation.GetTrigType(trig.TrigType);
                switch (trig.TrigType)
                {
                    case "Jump":
                        t.type = TriggerCollection.TrigType.Jump;
                        t.HighJump = trig.dataFloat1;
                        break;
                    case "Speed":
                        t.type = TriggerCollection.TrigType.Speed;
                        t.TargetSpeed = trig.dataFloat1;
                        break;
                    case "Move":
                    case "Scale":
                    case "Rotate":
                        t.MRSEaseAnimation = TriggerInformation.GetTweenType(trig.dataString2);
                        t.MRSisUseGroup = trig.isUseGroup;
                        t.MRSTargetGroup = trig.TrigGroup;
                        t.targetObjectName = trig.dataString1;
                        t.MRSTargetTime = trig.dataFloat1;
                        t.MRSTargetVector = trig.dataVec1;
                        break;
                    case "Teleport":
                        t.type = TriggerCollection.TrigType.Teleport;
                        t.targetObjectName = trig.dataString1;
                        break;
                    case "Activate":
                        t.type = TriggerCollection.TrigType.Activate;
                        t.ActivateTargetGroup = trig.TrigGroup;
                        break;
                    case "Camera":
                        t.type = TriggerCollection.TrigType.Camera;
                        if (trig.dataString1 != "")
                        {
                            t.ChangeTargetObject = true;
                            t.targetObjectName = trig.dataString1;
                        }
                        t.TargetAngleRotation = trig.dataVec1;
                        t.TargetPivotOffset = trig.dataVec2;
                        t.TargetCamDistance = trig.dataFloat1;
                        t.TargetSmoothing = trig.dataFloat2;
                        t.TargetRotationSmoothing = trig.dataFloat3;
                        if (trig.dataFloat4 != 0)
                        {
                            t.TargetFactor = trig.dataFloat4;
                        }
                        break;
                    case "UTurn":
                        t.type = TriggerCollection.TrigType.UTurn;
                        t.targetBlock1 = trig.dataVec1.y;
                        t.targetBlock2 = trig.dataVec2.y;
                        break;
                    case "Finish":
                        t.type = TriggerCollection.TrigType.Finish;
                        break;
                    case "ShakeCam":
                        t.type = TriggerCollection.TrigType.ShakeCam;
                        t.ShakeDuration = trig.dataFloat1;
                        t.ShakeStrength = trig.dataFloat2;
                        break;
                    case "Fog":
                        t.type = TriggerCollection.TrigType.Fog;
                        t.FogTime = trig.dataFloat2;
                        t.DensityTo = trig.dataFloat1;
                        t.ColorTo = trig.dataColor1;
                        break;
                    case "Fov":
                        t.type = TriggerCollection.TrigType.Fov;
                        t.FOVTime = trig.dataFloat1;
                        t.targetFOV = trig.dataFloat2;
                        t.FOVAnimation = TriggerInformation.GetTweenType(trig.dataString1);
                        break;
                    case "Color":
                        t.type = TriggerCollection.TrigType.Color;
                        t.ColorTo = trig.dataColor1;
                        t.ColorTime = trig.dataFloat1;
                        t.ColorAnimation = TriggerInformation.GetTweenType(trig.dataString1);
                        t.targetObjectName = trig.dataString2;
                        t.ColorUseGroup = trig.isUseGroup;
                        t.ColorGroup = trig.TrigGroup;
                        break;
                    case "ClearTails":
                        t.type = TriggerCollection.TrigType.ClearTails;
                        break;
                    case "StopAWhile":
                        t.type = TriggerCollection.TrigType.StopAWhile;
                        t.Duration = trig.dataFloat1;
                        break;
                        /*case "AnalogGlitch":{
                            t.type = TriggerCollection.TrigType.AnalogGlitch;
                            t.GlitchTime = trig.dataFloat1;
                            t.ColorDrift = trig.dataFloat2;
                            t.GlitchAnimationType = trig.dataString1;
                            t.HorizontalShake = trig.dataFloat3;
                            t.ScanLineJitter = trig.dataFloat4;
                            t.VerticalJump = trig.dataFloat5;
                        }*/
                        //t.FindObject();
                }
                temp.trigger = t;
                triggersRegistered.Add(temp);
                registeredObjects.Add(obj);
                //obj.components.Add(t.GetData());
            }
            foreach (var trig in triggersRegistered)
            {
                trig.trigger.FindObject(registeredObjects);
                trig.gameObject.components.Add(trig.trigger.GetData());
                liwb.project.gameObjects.Add(trig.gameObject);
            }

            // Files
            var directoryPath = ExtUtility.GetRidOfLastPath(popylPath);
            var songPath = Path.Combine(directoryPath, "song.ogg");
            if (File.Exists(songPath))
            {
                var songFile = new LiwbFile(songPath);
                liwb.audioFile = songFile;
            }

            File.WriteAllText(path, JsonUtility.ToJson(liwb));
        }
        public class TriggerTemp
        {
            public Serializables.SerializedTrigger trigger;
            public LiwGameObject gameObject;
        }
        public Serializables.SerializedMesh GetMesh(ObjInformation obj)
        {
            var s_objRenderer = new Serializables.SerializedMesh();
	        s_objRenderer.meshColor = obj.colour;
	        switch(obj.ObjectType)
	        {
	        	case "Cube":
	        	case "Road":
                case "Obstacle":
		            s_objRenderer.meshInstanceID = 0;
	                return s_objRenderer;
	            case "Capsule":
		            s_objRenderer.meshInstanceID = 1;
	                return s_objRenderer;
                case "Cylinder":
                    s_objRenderer.meshInstanceID = 2;
                    return s_objRenderer;
                case "Sphere":
                    s_objRenderer.meshInstanceID = 3;
                    return s_objRenderer;
                default:
                    s_objRenderer.meshInstanceID = 0;
                    ExtActionInspector.Log("ObjectType not supported", "Arphros Converter", "Type: " + obj.ObjectType);
                    return s_objRenderer;
            }

            // Unsupported types
            /*else if (obj.ObjectType == "Plane")
            {
	            s_objRenderer.meshInstanceID = 0;
	            ExtActionInspector.Log("ObjectType not supported", "Arphros Converter", "Type: " + obj.ObjectType);
                return s_objRenderer;
            }
            if (obj.ObjectType == "Quad")
            {
	            s_objRenderer.meshInstanceID = 0;
                return s_objRenderer;
            }
            if (obj.ObjectType == "Finish")
            {
	            s_objRenderer.meshInstanceID = 0;
	            ExtActionInspector.Log("ObjectType not supported", "Arphros Converter", "Type: " + obj.ObjectType);
                return s_objRenderer;
            }
            if (obj.ObjectType == "RoadRiged")
            {
                e = GameObject.CreatePrimitive(PrimitiveType.Cube);
                e.tag = "Finish";
            }
            if (obj.ObjectType == "HalfPyramid")
            {
                e = Instantiate(HalfPyramid);
            }
            if (obj.ObjectType == "PointLight")
            {
                e = Instantiate(FindObjectOfType<LevelEditor>().Objects[1]);
                var l = e.GetComponent<Light>();
                l.color = obj.colour;
                l.type = LightType.Point;
            }*/
        }
    }
    [Serializable]
    public class ObjInformation
    {
        public Vector3 position;
        public Vector3 scale;
        public Quaternion rotate;
        public string ObjectName;
        public string ObjectType;
        public bool isObstacle;
        public Color colour;
        public List<int> GroupID = new List<int>();
    }
    [Serializable]
    public class TriggerInformation
    {
        public Vector3 position;
        public Vector3 scale;
        public Quaternion rotate;

        //data stocks
        public Vector3 dataVec1;
        public Vector3 dataVec2;

        public float dataFloat1;
        public float dataFloat2;
        public float dataFloat3;
        public float dataFloat4;

        public string dataString1;
        public string dataString2;

        public Color dataColor1;

        public string ObjectType;
        public string TrigType;

        public List<int> GroupID = new List<int>();
        public string TrigName;

        public bool isUseGroup;
        public List<int> TrigGroup = new List<int>();

        public LiwGameObject ToLiwGameObject()
        {
            var obj = new LiwGameObject();
            var trig = this;

            obj.transform.position = trig.position;
            obj.transform.scale = trig.scale;
            obj.transform.rotation = trig.rotate.eulerAngles;
            obj.name = trig.TrigName;

            var g = new Serializables.SerializedTrigger();
            obj.groupIDs = trig.GroupID;

            var s_objRenderer = new Serializables.SerializedMesh();
            s_objRenderer.meshInstanceID = 0;
            obj.components.Add(s_objRenderer.GetData());

            var t = g;
            t.type = GetTrigType(trig.TrigType);
            switch (trig.TrigType)
            {
                case "Jump":
                    t.type = TriggerCollection.TrigType.Jump;
                    t.HighJump = trig.dataFloat1;
                    break;
                case "Speed":
                    t.type = TriggerCollection.TrigType.Speed;
                    t.TargetSpeed = trig.dataFloat1;
                    break;
                case "Move":
                case "Scale":
                case "Rotate":
                    t.MRSEaseAnimation = GetTweenType(trig.dataString2);
                    t.MRSisUseGroup = trig.isUseGroup;
                    t.MRSTargetGroup = trig.TrigGroup;
                    t.targetObjectName = trig.dataString1;
                    t.MRSTargetTime = trig.dataFloat1;
                    t.MRSTargetVector = trig.dataVec1;
                    break;
                case "Teleport":
                    t.type = TriggerCollection.TrigType.Teleport;
                    t.targetObjectName = trig.dataString1;
                    break;
                case "Activate":
                    t.type = TriggerCollection.TrigType.Activate;
                    t.ActivateTargetGroup = trig.TrigGroup;
                    break;
                case "Camera":
                    t.type = TriggerCollection.TrigType.Camera;
                    if (trig.dataString1 != "")
                    {
                        t.ChangeTargetObject = true;
                        t.targetObjectName = trig.dataString1;
                    }
                    t.TargetAngleRotation = trig.dataVec1;
                    t.TargetPivotOffset = trig.dataVec2;
                    t.TargetCamDistance = trig.dataFloat1;
                    t.TargetSmoothing = trig.dataFloat2;
                    t.TargetRotationSmoothing = trig.dataFloat3;
                    if (trig.dataFloat4 != 0)
                    {
                        t.TargetFactor = trig.dataFloat4;
                    }
                    break;
                case "UTurn":
                    t.type = TriggerCollection.TrigType.UTurn;
                    t.targetBlock1 = trig.dataVec1.y;
                    t.targetBlock2 = trig.dataVec2.y;
                    break;
                case "Finish":
                    t.type = TriggerCollection.TrigType.Finish;
                    break;
                case "ShakeCam":
                    t.type = TriggerCollection.TrigType.ShakeCam;
                    t.ShakeDuration = trig.dataFloat1;
                    t.ShakeStrength = trig.dataFloat2;
                    break;
                case "Fog":
                    t.type = TriggerCollection.TrigType.Fog;
                    t.FogTime = trig.dataFloat2;
                    t.DensityTo = trig.dataFloat1;
                    t.ColorTo = trig.dataColor1;
                    break;
                case "Fov":
                    t.type = TriggerCollection.TrigType.Fov;
                    t.FOVTime = trig.dataFloat1;
                    t.targetFOV = trig.dataFloat2;
                    t.FOVAnimation = GetTweenType(trig.dataString1);
                    break;
                case "Color":
                    t.type = TriggerCollection.TrigType.Color;
                    t.ColorTo = trig.dataColor1;
                    t.ColorTime = trig.dataFloat1;
                    t.ColorAnimation = GetTweenType(trig.dataString1);
                    t.targetObjectName = trig.dataString2;
                    t.ColorUseGroup = trig.isUseGroup;
                    t.ColorGroup = trig.TrigGroup;
                    break;
                case "ClearTails":
                    t.type = TriggerCollection.TrigType.ClearTails;
                    break;
                case "StopAWhile":
                    t.type = TriggerCollection.TrigType.StopAWhile;
                    t.Duration = trig.dataFloat1;
                    break;
                    /*case "AnalogGlitch":{
                        t.type = TriggerCollection.TrigType.AnalogGlitch;
                        t.GlitchTime = trig.dataFloat1;
                        t.ColorDrift = trig.dataFloat2;
                        t.GlitchAnimationType = trig.dataString1;
                        t.HorizontalShake = trig.dataFloat3;
                        t.ScanLineJitter = trig.dataFloat4;
                        t.VerticalJump = trig.dataFloat5;
                    }*/
                    //t.FindObject();
            }
            obj.components.Add(t.GetData());
            return obj;
        }

        public static LeanTweenType GetTweenType(string str)
        {
            return (LeanTweenType)Enum.Parse(typeof(LeanTweenType), str);
        }
        public static TriggerCollection.TrigType GetTrigType(string str)
        {
            return (TriggerCollection.TrigType)Enum.Parse(typeof(TriggerCollection.TrigType), str);
        }
    }
    [Serializable]
    public class GameProperties
    {
        public float Speed;
        public int authorid;
        public string levelname;
        public string authorname;
        public string description;
        public int id;
        public string EditedTime;
        public string GameVersion;
        public string Skybox;
        public Color playerCol;
        public Color roadCol;
        public Vector3 lightRotate;
        public Color lightColor;
        public int LevelVersion;
        public string diff;
        public string theme;
        public string genre;
        public string musicName;
        public string musicAuthor;
        public string socialLink;
    }
    [Serializable]
    public class Resources
    {
        public string objName;
        public List<int> GroupID = new List<int>();
        public string src;
        public Vector3 pos;
        public Vector3 scale;
        public Vector3 rotate;
        public Color color;
        public bool isObstacle;
    }
}
