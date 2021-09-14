using Packages.Excursion360_Builder.Editor.WebBuild.RemoteItems;
using Packages.tour_creator.Editor.WebBuild.GitHubAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Packages.Excursion360_Builder.Editor.RemoteItemsControllers
{
    internal abstract class RemoteControllerGUI<T>
        where T : RemoteBuildPack
    {
        public T CurrentPack { get; private set; }

        protected abstract List<T> FindPacks(string rootLocation);
        protected abstract string PacksFolderName { get; }
        protected abstract string GithubRepo { get; }
        protected abstract void RemovePack(T pack);
        protected abstract IEnumerator DownloadPack(string rootLocation, ReleaseResponse release, Action<string> errorAction);


        private string packsLocation;
        private List<T> buildPacks = new List<T>();
        private int selectedbuildTagNum = 0;
        private string[] buildPackTags = Array.Empty<string>();

        /// <summary>
        /// Load first info, do it in Enable
        /// </summary>
        public void Initialize()
        {
            packsLocation = Path.Combine(Application.dataPath, "Tour creator", PacksFolderName);
            if (!Directory.Exists(packsLocation))
            {
                Directory.CreateDirectory(packsLocation);
            }

            UpdatePacksList();
            
            if (buildPacks.Any())
            {
                CurrentPack = buildPacks[selectedbuildTagNum];
            }
        }

        private void UpdatePacksList()
        {
            buildPacks = FindPacks(packsLocation);
            buildPackTags = buildPacks.Select(p => p.Version).ToArray();
            selectedbuildTagNum = buildPacks.Count - 1;
        }

        public void Draw()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUI.indentLevel * 20);
            if (GUILayout.Button("Download last stable"))
            {
                BackgroundTaskInvoker.StartBackgroundTask(DownloadPack(ReleaseType.Stable));
            }
            if (GUILayout.Button("pre-release"))
            {
                BackgroundTaskInvoker.StartBackgroundTask(DownloadPack(ReleaseType.PreRelease));
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Available", EditorStyles.boldLabel);
            if (GUILayout.Button("Refresh"))
            {
                UpdatePacksList();
            }

            GUILayout.EndHorizontal();
            if (buildPacks.Count == 0)
            {
                EditorGUILayout.LabelField("No versions", EditorStyles.label);
            }
            else
            {
                foreach (var pack in buildPacks)
                {
                    pack.IsFolded = EditorGUILayout.Foldout(pack.IsFolded, pack.Version);
                    if (pack.IsFolded)
                    {
                        RenderPack(pack);
                    }
                }
            }
            if (buildPacks.Count > 0)
            {
                selectedbuildTagNum = EditorGUILayout.Popup("Selected version", selectedbuildTagNum, buildPackTags);
                CurrentPack = buildPacks[selectedbuildTagNum];
            }
            else
            {
                EditorGUILayout.LabelField("Please, download some version", EditorStyles.label);
                CurrentPack = null;
            }
        }

        private IEnumerator DownloadPack(ReleaseType releaseType)
        {
            EditorUtility.DisplayProgressBar("Downloading", $"Downloading latest {releaseType}", 0);
            try
            {
                ReleaseResponse parsed = null;
                string errorMessage = null;
                var releaseTask = GitHubApi.GetLatestReleaseForRepo(
                    GithubRepo,
                    releaseType,
                    r => parsed = r,
                    e => errorMessage = e);
                while (releaseTask.MoveNext())
                {
                    yield return releaseTask.Current;
                }
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    EditorUtility.DisplayDialog("Error", errorMessage, "Ok");
                    yield break;
                }

                if (buildPacks.Any(p => p.Id == parsed.id))
                {
                    EditorUtility.DisplayDialog("Info", $"Version {parsed.tag_name} already downloaded", "Ok");
                    yield break;
                }

                var downloadTask = DownloadPack(packsLocation, parsed, error =>
                {
                    errorMessage = error;
                });
                while(downloadTask.MoveNext())
                {
                    yield return downloadTask.Current;
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        EditorUtility.DisplayDialog("Error", errorMessage, "Ok");
                        yield break;
                    }
                }
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    EditorUtility.DisplayDialog("Error", errorMessage, "Ok");
                    yield break;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                UpdatePacksList();
            }
        }

        private void RenderPack(T pack)
        {
            EditorGUI.indentLevel++;
            switch (pack.Status)
            {
                case BuildPackStatus.NotLoaded:
                    BackgroundTaskInvoker.StartBackgroundTask(GitHubApi.GetReleaseForRep(GithubRepo, pack.Id, p =>
                    {
                        pack.PublishDate = p.PublishedAt;
                        pack.Status = BuildPackStatus.Loaded;
                    }, e => pack.Status = BuildPackStatus.LoadingError));
                    pack.Status = BuildPackStatus.Loading;
                    break;
                case BuildPackStatus.Loading:
                    EditorGUILayout.LabelField($"Downloading...");
                    break;
                case BuildPackStatus.Loaded:
                    EditorGUILayout.LabelField($"Publish date: {pack.PublishDate:yyyy-MM-dd}");
                    if (GUI.Button(EditorGUI.IndentedRect(EditorGUILayout.GetControlRect()), "Remove"))
                    {
                        RemovePack(pack);
                        UpdatePacksList();
                    }
                    break;
                case BuildPackStatus.LoadingError:
                    EditorGUILayout.LabelField($"Download error");
                    break;
                default:
                    EditorGUILayout.LabelField($"Unexpected error");
                    break;
            }
            EditorGUI.indentLevel--;
        }
    }
}
