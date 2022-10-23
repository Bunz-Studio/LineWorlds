using UnityEngine;

public class AppearAtDistance : MonoBehaviour
{
	public Vector3 startPos;
	public Vector3 startEuler;
	public Vector3 startScale;
	public Vector3 startOffset = new Vector3(0, -50, 0);
	
	public LeanTweenType easeType;
	
	public float minimalDistance = 15;
	public float speed = 0.2f;
	
	public string tagTo = "Way";
	
	public bool withZScale;
	public bool copyTransform = true;
	public bool disableMesh = true;
	
	public GameObject instance;
	LineMovement mov;
	bool hasAnimate;
	float zStuff;
	void Start()
	{
		var m = GetComponent<MeshRenderer>();
		if (copyTransform) {
			instance = Instantiate(gameObject, transform.parent);
			Destroy(instance.GetComponent<BoxCollider>());
			Destroy(instance.GetComponent<AppearAtDistance>());
			var l = GetComponent<Light>();
			if (l != null) {
				Destroy(l);
			}
			var rend = instance.GetComponent<MeshRenderer>();
			rend.enabled = true;
			rend.material = m.material;
			instance.transform.position = transform.position + startOffset;
			instance.transform.eulerAngles = transform.eulerAngles;
			instance.transform.localScale = transform.localScale;
			if (transform.parent != null) {
				instance.transform.SetParent(transform.parent);
			}
			instance.tag = gameObject.tag;
		} else {
			instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
			instance.transform.position = startPos + startOffset;
			instance.transform.eulerAngles = startEuler;
			instance.transform.localScale = startScale;
			if (transform.parent != null) {
				instance.transform.SetParent(transform.parent);
			}
			instance.GetComponent<MeshRenderer>().material = m.material;
			if (tagTo != "") {
				instance.tag = tagTo;
			}
		}
		if (disableMesh) {
			m.enabled = false;
			MeshRenderer[] meshes = transform.GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer ya in meshes) {
				ya.enabled = false;
			}
		}
		instance.AddKeeper();
		mov = FindObjectOfType<LineMovement>();
	}
    
	void Update()
	{
		float dist = Vector3.Distance(mov.gameObject.transform.position, transform.position);
		float minimumDistance = minimalDistance;
		if (withZScale) {
			minimumDistance = minimalDistance * transform.localScale.z / 0.5f;
		}
		if (dist < minimumDistance) {
			if (!hasAnimate) {
				instance.LeanMoveLocal(transform.localPosition, speed).setEase(easeType);
				hasAnimate = true;
			}
		}
	}
	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		if (!Application.isPlaying) {
			Gizmos.DrawLine(transform.position, transform.position + startOffset);
		} else {
			Gizmos.DrawLine(transform.position, instance.transform.position);
		}
		if (withZScale) {
			zStuff = transform.localScale.z;
		}
		Gizmos.color = Color.yellow;
		if (withZScale) {
			Gizmos.DrawWireSphere(transform.position, minimalDistance * transform.localScale.z / 0.5f);
		} else {
			Gizmos.DrawWireSphere(transform.position, minimalDistance);
		}
	}
}