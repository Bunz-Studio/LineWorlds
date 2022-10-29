using System.Collections.Generic;
using UnityEngine;

public class UIAnimationControl : MonoBehaviour
{
	public Animator animator;
	public CanvasGroup gr;
    public string openState;
	public string closeState;
    public bool useTransform;
    public Transform openedTransform;
    public Transform closedTransform;

    public bool isOpened;
    bool wasOpened;
	
	public void SwitchState()
	{
        if (useTransform)
        {
            Vector3 target = isOpened ? closedTransform.transform.position : openedTransform.transform.position;
            transform.position = target;
        }
        else
        {
            string state = isOpened ? closeState : openState;
            animator.Play(state);
        }
		isOpened = !isOpened;
	}

    public void MaybeClose()
    {
        if (isOpened)
        {
            if (useTransform)
            {
                transform.position = closedTransform.transform.position;
            }
            else
            {
                animator.Play(closeState);
            }
        }
        wasOpened = isOpened;
        isOpened = false;
    }

    public void MaybeOpen()
    {
        if (wasOpened)
        {
            if (useTransform)
            {
                transform.position = openedTransform.transform.position;
            }
            else
            {
                animator.Play(openState);
            }
            isOpened = true;
        }
    }
    
    public void FloatOpen()
    {
        LeanTween.cancel(gameObject);
    	gameObject.SetActive(true);
    	CheckCanvasGroup();
    	if(gr != null)
    	{
			LeanTween.value(gameObject, 0, 1f, 0.5f).setEase(LeanTweenType.easeOutCubic).setOnUpdate((float val) => gr.alpha = val);
    	}
		LeanTween.value(gameObject, 0.9f, 1, 0.5f).setEase(LeanTweenType.easeOutCubic).setOnUpdate((float val) =>
			transform.localScale = new Vector3(val, val, 1)
		);
    }
    
    public void FloatClose()
    {
        LeanTween.cancel(gameObject);
        CheckCanvasGroup();
    	if(gr != null)
    	{
			LeanTween.value(gameObject, 1f, 0f, 0.5f).setEase(LeanTweenType.easeOutCubic).setOnUpdate((float val) => gr.alpha = val);
    	}
		LeanTween.value(gameObject, 1, 0.9f, 0.5f).setEase(LeanTweenType.easeOutCubic).setOnUpdate((float val) =>
			transform.localScale = new Vector3(val, val, 1)
		).setOnComplete(() => gameObject.SetActive(false));
    }
    
    public void CheckCanvasGroup()
    {
    	if(gr == null) gr = GetComponent<CanvasGroup>();
    }
}
