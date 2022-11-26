using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LineWorlds.Launcher
{
    public static class LauncherClasses
    {
        public const string versionReference = "2.0";
    }

    public class Preferences
    {
        public List<Project> projects = new List<Project>();
        public List<Editor> editors = new List<Editor>();
        public int updaterUpdate;

        public string editorPaths = "Editor";
        public string cachePaths = "Cache";

        public class Project
        {
            public string name;
            public string path;
            public LiwProjectInfo info;
        }
        public class Editor
        {
            public string name;
            public string path;
        }
    }
}
