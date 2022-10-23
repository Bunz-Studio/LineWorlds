using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class CameraEditorMovement : MonoBehaviour
{
    public Vector2 lookat;
    Vector2 prevMouse;
    private Vector3 mouseOrigin;
    public bool isRotating;
    
    public Joystick joyStick;
    public float joyStickSpeed = 100f;
    
    class CameraState
    {
        public float yaw;
        public float pitch;
        public float roll;
        public float x;
        public float y;
        public float z;
        
        public void SetFromTransform(Transform t)
        {
            pitch = t.eulerAngles.x;
            yaw = t.eulerAngles.y;
            roll = t.eulerAngles.z;
            x = t.position.x;
            y = t.position.y;
            z = t.position.z;
        }

        public void Translate(Vector3 translation)
        {
            Vector3 rotatedTranslation = Quaternion.Euler(pitch, yaw, roll) * translation;

            x += rotatedTranslation.x;
            y += rotatedTranslation.y;
            z += rotatedTranslation.z;
        }

        public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
        {
            yaw = Mathf.Lerp(yaw, target.yaw, rotationLerpPct);
            pitch = Mathf.Lerp(pitch, target.pitch, rotationLerpPct);
            roll = Mathf.Lerp(roll, target.roll, rotationLerpPct);
            
            x = Mathf.Lerp(x, target.x, positionLerpPct);
            y = Mathf.Lerp(y, target.y, positionLerpPct);
            z = Mathf.Lerp(z, target.z, positionLerpPct);
        }

        public void UpdateTransform(Transform t)
        {
            t.eulerAngles = new Vector3(pitch, yaw, roll);
            t.position = new Vector3(x, y, z);
            OnCameraMove.Invoke(CameraEditorMovement.cam);
        }
    }

    public static System.Action<Camera> OnCameraMove;
    public static Camera cam;
    
    CameraState m_TargetCameraState = new CameraState();
    CameraState m_InterpolatingCameraState = new CameraState();
    
    public Texture2D customCursor;
    public Slider CamBoost;
    public Slider CamPosLerp;
    [Header("Movement Settings")]
    [Tooltip("Exponential boost factor on translation, controllable by mouse wheel.")]
    public float boost = 3.5f;

    [Tooltip("Time it takes to interpolate camera position 99% of the way to the target."), Range(0.001f, 1f)]
    public float positionLerpTime = 0.2f;

    [Header("Rotation Settings")]
    [Tooltip("X = Change in mouse position.\nY = Multiplicative factor for camera rotation.")]
    public AnimationCurve mouseSensitivityCurve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));

    [Tooltip("Time it takes to interpolate camera rotation 99% of the way to the target."), Range(0.001f, 1f)]
    public float rotationLerpTime = 0.01f;

    [Tooltip("Whether or not to invert our Y axis for mouse input to rotation.")]
    public bool invertY = false;

    void OnEnable()
    {
        cam = GetComponent<Camera>();
        m_TargetCameraState.SetFromTransform(transform);
        m_InterpolatingCameraState.SetFromTransform(transform);
    }

    Vector3 GetInputTranslationDirection()
    {
        Vector3 direction = new Vector3();
        if (Input.GetKey(KeyCode.W))
        {
            direction += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            direction += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A))
        {
            direction += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            direction += Vector3.right;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            direction += Vector3.down;
        }
        if (Input.GetKey(KeyCode.E))
        {
            direction += Vector3.up;
        }
        return direction;
    }
    public Vector3 camRo;
    public void getTouchMove()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Get mouse origin
            mouseOrigin = Input.mousePosition;
            isRotating = true;
        }
        if (!Input.GetMouseButton(0)) isRotating = false;
        if (isRotating)
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);

            //transform.RotateAround(transform.position, transform.right, -pos.y * turnSpeed); //around x
            transform.Rotate(Vector3.up, pos.x * 50); // around y
            transform.Rotate(Vector3.left, pos.y * 50);
            transform.eulerAngles = transform.eulerAngles + camRo;// around x
            
            camRo = transform.eulerAngles;
        }
        else
        {
            transform.eulerAngles = camRo;
        }
    }
    void Update()
    {
        //getTouchMove();
        m_TargetCameraState.Translate(new Vector3(joyStick.Horizontal, 0, joyStick.Vertical) * joyStickSpeed * Time.deltaTime);
        if(CamBoost != null && CamPosLerp != null)
        {
            boost = CamBoost.value;
            positionLerpTime = CamPosLerp.value;
        }
        
        
        #if UNITY_ANDROID
        HandleAndroidInput();
        #else
        HandlePCInput();
        #endif
        
        m_TargetCameraState.UpdateTransform(transform);
    }
    
    public void HandlePCInput()
    {
        if (isRotating)
        {
        	float x = Input.GetAxis("Mouse X");
        	float y = Input.GetAxis("Mouse Y");
        	if(Input.touches.Length > 0)
        	{
        		var pos = Input.touches[0].deltaPosition;
        		x = pos.x;
        		y = pos.y;
        	}
        	
            var mouseMovement = new Vector2(x, y * (invertY ? 1 : -1));
            
            var mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);

            m_TargetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
            m_TargetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;
            
            var translation = GetInputTranslationDirection() * Time.deltaTime;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                translation *= 10.0f;
            }
	        translation *= Mathf.Pow(2.0f, boost);
	
	        m_TargetCameraState.Translate(translation);
        }
        
        if (Input.GetMouseButtonUp(1))
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            isRotating = false;
        }
        if (ExternMaker.ExtUtility.IsInputHoveringUI())
            return;
        if (Input.GetMouseButtonDown(1))
        {
            if(customCursor != null) Cursor.SetCursor(customCursor, Vector2.zero, CursorMode.Auto);
        	isRotating = true;
        }
    }
    
    public void HandleAndroidInput()
    {	
        if (isRotating)
        {
        	float x = Input.GetAxis("Mouse X");
        	float y = Input.GetAxis("Mouse Y");
        	if(Input.touches.Length > 0)
        	{
        		var pos = Input.touches[0].deltaPosition;
        		x = pos.x / 2;
        		y = pos.y / 2;
        	}
        	
            var mouseMovement = new Vector2(x, y * (invertY ? 1 : -1));
            
            var mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);

            m_TargetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
            m_TargetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;
            
            var translation = GetInputTranslationDirection() * Time.deltaTime;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                translation *= 10.0f;
            }
	        translation *= Mathf.Pow(2.0f, boost);
	
	        m_TargetCameraState.Translate(translation);
        }
        
        if (ExternMaker.ExtUtility.IsInputHoveringUI())
            return;
        if (Input.GetMouseButtonDown(0))
        {
        	isRotating = true;
        }
        
        if (Input.GetMouseButtonUp(0))
        {
        	isRotating = false;
        }
    }
}