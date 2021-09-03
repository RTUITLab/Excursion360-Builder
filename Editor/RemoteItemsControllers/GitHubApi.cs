using Packages.tour_creator.Editor.WebBuild.GitHubAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Packages.Excursion360_Builder.Editor.RemoteItemsControllers
{
    internal class GitHubApi
    {
        public static IEnumerator GetLatestReleaseForRepo(
            string repo,
            ReleaseType releaseType,
            Action<ReleaseResponse> done,
            Action<string> error)
        {
            var releaseRequest = DownloadReleaseInfo(releaseType == ReleaseType.WithPreRelease ? "?per_page=1" : "/latest", repo, done, error);
            while (releaseRequest.MoveNext())
            {
                yield return releaseRequest.Current;
            }
        }

        public static IEnumerator GetReleaseForRep(
            string repo,
            int releaseId,
            Action<ReleaseResponse> done,
            Action<string> error)
        {
            var releaseRequest = DownloadReleaseInfo($"/{releaseId}", repo, done, error);
            while (releaseRequest.MoveNext())
            {
                yield return releaseRequest.Current;
            }
        }
        private static IEnumerator DownloadReleaseInfo(
                    string postfix,
                    string repo,
                    Action<ReleaseResponse> done,
                    Action<string> error)
        {
            try
            {
                var getRequest =
                InvokeGetRequest(
                    $"https://api.github.com/repos/RTUITLab/{repo}/releases{postfix}",
                    $"Fetching {repo} release information",
                    handler =>
                    {
                        var raw = handler.text;
                        if (raw.StartsWith("["))
                        {
                            raw = raw.Trim('[', ']');
                        }
                        var parsed = JsonUtility.FromJson<ReleaseResponse>(raw);
                        done(parsed);
                    },
                    error);
                while (getRequest.MoveNext())
                {
                    yield return getRequest.Current;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        public static IEnumerator InvokeGetRequest(string url, string infoMessage, Action<DownloadHandler> done, Action<string> error)
        {
            try
            {
                using (UnityWebRequest w = UnityWebRequest.Get(url))
                {
                    w.SetRequestHeader("User-Agent", "Mozilla/5.0");
                    yield return w.SendWebRequest();
                    EditorUtility.DisplayProgressBar("Downloading", infoMessage, 0f);
                    while (w.isDone == false)
                    {
                        yield return null;
                        EditorUtility.DisplayProgressBar("Downloading", infoMessage, w.downloadProgress);
                    }
                    if (w.isHttpError)
                    {
                        error(w.downloadHandler.text);
                        yield break;
                    }
                    done(w.downloadHandler);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
