using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmosBone : MonoBehaviour
{
    public bool showDirection;
    public float directionScale = 2;

    public float sphereSize = 0.2f;

	public Transform[] matches;

	void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if(showDirection)
        {
            Gizmos.DrawLine(transform.position, transform.position + (transform.up * directionScale));
        }
        Gizmos.DrawWireSphere(transform.position, sphereSize);
        foreach (var match in matches)
        {
            Gizmos.DrawLine(transform.position, match.position);
        }
    }
}
