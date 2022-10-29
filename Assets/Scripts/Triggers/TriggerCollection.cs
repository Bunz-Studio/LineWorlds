using System.Collections.Generic;
using UnityEngine;
using System;
using ExternMaker;
using Kino;

public class TriggerCollection : MonoBehaviour
{
    #region Variables
    public enum TrigType
    {
        Camera,
        Jump,
        Speed,
        Fog,
        CameraBackground,
        StopAWhile,
        ShakeCam,
        Move,
        Rotate,
        Scale,
        // New
        Teleport,
        Activate,
        UTurn,
        Fov,
        Color,
        //AnalogGlitch,
        ClearTails,
        // Old
        Finish
    };
    public TrigType TriggerTypes;

    // Camera
    public Vector3 TargetAngleRotation = new Vector3(45, 45, 0), TargetPivotOffset;
    public float TargetCamDistance = 20;
    public float TargetSmoothing = 1, TargetRotationSmoothing = 1;
    public float TargetFactor = 1;
    public bool ChangeTargetObject;
    public GameObject TargetObjectToSee;

    // Jump
    public float HighJump = 500;

    // Speed
    public float TargetSpeed = 10;

    // Fog
    public float DensityTo = 0.01f;
    public Color ColorTo;
    public float FogTime = 5;
    //bool lerp;

    // Camera Background
    public Color camBG_Color = Color.white;
    public float camBG_Time = 1;

    // StopAWhile
    public float Duration = 1.0f;

    // Shake
    public float ShakeDuration = 0.3f;
    public float ShakeStrength = 1;
    
    // Move Rotate Scale
    public Vector3 MRSTargetVector = Vector3.one;
    public LeanTweenType MRSEaseAnimation = LeanTweenType.linear;
    public GameObject MRSTargetObject;
    public bool MRSisUseGroup = false;
    public List<int> MRSTargetGroup = new List<int>();
    public float MRSTargetTime = 1;

    // Teleport
    public GameObject TLPTargetObject;

    // Activate
    public List<int> ActivateTargetGroup = new List<int>();

    // UTurn
    public float targetBlock1 = 0;
    public float targetBlock2 = 90;

    // FOV
    public float targetFOV = 60;
    public float FOVTime = 1;
    public LeanTweenType FOVAnimation = LeanTweenType.linear;

    // Color
    public float ColorTime;
    public GameObject ColorTarget;
    public bool ColorUseGroup = false;
    public List<int> ColorGroup = new List<int>();
    public LeanTweenType ColorAnimation = LeanTweenType.linear;

    public List<GameObject> TargetGameObjects;

    // AnalogGlitch
    /*public LeanTweenType GlitchAnimationType = LeanTweenType.linear;
    public float ScanLineJitter = 0f;
    public float VerticalJump = 0f;
    public float HorizontalShake = 0f;
    public float ColorDrift = 0f;
    public float GlitchTime = 0f;*/

    // MeshColor
    /*public MeshRenderer meshColor_renderer;
    public bool meshColor_isChangingMaterial;
    public Material meshColor_material;
    public Color meshColor_Color = Color.white;
    public bool meshColor_changeEmmision;
    public float meshColor_Speed = 5;*/

    // ExecuteAfter
    public bool executeTrigger;
    public TriggerCollection targetTrigger;

    // Temporary Values
    public int checkpointGot;
    CheckpointManager gameMgr;
    LineMovement hittingLine;
    #endregion

    #region Functions
    /*public void AnalogGlitch()
    {
        BetterCamera bc = FindObjectOfType<BetterCamera>();
        if (!bc) return;
        AnalogGlitch ag = bc.transform.GetComponentInChildren<AnalogGlitch>();
        LeanTween.value(ag.scanLineJitter, ScanLineJitter, GlitchTime).setEase(GlitchAnimationType).setOnUpdate((float val) =>
        {
            ag.scanLineJitter = val;
        });
        LeanTween.value(ag.verticalJump, VerticalJump, GlitchTime).setEase(GlitchAnimationType).setOnUpdate((float val) =>
        {
            ag.verticalJump = val;
        });
        LeanTween.value(ag.horizontalShake, HorizontalShake, GlitchTime).setEase(GlitchAnimationType).setOnUpdate((float val) =>
        {
            ag.horizontalShake = val;
        });
        LeanTween.value(ag.colorDrift, ColorDrift, GlitchTime).setEase(GlitchAnimationType).setOnUpdate((float val) =>
        {
            ag.colorDrift = val;
        });

    }*/
    
