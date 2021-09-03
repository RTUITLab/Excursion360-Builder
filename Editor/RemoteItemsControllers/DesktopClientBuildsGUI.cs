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

namespace Packages.Excursion360_Builder.Editor.RemoteItemsControllers
{
    internal static class DesktopClientBuildsGUI
    {
        private const string GITHUB_REPO = "Excursion360-Desktop";
        private static string packsLocation;
        private static List<DesktopClientBuildPack> builds = new List<DesktopClientBuildPack>();
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
        public static DesktopClientBuildPack Draw()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Download last desktop client"))
            {
                BackgroundTaskInvoker.StartBackgroundTask(DownloadDesktopClient(ReleaseType.OnlyStable));
            }
            if (GUILayout.Button("pre-release"))
            {
                BackgroundTaskInvoker.StartBackgroundTask(DownloadDesktopClient(ReleaseType.WithPreRelease));
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
                .Select(path => (path, match: Regex.Match(path, @"(?<tag>[^\\/]+)-(?<id>\d+)")))
                .Select(b => new DesktopClientBuildPack
                {
                    Id = int.Parse(b.match.Groups["id"].Value),
                    Version = b.match.Groups["tag"].Value,
                    FolderLocation = b.path
                })
                .ToList();


            buildPackVersions = builds.Select(p => p.Version).ToArray();
            selectedbuildTagNum = buildPackVersions
                .Select((tag, i) => (tag, i))
                .OrderBy(s => s.tag)
                .FirstOrDefault()
                .i;
        }
        
        private static IEnumerator DownloadDesktopClient(ReleaseType releaseType)
        {
            EditorUtility.DisplayProgressBar("Downloading", "Downloading latest desktop client", 0);
            try
            {
                ReleaseResponse parsed = null;
                string errorMessage = null;
                var downloadingTask = GitHubApi.GetLatestReleaseForRepo(
                    GITHUB_REPO,
                    releaseType,
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

                var exeAssets = parsed.assets.Where(a => a.name.EndsWith(".exe")).ToArray();
                if (exeAssets.Length == 0)
                {
                    EditorUtility.DisplayDialog("Error", "No needed asset in latest release", "Ok");
                    yield break;
                }

                var versionDirectory = Path.Combine(packsLocation, $"{parsed.tag_name}-{parsed.id}");
                Directory.CreateDirectory(versionDirectory);

                foreach (var exe in exeAssets)
                {
                    var downloadRequest = GitHubApi.InvokeGetRequest(exe.browser_download_url, $"Downloading {exe.name}",
                        handler =>
                        {
                            File.WriteAllBytes(Path.Combine(versionDirectory, exe.name), handler.data);
                        },
                        error =>
                        {
                            errorMessage = error;
                        });
                    while (downloadRequest.MoveNext())
                    {
                        yield return downloadRequest.Current;
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            EditorUtility.DisplayDialog("Error", errorMessage, "Ok");
                            yield break;
                        }
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                FindDesktopPacks();
            }
        }
        
        private static void RenderClientVersion(DesktopClientBuildPack pack)
        {
            EditorGUI.indentLevel++;
            switch (pack.Status)
            {
                case BuildPackStatus.NotLoaded:
                    BackgroundTaskInvoker.StartBackgroundTask(GitHubApi.GetReleaseForRep(GITHUB_REPO, pack.Id, p =>
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
                        Directory.Delete(pack.FolderLocation, true);
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
    }

}
