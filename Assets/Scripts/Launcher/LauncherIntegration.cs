using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace LineWorlds.Launcher
{
    public class LauncherIntegration : MonoBehaviour
    {
        public bool isLauncherValid
        {
            get
            {
                return string.IsNullOrWhiteSpace(launcherPath) ? false : File.Exists(launcherPath);
            }
        }

        public string launcherPath;
        public string launcherDirectory;

        public void GetLauncherPath()
        {
            string path = Storage.GetPathLocal("launcherPath");
            if (!File.Exists(path)) return;

            SetLauncherPath(File.ReadAllText(path));
        }

        public void SetLauncherPath(string path)
        {
            launcherPath = path;
            launcherDirectory = Storage.GoUpFolder(path);
        }

        public Preferences GetPreferences()
        {
            string path = Path.Combine(launcherDirectory, "preferences.json");
            if (!File.Exists(path)) return null;

            var data = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Preferences>(data);
        }
    }
}