    public List<SpriteRenderer> ColorSpriteTargets = new List<SpriteRenderer>();
    public List<Renderer> ColorRendererTargets = new List<Renderer>();

    public void ChangeColor()
    {
        foreach(var s in ColorSpriteTargets)
        {
            ChangeColorToSprite(s);
        }
        foreach(var r in ColorRendererTargets)
        {
            ChangeColorToRenderer(r);
        }
    }
    
    public void ChangeColorToSprite(SpriteRenderer obj)
    {
        var SR = obj;
        float R = SR.color.r;
        float G = SR.color.g;
        float B = SR.color.b;
        float A = SR.color.a;

        LeanTween.value(R, ColorTo.r, ColorTime).setEase(ColorAnimation).setOnUpdate((float e) =>
        {
            SR.color = new Color(e, SR.color.g, SR.color.b, SR.color.a);
        });
        LeanTween.value(G, ColorTo.g, ColorTime).setEase(ColorAnimation).setOnUpdate((float e) =>
        {
            SR.color = new Color(SR.color.r, e, SR.color.b, SR.color.a);
        });
        LeanTween.value(B, ColorTo.b, ColorTime).setEase(ColorAnimation).setOnUpdate((float e) =>
        {
            SR.color = new Color(SR.color.r, SR.color.g, e, SR.color.a);
        });
        LeanTween.value(A, ColorTo.a, ColorTime).setEase(ColorAnimation).setOnUpdate((float e) =>
        {
            SR.color = new Color(SR.color.r, SR.color.g, SR.color.b, e);
        });
    }

    public void ChangeColorToRenderer(Renderer obj)
    {
        var meshColor = obj.sharedMaterial.color;
        float R = meshColor.r;
        float G = meshColor.g;
        float B = meshColor.b;

        LeanTween.value(R, ColorTo.r, ColorTime).setEase(ColorAnimation).setOnUpdate((float e) =>
        {
            var currentColor = obj.sharedMaterial.color;
            float cR = currentColor.r;
            float cG = currentColor.g;
            float cB = currentColor.b;
            obj.sharedMaterial.color = new Color(e, cG, cB);
        });
        LeanTween.value(G, ColorTo.g, ColorTime).setEase(ColorAnimation).setOnUpdate((float e) =>
        {
            var currentColor = obj.sharedMaterial.color;
            float cR = currentColor.r;
            float cG = currentColor.g;
            float cB = currentColor.b;
            obj.sharedMaterial.color = new Color(cR, e, cB);
        });
        LeanTween.value(B, ColorTo.b, ColorTime).setEase(ColorAnimation).setOnUpdate((float e) =>
        {
            var currentColor = obj.sharedMaterial.color;
            float cR = currentColor.r;
            float cG = currentColor.g;
            float cB = currentColor.b;
            obj.sharedMaterial.color = new Color(cR, cG, e);
        });
    }

