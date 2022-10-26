using UnityEngine;
using System.Collections;
using Kino;
using ExternMaker;

public class AnalogGlitchTrigger : SerializableTrigger
{
    [IgnoreSavingState]
    public AnalogGlitch glitchScript;

    public float pulseIntensity = 1;
    public LeanTweenType easeType = LeanTweenType.easeOutCubic;

    public bool onlySet;
    public float duration = 0.5f;

    public override void OnEnter(Collider other)
    {
        base.OnEnter(other);
        glitchScript = glitchScript ?? FindObjectOfType<AnalogGlitch>();
        if(glitchScript != null)
        {
            if(onlySet)
            {
                glitchScript.scanLineJitter = pulseIntensity;
                return;
            }

            LeanTween.value(glitchScript.gameObject, pulseIntensity, 0, duration).setEase(easeType).setOnUpdate((val) => {
                glitchScript.scanLineJitter = val;
            });
        }
    }
}
