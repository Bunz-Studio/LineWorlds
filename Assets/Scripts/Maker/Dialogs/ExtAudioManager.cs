using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SimpleFileBrowser;

namespace ExternMaker
{
    public class ExtAudioManager : MonoBehaviour
    {
        public ExtCore core;
        public UISkin fileBrowserSkin;
        public ExtInsString audioFileInput;
        public string originalAudioPath;
        public Slider musicSlider;
        public AudioSource source;
        public AudioClip loadedClip;

        public void BrowseAudio()
        {
            source.Stop();
            var filter1 = new FileBrowser.Filter("Supported audio files", ".ogg", ".mp3");
            var filter2 = new FileBrowser.Filter("OGG Vorbis", ".ogg");
            var filter3 = new FileBrowser.Filter("MPEG Audio File", ".mp3");
            FileBrowser.Skin = fileBrowserSkin;
            FileBrowser.SetFilters(true, filter1, filter2, filter3);
            FileBrowser.SetDefaultFilter(".ogg");
            FileBrowser.ShowLoadDialog(
                (val) =>
                {
                    TryImport(val[0]);
                },
                () =>
                {
                },
                FileBrowser.PickMode.Files,
                false,
                null,
                "audio.ogg",
                "Select Audio File"
            );
        }

        public void TryImport(string path)
        {
            var loadingPanel = LoadingPanelManager.CreatePanel("Loading audio: " + path);
            loadingPanel.SetProgress(0.1f);
            originalAudioPath = path;
            var onSuccess = default(Action<AudioClip>);
            var onFailed = default(Action);
            onSuccess += (clip) =>
            {
                if(loadedClip != null) Destroy(loadedClip);
                loadedClip = clip;
                ExtCore.instance.lineMovement.GetComponent<AudioSource>().clip = clip;
                source.clip = clip;
                _audioLength = source.clip.length;
                ExtProjectManager.instance.AddAudio(path);
                Destroy(loadingPanel.gameObject);
            };
            onFailed += () =>
            {
                Debug.LogError("Failed to load audio");
                Destroy(loadingPanel.gameObject);
            };
            core.levelManager.TryLoadingAudio(originalAudioPath, onSuccess, onFailed);
        }

        public void TryImport(LiwbFile file)
        {
            if (file.isExternal)
            {
                originalAudioPath = System.IO.Path.Combine(ExtProjectManager.exeDirectory, "Levels", file.path);
                var onSuccess = default(Action<AudioClip>);
                var onFailed = default(Action);
                onSuccess += (clip) =>
                {
                    if (loadedClip != null) Destroy(loadedClip);
                    loadedClip = clip;
                    core.lineMovement.GetComponent<AudioSource>().clip = clip;
                    source.clip = clip;
                    _audioLength = source.clip.length;
                };
                onFailed += () =>
                {
                    Debug.LogError("Failed to load audio");
                };
                core.levelManager.TryLoadingAudio(originalAudioPath, onSuccess, onFailed);
            }
            else
            {
                Storage.CheckDirectory("Cache");
                originalAudioPath = System.IO.Path.Combine(ExtProjectManager.exeDirectory, "Cache", file.path);
                System.IO.File.WriteAllBytes(originalAudioPath, file.GetBytes());
                var onSuccess = default(Action<AudioClip>);
                var onFailed = default(Action);
                onSuccess += (clip) =>
                {
                    if (loadedClip != null) Destroy(loadedClip);
                    loadedClip = clip;
                    core.lineMovement.GetComponent<AudioSource>().clip = clip;
                    source.clip = clip;
                    _audioLength = source.clip.length;
                };
                onFailed += () =>
                {
                    Debug.LogError("Failed to load audio");
                };
                core.levelManager.TryLoadingAudio(originalAudioPath, onSuccess, onFailed);
            }
        }

        private void Start()
        {
            source.clip = core.lineMovement.GetComponent<AudioSource>().clip;
            _audioLength = source.clip.length;
            musicSlider.maxValue = 1f;
            musicSlider.onValueChanged.AddListener(OnSliderValueChange);
        }

        private void Update()
        {
            if (!Input.GetMouseButton(0))
                musicSlider.value = source.time / source.clip.length;
        }

        private float _audioLength;

        private void OnSliderValueChange(float f) => source.time = f * _audioLength;
        private void OnDisable() => musicSlider.onValueChanged.RemoveAllListeners();
    }
}