    public void FindObject()
    {
        TargetGameObjects = new List<GameObject>();
        switch (TriggerTypes)
        {
            case TrigType.Scale:
            case TrigType.Rotate:
            case TrigType.Move:
                if (MRSisUseGroup)
                {
                    //Move all objects in group
                    foreach (ExtObject obj in FindObjectsOfType<ExtObject>())
                    {
                        foreach (int g in obj.groupID)
                        {
                            if (MRSTargetGroup.FindIndex(val => val.ToString() == g.ToString()) > -1)
                            {
                                var target = obj.gameObject;
                                TargetGameObjects.Add(target);
                            }
                        }
                    }
                }
                else
                {
                    //Move object with name
                    GameObject target = MRSTargetObject;
                    TargetGameObjects.Add(target);

                }
                break;
            case TrigType.Color:
                if (ColorUseGroup)
                {
                    foreach (ExtObject og in FindObjectsOfType<ExtObject>())
                    {
                        foreach (int group in og.groupID)
                        {
                            foreach (int e in ColorGroup)
                            {
                                if (group.ToString() == e.ToString())
                                {
                                    var sprite = og.GetComponent<SpriteRenderer>();
                                    if (sprite != null) ColorSpriteTargets.Add(sprite);
                                    else
                                    {
                                        var renderer = og.GetComponent<Renderer>();
                                        ColorRendererTargets.Add(renderer);
                                    }
                                    //TargetGameObjects.Add(og.gameObject);
                                }
                            }
                        }
                    }
                }
                else
                {
                    var obj = ColorTarget;
                    if (obj != null)
                    {
                        var sprite = obj.GetComponent<SpriteRenderer>();
                        if (sprite != null) ColorSpriteTargets.Add(sprite);
                        else
                        {
                            var renderer = obj.GetComponent<Renderer>();
                            ColorRendererTargets.Add(renderer);
                        }
                    }
                }
                break;
            case TrigType.Activate:
                foreach (TriggerCollection t in FindObjectsOfType<TriggerCollection>())
                {
                    foreach (int g in t.GetComponent<ExtObject>().groupID)
                    {
                        foreach (int e in ActivateTargetGroup)
                        {
                            if (g.ToString() == e.ToString())
                            {
                                TargetTriggers.Add(t);
                            }
                        }
                    }
                }
                break;
        }
    }
    public void SetupColor()
    {
        var color = new Color32();
        switch (TriggerTypes)
        {
            case TrigType.Camera:
                color = new Color32(0, 186, 50, 255);
                break;
            case TrigType.ShakeCam:
                color = new Color32(0, 255, 255, 255);
                break;
            case TrigType.Jump:
                color = new Color32(255, 140, 0, 255);
                break;
            case TrigType.CameraBackground:
                color = new Color32(90, 255, 200, 255);
                break;
            case TrigType.StopAWhile:
                color = new Color32(251, 255, 0, 255);
                break;
            case TrigType.Move:
                color = new Color32(0, 166, 255, 255);
                break;
            case TrigType.Rotate:
                color = new Color32(61, 68, 255, 255);
                break;
            case TrigType.Scale:
                color = new Color32(255, 61, 236, 255);
                break;
            case TrigType.Fog:
                color = new Color32(200, 200, 50, 255);
                break;
            case TrigType.Finish:
                color = new Color32(3, 69, 252, 255);
                break;
            case TrigType.Teleport:
                color = new Color32(255, 61, 116, 255);
                break;
            case TrigType.Activate:
                color = new Color32(189, 222, 160, 255);
                break;
            case TrigType.UTurn:
                color = new Color32(160, 222, 204, 255);
                break;
            case TrigType.Fov:
                color = new Color32(100, 200, 100, 255);
                break;
            case TrigType.Color:
                color = new Color32(255, 54, 178, 255);
                break;
            default:
                color = new Color32(0, 20, 20, 255);
                break;
            /*case TrigType.AnalogGlitch:
                color = new Color32(53, 7, 66, 255);
                break;*/
        }
        gameObject.GetComponent<Renderer>().material.color = color;
    }

