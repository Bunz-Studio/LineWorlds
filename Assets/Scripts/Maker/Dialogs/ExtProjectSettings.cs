using System.Collections.Generic;
using UnityEngine;

namespace ExternMaker
{
    public class ExtProjectSettings : MonoBehaviour
    {
        public ExtCore core;
        public Color backgroundColor
        {
            get
            {
                return core.playmodeCamera.mainCamera.backgroundColor;
            }
            set
            {
                if(core.editorCamera != null) ExtCore.instance.editorCamera.backgroundColor = value;
                core.playmodeCamera.mainCamera.backgroundColor = value;
            }
        }

        public LiwProjectInfo projectInfo = new LiwProjectInfo();

        public ExtInsColor backgroundColorInspect;

        public ExtProjectManager projectManager;
        public List<ProjectInfoFieldInspect> projectInfoFieldInspects;

        public List<RenderFieldInspect> renderFieldInspect;

        private void Start()
        {
            // Background Color Field
            backgroundColorInspect.propertyInfo = new ExtProperty(GetType().GetProperty("backgroundColor"));
            backgroundColorInspect.Initialize();
            backgroundColorInspect.sources.Add(this);

            // Render Settings Fields
            foreach (var insp in renderFieldInspect)
            {
                insp.extFieldInspect.isStatic = true;
                insp.Initialize();
            }

            // Project Info Fields
            foreach (var insp in projectInfoFieldInspects)
            {
                insp.extFieldInspect.sources = new List<object>(new object[] { projectInfo });
                insp.extFieldInspect.fieldFinishEdit += (aa) =>
                {
                    projectManager.project.info = projectInfo;
                };
                insp.Initialize();
            }

            UpdateInspectors();
        }

        public void UpdateInspectors()
        {
            backgroundColorInspect.ApplyTemp();

            foreach (var insp in projectInfoFieldInspects)
            {
                insp.extFieldInspect.sources = new List<object>(new object[] { projectInfo });
                insp.extFieldInspect.ApplyTemp();
            }
            foreach (var insp in renderFieldInspect)
            {
                insp.extFieldInspect.sources.Clear();
                insp.extFieldInspect.ApplyTemp();
            }
        }
    }

    [System.Serializable]
    public class RenderFieldInspect : ExtInspectableUnite.InspectField<RenderSettings>
    {
    }

    [System.Serializable]
    public class ProjectInfoFieldInspect : ExtInspectableUnite.InspectField<LiwProjectInfo>
    {
    }
}
