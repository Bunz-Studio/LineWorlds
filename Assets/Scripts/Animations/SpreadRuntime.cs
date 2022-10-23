using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SpreadRuntime : MonoBehaviour
{
    public bool simulate = true;
    public float distance;

    public List<Transform> transforms = new List<Transform>();
    public bool getChilds;

    void Update()
    {
        if(getChilds)
        {
            transforms.Clear();
            transforms = new List<Transform> (GetComponentsInChildren<Transform>());
            transforms.Remove(this.transform);
            getChilds = false;
        }
        if(!Application.isPlaying && !simulate) return;
        CalculateDistance();
    }

    public void CalculateDistance()
    {
        int limit = transforms.Count;
        for(int i = 0; i < transforms.Count; i++)
        {
            if(transforms[i] != null)
            {
                int indexCalc = transforms.Count % 2 == 0 && i > 0 ? 
                (i + 1) - (transforms.Count / 2) : 
                i - (transforms.Count / 2);
                transforms[i].transform.localPosition = new Vector3(indexCalc * distance, 0, 0);
            }
            else
            {
                transforms.RemoveAt(i);
                limit--;
                i--;
            }
        }
    }
}