    public void ClearTails()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Tail"))
        {
            Destroy(obj);
        }
        FindObjectOfType<LineMovement>().CreateTail();
    }

    public void Finish()
    {
        BetterCamera.current.StayInPosition();
        FindObjectOfType<LineMovement>().Finish();
    }

    public void UTurn()
    {
        hittingLine.turns[0] = new Vector3(0, targetBlock1, 0);
        hittingLine.turns[1] = new Vector3(0, targetBlock2, 0);
    }

    public List<TriggerCollection> TargetTriggers = new List<TriggerCollection>();
    public void Activate(Collider other)
    {
        if (TargetGameObjects.Count == 0)
            FindObject();

        foreach (TriggerCollection t in TargetTriggers)
        {
            t.OnTriggerEnter(other);
        }
    }

    public void FOV()
    {
        LeanTweenType targetEase = FOVAnimation;
        LeanTween.value(BetterCamera.current.mainCamera.fieldOfView, targetFOV, FOVTime).setEase(targetEase).setOnUpdate((float e) =>
        {
            BetterCamera.current.mainCamera.fieldOfView = e;
        });
    }

    public void Teleport()
    {
        hittingLine.CreateTail();
        hittingLine.gameObject.transform.position = TLPTargetObject.transform.position;
    }

    public void Scale()
    {
        //Get ease animation
        LeanTweenType targetEase = MRSEaseAnimation;

        foreach (GameObject obj in TargetGameObjects)
        {
            obj.LeanScale(MRSTargetVector, MRSTargetTime).setEase(targetEase);
        }
    }

    public void Rotate()
    {
        //Get ease animation
        LeanTweenType targetEase = MRSEaseAnimation;
        foreach (GameObject obj in TargetGameObjects)
        {
            obj.LeanRotate(MRSTargetVector, MRSTargetTime).setEase(targetEase);
        }
    }

    public void Fog()
    {
        LeanTween.value(RenderSettings.fogDensity, DensityTo, FogTime).setOnUpdate((float e) =>
        {
            RenderSettings.fogDensity = e;
        });

        LeanTween.value(gameObject, RenderSettings.fogColor, ColorTo, FogTime).setOnUpdate((Color e) =>
        {
            RenderSettings.fogColor = e;
        });
    }

    public void CameraBackground()
    {
        LeanTween.value(gameObject, BetterCamera.current.mainCamera.backgroundColor, camBG_Color, camBG_Time).setOnUpdate((Color e) =>
        {
            BetterCamera.current.mainCamera.backgroundColor = e;
        });
    }

    public void Move()
    {
        //Get ease animation
        LeanTweenType targetEase = MRSEaseAnimation;
        foreach (GameObject obj in TargetGameObjects)
        {
            if(obj != null) obj.LeanMove(obj.transform.position + MRSTargetVector, MRSTargetTime).setEase(targetEase);
        }

    }
    void EndedLerping()
    {
        //lerp = false;
    }

    void ReturnTail()
    {
        hittingLine.CreateTail();
    }

    public void ResetTrigger(int checkSort)
    {
        // Implementation is no longer used
    }
    void RevokeStopping()
    {
        hittingLine.isStarted = true;
        hittingLine.isControllable = true;
    }
    #endregion

    #region Unity Functions
    void Start()
    {
        hittingLine = GameObject.FindObjectOfType<LineMovement>();
        gameMgr = FindObjectOfType<CheckpointManager>();
        gameMgr.OnUndo += ResetTrigger;
        FindObject();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (ExtCore.playState != EditorPlayState.Playing) return;

            //checkpointGot = gameMgr.checkpointResult;
            hittingLine = other.GetComponent<LineMovement>();
            switch (TriggerTypes)
            {
                case TrigType.Camera:
                    BetterCamera cam = BetterCamera.current;
                    cam.ChangeVar(TargetAngleRotation, TargetSmoothing, TargetRotationSmoothing, TargetPivotOffset, TargetCamDistance);
                    cam.SmoothFactor = TargetFactor;
                    if (ChangeTargetObject)
                    {
                        cam.Line = TargetObjectToSee.transform;
                    }
                    break;
                case TrigType.Jump:
                    //Invoke("ReturnTail", HighJump / 1000);
                    var rigid = other.GetComponent<Rigidbody>();
                    rigid.AddForce(Vector3.up * HighJump * rigid.mass);
                    break;
                case TrigType.Speed:
                    hittingLine.lineSpeed = TargetSpeed;
                    break;
                case TrigType.Fog:
                    Fog();
                    break;
                case TrigType.CameraBackground:
                    CameraBackground();
                    break;
                case TrigType.StopAWhile:
                    Invoke("RevokeStopping", Duration);
                    hittingLine.isStarted = false;
                    hittingLine.isControllable = false;
                    break;
                case TrigType.ShakeCam:
                    BetterCamera.current.Shake(ShakeDuration, ShakeStrength);
                    break;
                case TrigType.Move:
                    Move();
                    break;
                case TrigType.Rotate:
                    Rotate();
                    break;
                case TrigType.Scale:
                    Scale();
                    break;
                case TrigType.Finish:
                    Finish();
                    break;
                case TrigType.Color:
                    ChangeColor();
                    break;
                case TrigType.Teleport:
                    Teleport();
                    break;
                case TrigType.Activate:
                    Activate(other);
                    break;
                case TrigType.UTurn:
                    UTurn();
                    break;
                case TrigType.Fov:
                    FOV();
                    break;
                /*case TrigType.AnalogGlitch:
                    //AnalogGlitch();
                    break;*/
                case TrigType.ClearTails:
                    ClearTails();
                    break;
            }
            if (executeTrigger)
            {
                if(targetTrigger != null)
                {
                    targetTrigger.OnTriggerEnter(other);
                }
            }
        }
        /*if (TriggerTypes == TrigType.Teleport ||
            TriggerTypes == TrigType.UTurn ||
            TriggerTypes == TrigType.AnalogGlitch)
        {
            return;
        }*/
        //gameMgr.TouchedTrig.Add(this);
    }
    #endregion
}