using Packages.Excursion360_Builder.Editor.WebBuild;
using Packages.Excursion360_Builder.Editor.WebBuild.RemoteItems;
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
    internal static class DesktopClientBuildsGUI
    {
        private static string packsLocation;
        private static List<WebViewerBuildPack> builds = new List<WebViewerBuildPack>();
        private static string[] buildPackVersions = Array.Empty<string>();
        private static int selectedbuildTagNum = 0;

        static DesktopClientBuildsGUI()
        {
            packsLocation = Path.Combine(Application.dataPath, "Tour creator", "desktop_clients");
            if (!Directory.Exists(packsLocation))
            {
                Directory.CreateDirectory(packsLocation);
            }
            FindDesktopPacks();
            selectedbuildTagNum = builds.Count - 1;
        }
        public static WebViewerBuildPack Draw()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Download last desctop client"))
            {
                BackgroundTaskInvoker.StartBackgroundTask(DownloadDesktopClient(packsLocation, ReleaseType.OnlyStable));
            }
            if (GUILayout.Button("pre-release"))
            {
                BackgroundTaskInvoker.StartBackgroundTask(DownloadDesktopClient(packsLocation, ReleaseType.WithPreRelease));
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Available clients", EditorStyles.boldLabel);
            if (GUILayout.Button("Refresh"))
            {
                FindDesktopPacks();
            }

            GUILayout.EndHorizontal();
            if (builds.Count == 0)
            {
                GUILayout.Label("No descktop clients", EditorStyles.label);
            }
            else
            {
                foreach (var pack in builds)
                {
                    pack.IsFolded = EditorGUILayout.Foldout(pack.IsFolded, pack.Version);
                    if (pack.IsFolded)
                    {
                        RenderClientVersion(pack);
                    }
                }
            }
            if (builds.Count > 0)
            {
                selectedbuildTagNum = EditorGUILayout.Popup("Selected version", selectedbuildTagNum, buildPackVersions);
                return builds[selectedbuildTagNum];
            }
            else
            {
                GUILayout.Label("Please, download desktop client", EditorStyles.label);
                return null;
            }
        }
        private static void FindDesktopPacks()
        {
            builds = Directory.GetDirectories(packsLocation)
                .Select(path => (path, match: Regex.Match(path, @"(?<tag>\S+)-(?<id>\d+)")))
                .Select(b => new WebViewerBuildPack
                {
                    Id = int.Parse(b.match.Groups["id"].Value),
                    Version = b.match.Groups["tag"].Value,
                    Location = b.path
                })
                .ToList();
            buildPackVersions = builds.Select(p => p.Version).ToArray();
            selectedbuildTagNum = buildPackVersions
                .Select((tag, i) => (tag, i))
                .OrderBy(s => s.tag)
                .FirstOrDefault()
                .i;
        }
        enum ReleaseType { OnlyStable, WithPreRelease }
        private static IEnumerator DownloadDesktopClient(string folderPath, ReleaseType releaseType)
        {
            EditorUtility.DisplayProgressBar("Downloading", "Downloading latest desktop client", 0);
            try
            {
                ReleaseResponse parsed = null;
                string errorMessage = null;
                var downloadingTask = DownloadReleaseInfo(
                    releaseType == ReleaseType.WithPreRelease ? "?per_page=1" : "/latest",
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
                FindDesktopPacks();
            }
        }
        private static IEnumerator DownloadReleaseInfo(int releaseId, Action<ReleaseResponse> done, Action<string> error)
           => DownloadReleaseInfo("/" + releaseId.ToString(), done, error);
        private static IEnumerator DownloadReleaseInfo(string releaseId,
            Action<ReleaseResponse> done,
            Action<string> error)
        {
            try
            {
                string raw;
                using (UnityWebRequest w = UnityWebRequest.Get("https://api.github.com/repos/RTUITLab/Excursion360-Desktop/releases" + releaseId))
                {
                    w.SetRequestHeader("User-Agent", "Mozilla/5.0");
                    yield return w.SendWebRequest();
                    EditorUtility.DisplayProgressBar("Release info", $"Fetching release information {releaseId}", 0f);
                    while (w.isDone == false)
                    {
                        yield return null;
                        EditorUtility.DisplayProgressBar("Downloading", $"Fetching release information {releaseId}", w.downloadProgress);
                    }
                    raw = w.downloadHandler.text;
                    if (w.isHttpError)
                    {
                        error(raw);
                        yield break;
                    }
                }
                try
                {
                    var parsed = JsonUtility.FromJson<ReleaseResponse>(raw);
                    done(parsed);
                }
                catch (ArgumentException)
                {
                    raw = $"{{\"items\": {raw}}}";// wrap array into object
                    var parsed = JsonUtility.FromJson<Wrapper<ReleaseResponse>>(raw).items[0];
                    done(parsed);
                }
                catch (Exception ex)
                {
                    error(ex.Message);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
        private static void RenderClientVersion(WebViewerBuildPack pack)
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
                        FindDesktopPacks();
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
        /// <summary>
        /// Used for deserialize JSON array
        /// </summary>
        /// <typeparam name="T">types of object in array</typeparam>
        [Serializable]
        private class Wrapper<T>
        {
            public T[] items;
        }
    }
    
}
