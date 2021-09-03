using Packages.Excursion360_Builder.Editor.WebBuild;
using Packages.tour_creator.Editor.WebBuild.GitHubAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Packages.Excursion360_Builder.Editor.Viewer
{
    class ViewerBuildsGUI
    {
        private static string packsLocation;
        private static List<BuildPack> buildPacks = new List<BuildPack>();
        private static string[] buildPackTags = Array.Empty<string>();
        private static int selectedbuildTagNum = 0;

        static ViewerBuildsGUI()
        {
            packsLocation = Application.dataPath + "/Tour creator";
            if (!Directory.Exists(packsLocation))
            {
                Directory.CreateDirectory(packsLocation);
            }
            FindBuildPacks();
            selectedbuildTagNum = buildPacks.Count - 1;
        }
        public static BuildPack Draw()
        {
            if (GUILayout.Button("Download last viewer version"))
            {
                BackgroundTaskInvoker.StartBackgroundTask(DownloadViewer(packsLocation));
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("Available viewers", EditorStyles.boldLabel);
            if (GUILayout.Button("Refresh"))
            {
                FindBuildPacks();
            }

            GUILayout.EndHorizontal();
            if (buildPacks.Count == 0)
            {
                GUILayout.Label("No web viewers", EditorStyles.label);
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
                return buildPacks[selectedbuildTagNum];
            }
            else
            {
                GUILayout.Label("Please, download viewer", EditorStyles.label);
                return null;
            }
        }
        private static void FindBuildPacks()
        {
            buildPacks = Directory.GetFiles(packsLocation, "web-viewer-*-*.zip")
                .Select(path => (path, match: Regex.Match(path, @"web-viewer-(?<tag>\S+)-(?<id>\d+).zip")))
                .Select(b => new BuildPack
                {
                    Id = int.Parse(b.match.Groups["id"].Value),
                    Version = b.match.Groups["tag"].Value,
                    Location = b.path
                })
                .ToList();
            buildPackTags = buildPacks.Select(p => p.Version).ToArray();
            selectedbuildTagNum = buildPackTags
                .Select((tag, i) => (tag, i))
                .OrderBy(s => s.tag)
                .FirstOrDefault()
                .i;
        }
        private static IEnumerator DownloadViewer(string folderPath)
        {
            EditorUtility.DisplayProgressBar("Downloading", "Downloading latest viewer", 0);
            try
            {
                ReleaseResponse parsed = null;
                string errorMessage = null;
                var downloadingTask = DownloadReleaseInfo(
                    "latest",
                    r => parsed = r,
                    e => errorMessage = e);
                while (downloadingTask.MoveNext())
                {
                    yield return downloadingTask.Current;
                }
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    EditorUtility.DisplayDialog("Error", errorMessage, "Ok");
                    yield break;
                }
                var targetLink = parsed.assets.FirstOrDefault(a => a.name == "build.zip");
                if (targetLink == null)
                {
                    EditorUtility.DisplayDialog("Error", "No needed asset in latest release", "Ok");
                    yield break;
                }

                using (UnityWebRequest w = UnityWebRequest.Get(targetLink.browser_download_url))
                {
                    w.SetRequestHeader("User-Agent", "Mozilla/5.0");
                    yield return w.SendWebRequest();

                    while (w.isDone == false)
                    {
                        yield return null;
                        EditorUtility.DisplayProgressBar("Downloading", "Downloading viewer", w.downloadProgress);
                    }
                    File.WriteAllBytes(Path.Combine(folderPath, $"web-viewer-{parsed.tag_name}-{parsed.id}.zip"), w.downloadHandler.data);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                FindBuildPacks();
            }
        }
        private static IEnumerator DownloadReleaseInfo(int releaseId, Action<ReleaseResponse> done, Action<string> error)
           => DownloadReleaseInfo(releaseId.ToString(), done, error);
        private static IEnumerator DownloadReleaseInfo(string releaseId,
            Action<ReleaseResponse> done,
            Action<string> error)
        {
            try
            {
                string row;
                using (UnityWebRequest w = UnityWebRequest.Get("https://api.github.com/repos/RTUITLab/Excursion360-Web/releases/" + releaseId))
                {
                    w.SetRequestHeader("User-Agent", "Mozilla/5.0");
                    yield return w.SendWebRequest();
                    EditorUtility.DisplayProgressBar("Release info", $"Fetching release information {releaseId}", 0f);
                    while (w.isDone == false)
                    {
                        yield return null;
                        EditorUtility.DisplayProgressBar("Downloading", $"Fetching release information {releaseId}", w.downloadProgress);
                    }
                    row = w.downloadHandler.text;
                    if (w.isHttpError)
                    {
                        error(row);
                        yield break;
                    }
                }
                var parsed = JsonUtility.FromJson<ReleaseResponse>(row);
                done(parsed);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
        private static void RenderPack(BuildPack pack)
        {
            EditorGUI.indentLevel++;
            switch (pack.Status)
            {
                case BuildPackStatus.NotLoaded:
                    BackgroundTaskInvoker.StartBackgroundTask(DownloadReleaseInfo(pack.Id, p =>
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
                        File.Delete(pack.Location);
                        FindBuildPacks();
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
