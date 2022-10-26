using UnityEngine;
using ExternMaker;

public class WorldInterface : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public float maxDistance = 40;
    public float fadeFactor = 5;

    void Start()
    {
        CameraEditorMovement.OnCameraMove += OnCameraMove;
    }

    public void OnRenderCam()
    {
        var queue = new ActionQueue();
        queue.onCall += () =>
        {
            GL.Begin(GL.LINES);
            GL.Color(Color.green);
            ExtGrid.DrawGLLine(transform.position, transform.position + transform.forward * 10);
            GL.End();
        };
        ExtGrid.instance.actionsQueue.Add(queue);
    }

    public void OnCameraMove(Camera cam)
    {
        transform.LookAt(cam.transform);

        if (spriteRenderer == null) return;
        var distance = Vector3.Distance(transform.position, cam.transform.position);
        if(distance > maxDistance)
        {
            if(distance - maxDistance > fadeFactor)
            {
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
            }
            else
            {
                var factor = distance - maxDistance - fadeFactor;
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1 - (factor / fadeFactor));
            }
        }
        else
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1);
        }
    }

    private void OnDestroy()
    {
        CameraEditorMovement.OnCameraMove -= OnCameraMove;
    }
}
