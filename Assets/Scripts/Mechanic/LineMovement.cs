using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using ExternMaker;
using LineWorldsMod;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(BoxCollider))]
public class LineMovement : MonoBehaviour
{
	#region Variables
	public LevelManager manager;
	
	public Mesh lineMesh;
    [AllowSavingState]
    [ShowInCustomInspector]
    public float lineSpeed = 15;
	public Transform lineParent;

    [AllowSavingState]
    public int turnIndex;
    [AllowSavingState]
    public Vector3[] turns = {
		new Vector3(0, 90, 0),
		Vector3.zero
	};
    [AllowSavingState]
    public Vector3 finishTurn = new Vector3(0, 45, 0);
	
    [AllowSavingState]
	public bool isStarted;
    [AllowSavingState]
    public bool isAlive = true;
    [AllowSavingState]
    public bool isControllable = true;
    [ShowInCustomInspector]
    public bool immortal;
    public bool noTurns;
    [ShowInCustomInspector]
    public bool useIndicators;
	
	public List<string> ignoredTags = new List<string>(new [] {
		"Obstacle",
		"Trigger"
	});
	public KeyCode[] tapKeys = { KeyCode.Mouse0, KeyCode.Space, KeyCode.UpArrow };
	
	GameObject currentTail;
	[HideInInspector] public BoxCollider colai;
    [HideInInspector] public List<GameObject> tails = new List<GameObject>();
	readonly List<MeshRenderer> tailRenderers = new List<MeshRenderer>();
	[HideInInspector] public int taps;
	[HideInInspector] public MeshRenderer rend;
	[HideInInspector] public AudioSource source;
	[HideInInspector] public bool isNotGrounded;
	[HideInInspector] public Rigidbody rigid;
    [HideInInspector] public Vector3 lineStart;
    [HideInInspector] public List<MeshRenderer> lineIndicators = new List<MeshRenderer>();
    [HideInInspector] public int lineIndicatorIndex;
	[HideInInspector] public bool keepGoing;
	
	public System.Action OnLineStart;
    public System.Action OnLineTurns;
    public System.Action OnLineKilled;
	public System.Action OnLineFinished;
	public System.Action OnLineRespawned;
	
	// Static Accessor
	public LevelManager s_manager
	{
		get
		{
			return manager ?? FindObjectOfType<LevelManager>();
		}
	}

    bool isInitialized;
	#endregion
	
