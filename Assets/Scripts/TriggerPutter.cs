using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerPutter : MonoBehaviour
{
	public GameObject Instance;
	public Vector3 Offset = new Vector3(1,0,1);
	public KeyCode spawnKey = KeyCode.T;
	public bool isTrigger;
	public TriggerPutter targTrigPutter;
	LineMovement line;
	
    void Start()
    {
    	line = FindObjectOfType<LineMovement>();
    }
    
    void Update()
    {
    	if(!isTrigger){
    		if(Input.GetKeyDown(spawnKey)){
    			GameObject h = Instantiate(Instance,transform.position + Offset,Instance.transform.rotation);
    		}
    	}
    }
    public void OnTriggerEnter(Collider other){
    	if(other.tag == "Player"){
    		if(isTrigger){
    			targTrigPutter.Offset = Offset;
    		}
    	}
    }
}
