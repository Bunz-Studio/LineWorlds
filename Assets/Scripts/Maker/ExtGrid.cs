using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExternMaker
{
    [ExecuteInEditMode]
    public class ExtGrid : MonoBehaviour
    {
        private static ExtGrid p_instance;
        public static ExtGrid instance
        {
            get
            {
                p_instance = ExtUtility.GetStaticInstance(p_instance);
                return p_instance;
            }
        }
		
        public Camera targetCamera;

        public Transform[] directionGizmos;
        
        public bool enableGrid = true;

        private float p_lineSpeed = 15;
        public float lineSpeed
        {
            get
            {
                return ExtCore.instance != null ? ExtCore.instance.lineMovement.lineSpeed / 2 : p_lineSpeed;
            }
        }

        public bool enableSnapping;

        public bool followObject;

        public float yPosition;
        [Range(1, 1000)]
        public float musicBPM = 120;
        [Range(1, 1000)]
        public float scale = 10;
        [Range(1, 1000)]
        public float maximumGrid = 200;

        public float snapSize
        {
            get
            {
                return 120 / musicBPM * (10 / scale) * lineSpeed;
            }
        }

        public Color gizmoColor = new Color(0, 1, 0, 0.25f);
        // Previous values
        private Transform prevObject;
        private Vector3 prevVector;
        
        public Material lineMaterial;
        public Shader lineShader;

        private void Start()
        {
            p_instance = this;
        }

        private void OnDestroy()
        {
            p_instance = null;
        }

        public void Update()
        {
            if (!enableGrid) return;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (scale > 0 && musicBPM > 0 && lineSpeed > 0)
                {
                    if (enableSnapping)
                    {
                        if (UnityEditor.Selection.gameObjects.Length > 0)
                        {
                            var obj = UnityEditor.Selection.gameObjects[0];
                            var pos = obj.transform.position;
                            if (prevObject == null)
                            {
                                prevObject = obj.transform;
                                prevVector = pos;
                            }
                            if (prevObject == obj.transform)
                            {
                                if (prevVector != pos)
                                {
                                    prevVector = pos;
                                    var adjusment = GetNearestPointOnGrid(pos);
                                    adjusment = new Vector3(adjusment.x, pos.y, adjusment.z);
                                    obj.transform.position = adjusment;
                                }
                            }
                            else
                            {
                                prevObject = obj.transform;
                                prevVector = pos;
                            }
                        }
                    }
                }
                else
                {
                    if (enableSnapping)
                    {
                        enabled = false;
                        Debug.LogError("None of the values can be below zero.");
                    }
                }
            }
#endif
            /*if(Application.isPlaying)
            {
                if (enableSnapping)
                {
                    ExtSelection.instance.transformGizmo.isSnapping = true;
                    ExtSelection.instance.transformGizmo.movementSnap = snapSize;
                }
                else
                {
                    ExtSelection.instance.transformGizmo.movementSnap = 0.5f;
                }
                ExtSelection.instance.transformGizmo.toggleSnapWithKey = !enableSnapping;
            }*/
        }

        private void OnDrawGizmos()
        {
            if (!enableGrid) return;
            DrawGizmosWithGL();
        }

        public Action onCameraPostRender;
        public List<ActionQueue> actionsQueue = new List<ActionQueue>();

        private void OnPostRender()
        {
            if (!enableGrid) return;
            /*foreach (var action in actionsQueue)
            {
                if (action.onCall != null) action.onCall.Invoke();
            }*/
            actionsQueue.Clear();
            DrawGizmosWithGL();
        }

        void CreateLineMaterial()
        {
            if (!lineMaterial)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                lineMaterial = new Material(lineShader);
#pragma warning restore CS0618 // Type or member is obsolete
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        public void DrawGizmosWithGL()
        {
        	if(targetCamera != null && Camera.current != targetCamera) return;
        	var sc = scale;
        	var camPos = Camera.current.transform.position;
            Vector3 off = GetNearestPointOnGrid(new Vector3(camPos.x, 0, camPos.z));
            CreateLineMaterial();
            lineMaterial.SetPass(0);
            foreach(var trans in directionGizmos)
            {
                GL.Begin(GL.LINES);
                GL.Color(Color.green);
                DrawGLLine(trans.position, trans.position + trans.forward * 10);
                GL.End();
            }
            GL.Begin(GL.LINES);
            GL.Color(gizmoColor);
            //GL.Vertex3(0, 0, 0);
            if (scale > 0 && musicBPM > 0 && lineSpeed > 0)
            {
                float size = 120 / musicBPM * (10 / sc) * lineSpeed;
                float length = maximumGrid * 1.05f;
                for (float x = -maximumGrid; x < maximumGrid; x += size)
                {
                    var point = GetNearestPointOnGrid(new Vector3(x, 0, 0f) + off);
                    if (followObject)
                    {
                        Vector3 offs = ExtSelection.instance.mainTransformSelected == null ? Vector3.zero : ExtSelection.instance.mainTransformSelected.position;
                        point += new Vector3(offs.x, offs.y + yPosition, offs.z);
                    }
                    else
                    {
                        point += new Vector3(0, yPosition, 0);
                    }
                    Vector3 vect = new Vector3(0, 0, length);
                    var v_start = point + vect;
                    var v_end = point + (-vect);
                    DrawGLLine(v_start, v_end);
                }
                for (float z = -maximumGrid; z < maximumGrid; z += size)
                {
                    var point = GetNearestPointOnGrid(new Vector3(0, 0, z) + off);
                    if (followObject)
                    {
                        Vector3 offs = ExtSelection.instance.mainTransformSelected == null ? Vector3.zero : ExtSelection.instance.mainTransformSelected.position;
                        point += new Vector3(offs.x, offs.y + yPosition, offs.z);
                    }
                    else
                    {
                        point += new Vector3(0, yPosition, 0);
                    }
                    Vector3 vect = new Vector3(length, 0, 0);
                    var v_start = point + vect;
                    var v_end = point + (-vect);
                    DrawGLLine(v_start, v_end);
                }
            }
            GL.End();
        }

        public static void DrawGLLine(Vector3 start, Vector3 end)
        {
            GL.Vertex3(start.x, start.y, start.z);
            GL.Vertex3(end.x, end.y, end.z);
        }

        public Vector3 GetNearestPointOnGrid(Vector3 position, bool addTransformPosition = false)
        {
            float size = 120 / musicBPM * (10 / scale) * lineSpeed;
            int xCount = Mathf.RoundToInt(position.x / size);
            int yCount = Mathf.RoundToInt(position.y / size);
            int zCount = Mathf.RoundToInt(position.z / size);

            Vector3 result = new Vector3(
                xCount * size,
                yCount * size,
                zCount * size
            );
            if (addTransformPosition) result += new Vector3(0, yPosition, 0);

            return result;
        }
    }

    [System.Serializable]
    public class ActionQueue
    {
        public Action onCall;
    }
}