	#region Unity Functions
	public void Start ()
	{
        if (isInitialized) return;

		source = GetComponent<AudioSource>();
		rend = GetComponent<MeshRenderer>();
		colai = GetComponent<BoxCollider>();
		rigid = GetComponent<Rigidbody>();
		rend.sharedMaterial = rend.material;
		source.volume = 1;
        InitializeIndicators();
        isInitialized = true;
	}
	void Update ()
	{
        gameObject.tag = "Player";
		if(isAlive){
			var c = CheckGround();
			if(isControllable){
				if(IsInputPressed()) {
					if (isStarted) {
						if (c && !noTurns) {
							taps++;
							InputLine();
						}
					} else {
						taps++;
						StartLine();
					}
				}
			}
			if(isStarted) {
				transform.position += transform.forward * Time.deltaTime * lineSpeed;
				if(c) {
					if(isNotGrounded)
					{
						CreateTail();
                        UndoPushTail();
						isNotGrounded = false;
					}
					PushTail();
				}
				else
				{
					isNotGrounded = true;
				}
			}
		}
		if(keepGoing)
		{
			transform.position += transform.forward * Time.deltaTime * lineSpeed;
		}
		
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			if(ExtCore.instance == null)
			{
				SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
			}
			else
			{
            	ExtCore.instance.StopGame();
                panelInstance = LoadingPanelManager.CreatePanel("Restarting level...");
                Invoke("DelayedPlay", 0.2f);
			}
		}
	}
    LoadingPanelInstance panelInstance;
    public void DelayedPlay()
    {
        ExtCore.instance.PlayGame();
        Destroy(panelInstance.gameObject);
    }
	#endregion
	
	#region Separated Update
	public void StartLine()
	{
		isStarted = true;
		if(source != null) source.Play();
		CreateTail();
		OnLineStart.TryInvoke();
	}
	
	public void InputLine()
    {
        lineStart = new Vector3(lineStart.x, transform.position.y, lineStart.z);
        //AdjustLine(transform.position);
        OnLineTurns.TryInvoke();
        TurnLine();
        CreateTail();
        MoveIndicator();
	}

	#endregion
	
	#region Functions
	public bool IsInputPressed()
	{
        foreach (var key in tapKeys)
        {
            bool isValid = true;
            if (key == KeyCode.Mouse0 || key == KeyCode.Mouse1) isValid = !EventSystem.current.IsPointerOverGameObject();
            if (Input.GetKeyDown(key) && isValid) return true;
        }
		return false;
	}

	public void TurnLine ()
	{
		turnIndex = (turnIndex + 1).RevertClamp(0, turns.Length - 1);
		transform.eulerAngles = turns[turnIndex];
	}
	
	public void PushTail ()
	{
        ResizeTail(Time.deltaTime);
    }
    public void UndoPushTail()
    {
        ResizeTail(-Time.deltaTime);
    }

    public void ResizeTail(float val)
    {
        if (currentTail != null)
        {
            var t = currentTail.transform;
            t.position = new Vector3(t.position.x, transform.position.y, t.position.z) + t.forward * lineSpeed / 2 * val;
            t.localScale += new Vector3(0, 0, 1) * lineSpeed * val;
        }
    }

    public void AdjustLine(Vector3 to)
    {
        transform.position = to;
        if (currentTail != null)
        {
            var t = currentTail.transform;
            t.eulerAngles = Vector3.zero;
            t.localScale = Vector3.one;
            var result = GetLine(lineStart, to, t.localScale);
            t.position = result[0];
            t.localScale = result[1];
            CreateTailInstance();
        }
    }

    public static Vector3[] GetLine(Vector3 from, Vector3 to, Vector3 minimumScale)
    {
        var position = Vector3.zero;
        var scale = Vector3.one;
        var centeredPosition = CenterOfVectors(new Vector3[] { from, to });
        var scaledLine = from - to;
        var fixedScaledLine = new Vector3(II(Mathf.Abs(scaledLine.x), minimumScale.x), II(Mathf.Abs(scaledLine.y), minimumScale.y), II(Mathf.Abs(scaledLine.z), minimumScale.z));
        position = centeredPosition;
        scale = fixedScaledLine;
        return new Vector3[] { position, scale };
    }

    public static float II(float val, float limit)
    {
        if (val < limit)
        {
            val = limit;
        }
        return val;
    }
    public static Vector3 CenterOfVectors(Vector3[] vectors)
    {
        Vector3 sum = Vector3.zero;
        if (vectors == null || vectors.Length == 0)
        {
            return sum;
        }

        foreach (Vector3 vec in vectors)
        {
            sum += vec;
        }
        return sum / vectors.Length;
    }
    public GameObject CreateTailInstance ()
	{
		var g_obj = new GameObject();
		g_obj.name = "Tail";
		g_obj.layer = gameObject.layer;
		var t_rend = g_obj.AddComponent<MeshRenderer>();
		t_rend.sharedMaterial = rend.sharedMaterial;
		g_obj.AddComponent<MeshFilter>().mesh = lineMesh;
		g_obj.transform.position = transform.position;
        lineStart = transform.position;
		g_obj.transform.eulerAngles = transform.eulerAngles;
		g_obj.transform.SetParent(lineParent);
		tailRenderers.Add(t_rend);
		tails.Add(g_obj);
		return g_obj;
	}

	public GameObject CreateTail ()
	{
		var g_obj = CreateTailInstance();
		currentTail = g_obj;
		return g_obj;
	}
	
	public void DestroyAllTail()
	{
		foreach(var t in tails)
		{
			Destroy(t);
		}
		tails.Clear();
	}
	public void ChangeColor(Color to)
	{
		rend.sharedMaterial.color = to;
	}
	public bool CheckGround()
	{
		RaycastHit hit = default(RaycastHit);
		var h = Physics.Raycast(transform.position, -transform.up, out hit, (colai.size.y / 2) + 0.03f);
		return h ? !ignoredTags.Contains(hit.collider.tag) : h;
	}

	public GameObject CreateCube ()
	{
		var g_obj = new GameObject();
		g_obj.name = "BreakShard";
		var t_rend = g_obj.AddComponent<MeshRenderer>();
		t_rend.sharedMaterial = rend.sharedMaterial;
		g_obj.AddComponent<MeshFilter>().mesh = lineMesh;
		g_obj.transform.position = transform.position;
		g_obj.transform.eulerAngles = transform.eulerAngles;
		g_obj.transform.SetParent(lineParent);
		var b = g_obj.AddComponent<BoxCollider>();
		var r = g_obj.AddComponent<Rigidbody>();
		r.velocity = Random.onUnitSphere * Random.Range(10, 20);
		tailRenderers.Add(t_rend);
		tails.Add(g_obj);
		Destroy(g_obj, 5);
		return g_obj;
	}

	public void Kill()
	{
		if(isAlive)
		{
			for(int i = 0; i < 3; i++)
			{
				CreateCube();
			}
			isAlive = false;
			LeanTween.value(this.gameObject, 1, 0, 0.5f).setOnUpdate(val => source.volume = val);
			rigid.isKinematic = true;
			rigid.useGravity = false;
			rigid.velocity = Vector3.zero;
			OnLineKilled.TryInvoke();
		}
	}
	public void KillFloating()
	{
		if(isAlive)
		{
			isAlive = false;
			keepGoing = true;
			LeanTween.value(this.gameObject, 1, 0, 0.5f).setOnUpdate(val => source.volume = val);
			OnLineKilled.TryInvoke();
		}
	}

	public void Finish()
	{
		transform.eulerAngles = finishTurn;
		CreateTail();
		isControllable = false;
		OnLineFinished.TryInvoke();
	}

    public bool IsTapAccepted()
    {
        return CheckGround();
    }

    public void InitializeIndicators()
    {
        if (useIndicators)
        {
            for (int i = 0; i < 20; i++)
            {
                var inst = new GameObject();
                var filter = inst.AddComponent<MeshFilter>();
                filter.sharedMesh = lineMesh;
                var renderer = inst.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = new Material(rend.sharedMaterial);
                var c = renderer.sharedMaterial.color;
                renderer.sharedMaterial.color = new Color(c.r, c.g, c.b, 0);
                inst.transform.localScale = new Vector3(100, 0.1f, 0.1f);
                inst.transform.localEulerAngles = new Vector3(0, 45, 0);
                inst.transform.localPosition = transform.position - (transform.up * 0.45f);
                lineIndicators.Add(renderer);
            }
        }
    }

    public void MoveIndicator()
    {
        if (useIndicators)
        {
            if (lineIndicatorIndex < lineIndicators.Count)
            {
                var l = lineIndicators[lineIndicatorIndex];
                LeanTween.cancel(l.gameObject);
                var c = l.sharedMaterial.color;
                var s = new Color(c.r, c.g, c.b, 1);
                var e = new Color(c.r, c.g, c.b, 0);
                LeanTween.value(l.gameObject, s, e, 0.3f).setEaseOutCubic().setOnUpdateColor(val =>
                {
                    l.sharedMaterial.color = val;
                });
                l.transform.localPosition = transform.position - (transform.up * 0.45f);
                lineIndicatorIndex++;
            }
            else
            {
                lineIndicatorIndex = 0;
                MoveIndicator();
            }
        }
    }
	#endregion
	
	#region Collisions
	Collider coa;
	void OnTriggerEnter(Collider col)
	{
		if(col != coa){
			coa = col;
			if(col.name.StartsWith("Turn", System.StringComparison.CurrentCulture)) {
				TurnLine();
				CreateTail();
			}
		}
		if(col.tag == "Finish")
		{
			Finish();
		}
		if(col.tag == "Obstacle" && !immortal)
		{
			KillFloating();
		}
	}
	void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject.tag == "Obstacle" && !immortal)
		{
			Kill();
		}
	}
	#endregion
}
