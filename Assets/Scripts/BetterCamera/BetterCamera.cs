using System.Runtime.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExternMaker;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

[ExecuteInEditMode]
public class BetterCamera : MonoBehaviour
{
	public static BetterCamera b_cam;
	public static BetterCamera current
	{
		get
		{
			b_cam = b_cam ?? FindObjectOfType<BetterCamera>();
			return b_cam;
		}
		set
		{
			b_cam = value;
		}
	}
	[Header("Camera Variables")]
    [AllowSavingState]
    public Transform Line;
	public Camera mainCamera;

    [AllowSavingState]
    public Vector3 pivotOffset = Vector3.zero;

	private float x;
	private float y;
	private float z;

    [AllowSavingState]
    public float targetX = 45f;
    [AllowSavingState]
    public float targetY = 60f;
    [AllowSavingState]
    public float targetZ;

    [AllowSavingState]
    public float TargetDistance = 20f;

    [AllowSavingState]
    public float SmoothTime = 1f;
    [AllowSavingState]
    public float SmoothFactor = 1f;
    [AllowSavingState]
    public float needtime = 1f;

	private float xVelocity = 1f;
	private float yVelocity = 1f;
	private float zVelocity = 1f;
    
	// How long the object should shake for.
    [AllowSavingState]
	public float shakeDuration = 0f;

    // Amplitude of the shake. A larger value shakes the camera harder.
    [AllowSavingState]
    public float shakeAmount = 0.7f;
	private float decreaseFactor = 1.0f;

	[Header("Misc")]
    [AllowSavingState]
    public Vector3 localPositionOffset;
    [AllowSavingState]
    public Vector3 localEulerOffset;
	// Should the camera be simulated in editor mode?
	public bool simulateInEditor = true;

	[System.Serializable]
	public class VariablesSaving{
		public Transform Line;
		public Vector3 pivotOffset = Vector3.zero;
		public float targetX = 45f;
		public float targetY = 60f;
		public float targetZ;
		public float TargetDistance = 20f;
		public float SmoothTime = 1f;
		public float SmoothFactor = 1f;
		public float needtime = 1f;
		public float shakeDuration = 0f;
		public float shakeAmount = 0.7f;
		public Vector3 position;
		public Quaternion rotation;
		public bool simulateInEditor;
	}
	private VariablesSaving currentVariables;
	private Vector3 cachePosition;
    void Start()
    {
    	current = this;
        x = targetX;
		y = targetY;
		z = targetZ;
		transform.position = Line.position + pivotOffset;
		mainCamera.transform.localPosition = new Vector3(0, 0, -TargetDistance);
		transform.eulerAngles = new Vector3(x, y, z);
		cachePosition = mainCamera.transform.localPosition;
    }

    void Update()
    {
        if (Application.isPlaying)
        {
            float speedTime = Time.deltaTime * SmoothTime;
            Vector3 targetDist = new Vector3(0, 0, -TargetDistance);
            x = Mathf.SmoothDampAngle(x, targetX, ref xVelocity, needtime);
            y = Mathf.SmoothDampAngle(y, targetY, ref yVelocity, needtime);
            z = Mathf.SmoothDampAngle(z, targetZ, ref zVelocity, needtime);
            transform.eulerAngles = new Vector3(x, y, z);
            if (Line != null)
                transform.position = Vector3.Slerp(transform.position, Line.position + pivotOffset, SmoothFactor * Time.deltaTime);
            else
                transform.position = Vector3.Slerp(transform.position, transform.position + pivotOffset, SmoothFactor * Time.deltaTime);
            var lerp = Vector3.Lerp(cachePosition, targetDist, speedTime);
            var actual = lerp + localPositionOffset;
            cachePosition = lerp;
            if (shakeDuration > 0)
            {
                mainCamera.transform.localPosition = actual + Random.onUnitSphere * shakeAmount;
                shakeDuration -= decreaseFactor * Time.deltaTime;
            }
            else
            {
                mainCamera.transform.localPosition = actual;
                shakeDuration = 0f;
            }
            mainCamera.transform.localEulerAngles = localEulerOffset;
        }
        else
        {
            if (simulateInEditor)
                UpdateCameraPosition();
        }
    }

