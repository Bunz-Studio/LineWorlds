using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TMPAnimaText : MonoBehaviour
{
	public TextMesh text;
	
	public TextMesh instance;
	public List<TextAnimateItem> instances = new List<TextAnimateItem>();
	
	public bool generate;
	public bool fetchChildrens;
	
    void Update()
    {
    	if(generate)
    	{
    		GenerateText();
    		generate = false;
    	}
    	if(fetchChildrens)
    	{
    		foreach(var c in GetComponentsInChildren<TextAnimateItem>())
    		{
    			instances.Add(c);
    		}
    		fetchChildrens = false;
    	}
    }
    
    public void GenerateText()
    {
    	foreach(var i in instances)
    	{
    		if(i != null)
    		DestroyImmediate(i.gameObject);
    	}
    	instances.Clear();
    	foreach(var a in text.text)
    	{
    		var i = Instantiate(instance.gameObject).GetComponent<TextMesh>();
    		var tm = i.GetComponent<TMPAnimaText>();
    		if(tm != null) DestroyImmediate(tm);
    		var ta = i.gameObject.AddComponent<TextAnimateItem>();
    		ta.mesh = i;
    		i.text = a.ToString();
    		i.transform.SetParent(text.transform);
    		instances.Add(ta);
    	}
    }
    
    public int animateIndex;
    
    public void AnimateText(float time)
    {
    	if(Application.isPlaying)
    	{
    		if(animateIndex < instances.Count)
    		{
    			var ins = instances[animateIndex];
    			ins.AnimateBack(time);
    			animateIndex++;
    		}
    	}
    }
}
