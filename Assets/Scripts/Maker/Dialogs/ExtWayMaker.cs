using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExternMaker
{
    public class ExtWayMaker : MonoBehaviour
    {
        public CanvasGroup group;
        public CanvasGroup secondGroup;

        public Button turnOnButton;
        public Button turnOffButton;

        public Slider minSlider;
        public Slider maxSlider;

        public Slider widthSlider;
        public Slider heightSlider;

        public Material glMaterial;

        public Color wayColor = Color.white;
        public ExtInsColor insColor;

        public List<int> wayGroups = new List<int>();
        public ExtInsListInt wayGroupsInspector;

        List<GameObject> registeredTails = new List<GameObject>();

        List<GameObject> tails
        {
            get
            {
                return ExtCore.instance.lineMovement.tails;
            }
            set
            {
                ExtCore.instance.lineMovement.tails = value;
            }
        }
        public bool createMode;

        private void Start()
        {
            ExtCore.instance.onPlaymodeChanged += UpdateSituation;
            //ExtGrid.instance.onCameraPostRender += UpdateRender;
            minSlider.onValueChanged.AddListener((val) => UpdatePreview());
            maxSlider.onValueChanged.AddListener((val) => UpdatePreview());
            UpdateSituation(ExtCore.playState);
            insColor.propertyInfo = new ExtProperty(GetType().GetField("wayColor"));
            insColor.sources.Add(this);
            insColor.Initialize();
            insColor.ApplyTemp();
            wayGroupsInspector.propertyInfo = new ExtProperty(GetType().GetField("wayGroups"));
            wayGroupsInspector.sources = this.WrapInList();
            wayGroupsInspector.Initialize();
            wayGroupsInspector.ApplyTemp();
        }

        public void UpdateSituation(EditorPlayState state)
        {
            group.interactable = state == EditorPlayState.Paused;
            if (state != EditorPlayState.Paused)
            {
                if (createMode) SetCreateMode(false);
            }
            else
            {
                UpdatePreview();
            }
        }

        public void SetCreateMode(bool to)
        {
            createMode = to;
            ExtSelection.instance.ClearTargets();
            ExtInspector.instance.lockInspector = to;
            secondGroup.interactable = to;
            turnOnButton.gameObject.SetActive(!to);
            turnOffButton.gameObject.SetActive(to);
            ExtSelection.instance.transformGizmo.allowSelection = !to;
            if(to)
                UpdatePreview();
        }

        public void UpdatePreview()
        {
            minSlider.maxValue = tails.Count;
            maxSlider.maxValue = tails.Count;
            if (minSlider.value > maxSlider.value) minSlider.value = maxSlider.value;
            if (maxSlider.value < minSlider.value) maxSlider.value = minSlider.value;

            registeredTails.Clear();
            for(int i = 0; i < tails.Count; i++)
            {
                if(i + 1 >= minSlider.value && i < maxSlider.value)
                {
                    registeredTails.Add(tails[i]);
                }
            }

            if(createMode) ExtSelection.instance.gameObjectSelection = registeredTails;
        }

        public void ProceedCreate()
        {
            var posList = new List<Vector3>();
            var rotList = new List<Vector3>();
            var scaleList = new List<Vector3>();

            foreach (var t in registeredTails)
            {
                var trTail = t.transform;
                var h = heightSlider.value;
                var w = widthSlider.value;
                var posY = trTail.position.y - (trTail.localScale.y / 2) - (h / 2);
                posList.Add(new Vector3(trTail.position.x, posY, trTail.position.z));
                rotList.Add(trTail.eulerAngles);
                scaleList.Add(new Vector3(w, h, trTail.localScale.z + (w - 1)));
            }

            var action = new ExtActionInstance();
            action.objects = new object[]{posList, rotList, scaleList, new List<GameObject>(), new Color(wayColor.r, wayColor.g, wayColor.b, wayColor.a)};
            action.action += () =>
            {
                var posListAl = (List<Vector3>)action.objects[0];
                var rotListAl = (List<Vector3>)action.objects[1];
                var scaleListAl = (List<Vector3>)action.objects[2];
                var objectsList = (List<GameObject>)action.objects[3];
                var color = (Color)action.objects[4];

                for (int i = 0; i < posList.Count; i++)
                {
                    var obj = ExtCore.instance.CreateObject(ExtCore.instance.configurations[0]);
                    obj.transform.position = posListAl[i];
                    obj.transform.eulerAngles = rotListAl[i];
                    obj.transform.localScale = scaleListAl[i];
                    obj.GetComponent<MeshRenderer>().sharedMaterial.color = color;
                    obj.groupID = wayGroups;
                    objectsList.Add(obj.gameObject);
                }
            };

            action.undo += () =>
            {
                var objectsList = (List<GameObject>)action.objects[3];
                foreach (var l in objectsList)
                {
                    DestroyImmediate(l);
                }
                objectsList.Clear();
            };
            action.AddToManager();
            action.action.Invoke();
        }

        /*public void UpdateRender()
        {
            if(createMode)
            {
                GL.Begin(GL.LINES);
                glMaterial.SetPass(0);
                GL.Color(gizmoColor);

                foreach(var t in registeredTails)
                {
                    var trTail = t.transform;
                    var h = heightSlider.value;
                    var posY = trTail.position.y - (trTail.localScale.y / 2) - (h / 2);
                    var scaleX = 
                }

                GL.End();
            }
        }*/

        public void DrawCube(Vector3 position, Vector3 scale)
        {
            var v1 = position + new Vector3(-scale.x / 2, scale.y / 2, -scale.z / 2);
            var v2 = position + new Vector3(-scale.x / 2, scale.y / 2, scale.z / 2);
            var v3 = position + new Vector3(-scale.x / 2, -scale.y / 2, -scale.z / 2);
            var v4 = position + new Vector3(-scale.x / 2, -scale.y / 2, scale.z / 2);
            var v5 = position + new Vector3(scale.x / 2, scale.y / 2, -scale.z / 2);
            var v6 = position + new Vector3(scale.x / 2, scale.y / 2, scale.z / 2);
            var v7 = position + new Vector3(scale.x / 2, -scale.y / 2, -scale.z / 2);
            var v8 = position + new Vector3(scale.x / 2, -scale.y / 2, scale.z / 2);
            DrawFace(v1, v2, v3, v4);
            DrawFace(v5, v6, v7, v8);
            ExtGrid.DrawGLLine(v1, v5);
            ExtGrid.DrawGLLine(v2, v6);
            ExtGrid.DrawGLLine(v3, v7);
            ExtGrid.DrawGLLine(v4, v8);
        }
        public void DrawFace(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            ExtGrid.DrawGLLine(v1, v2);
            ExtGrid.DrawGLLine(v2, v3);
            ExtGrid.DrawGLLine(v3, v4);
            ExtGrid.DrawGLLine(v4, v1);
        }

        public void ExitMaker()
        {
            gameObject.SetActive(false);
            SetCreateMode(false);
        }
    }
}