    public void UpdateCameraPosition()
    {
        var targetDist = new Vector3(0, 0, -TargetDistance);
        transform.eulerAngles = new Vector3(targetX, targetY, targetZ);
        mainCamera.transform.localPosition = targetDist + localPositionOffset;
        mainCamera.transform.localEulerAngles = localEulerOffset;
        if (Line != null)
        {
            transform.position = Line.position + pivotOffset;
        }
        else
        {
            transform.position = transform.position + pivotOffset;
        }
    }

    public void ChangeVar(Vector3 targetRot, float targetSmooth, float targetRotSpeed, Vector3 targetPivotOffset, float targetDistance){
		targetX = targetRot.x;
		targetY = targetRot.y;
		targetZ = targetRot.z;
		SmoothTime = targetSmooth;
		needtime = targetRotSpeed;
		pivotOffset = targetPivotOffset;
		TargetDistance = targetDistance;
	}
    public VariablesSaving GetCurrentVariable(){
    	var varSav = new VariablesSaving();
    	varSav.Line = Line;
    	varSav.needtime = needtime;
    	varSav.pivotOffset = pivotOffset;
    	varSav.shakeAmount = shakeAmount;
    	varSav.shakeDuration = shakeDuration;
    	varSav.SmoothFactor = SmoothFactor;
    	varSav.SmoothTime = SmoothTime;
    	varSav.TargetDistance = TargetDistance;
    	varSav.targetX = targetX;
    	varSav.targetY = targetY;
    	varSav.targetZ = targetZ;
    	varSav.position = transform.position;
    	varSav.rotation = transform.rotation;
    	varSav.simulateInEditor = simulateInEditor;
    	return varSav;
    }
    public void ApplyVariableSaving(VariablesSaving values){
    	Line = values.Line;
    	needtime = values.needtime;
    	pivotOffset = values.pivotOffset;
    	shakeAmount = values.shakeAmount;
    	shakeDuration = values.shakeDuration;
    	SmoothFactor = values.SmoothFactor;
    	SmoothTime = values.SmoothTime;
    	TargetDistance = values.TargetDistance;
    	targetX = values.targetX;
    	targetY = values.targetY;
    	targetZ = values.targetZ;
    	transform.position = values.position;
    	transform.rotation = values.rotation;
    	simulateInEditor = values.simulateInEditor;
    }
	public void Shake(float Duration, float Strength){
		shakeAmount = Strength;
		shakeDuration = Duration;
	}
	public void SuddenZ(float to){
		z = to;	
	}
    public void SetLocalZ(float to){
    	mainCamera.transform.localEulerAngles = new Vector3(0,0,to);
    }
    public void ResetCamera(int getInt){
    	ApplyVariableSaving(currentVariables);
    	transform.position = Line.position + pivotOffset;
    	SetCurrentAsTarget();
    }
    public void SetCurrentVar(int getInt){
    	currentVariables = GetCurrentVariable();
    }
    public void SetStarterAtNow(){
    	x = transform.eulerAngles.x;
    	y = transform.eulerAngles.y;
    	z = transform.eulerAngles.z;
    }
    
    public void StayInPosition()
    {
    	var currPos = new GameObject();
		currPos.transform.localScale = transform.localScale;
		currPos.transform.position = Line.position;
		currPos.transform.rotation = transform.rotation;
		Line = currPos.transform;
    }
    
    public void ForceCameraWithCurrent()
    {
		var targetDist = new Vector3(0, 0, -TargetDistance);
		transform.eulerAngles = new Vector3(targetX, targetY, targetZ);
		mainCamera.transform.localPosition = targetDist + localPositionOffset;
		mainCamera.transform.localEulerAngles = localEulerOffset;
		x = targetX;
		y = targetY;
		z = targetZ;
		if(Line != null) {
			transform.position = Line.position + pivotOffset;
		}else{
			transform.position = transform.position + pivotOffset;
		}
    }
    public void SetCurrentAsTarget(){
    	x = targetX;
    	y = targetY;
    	z = targetZ;
    }
}
