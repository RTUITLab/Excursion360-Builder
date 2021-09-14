using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Packages.Excursion360_Builder.Editor.HTTP
{
    internal static class HttpHelper
    {
        public static IEnumerator InvokeGetRequest(
            string url,
            string infoMessage,
            Action<DownloadHandler> done,
            Action<string> error,
            bool showStatus = true)
        {
            try
            {
                using (UnityWebRequest w = UnityWebRequest.Get(url))
                {
                    w.SetRequestHeader("User-Agent", "Mozilla/5.0");
                    yield return w.SendWebRequest();
                    if (showStatus)
                    {
                        EditorUtility.DisplayProgressBar("Downloading", infoMessage, 0f);
                    }
                    while (w.isDone == false)
                    {
                        if (showStatus)
                        {
                            EditorUtility.DisplayProgressBar("Downloading", infoMessage, w.downloadProgress);
                        }
                        yield return null;
                    }
                    if (w.isHttpError)
                    {
                        error(w.downloadHandler.text);
                        yield break;
                    }
                    if (w.isNetworkError)
                    {
                        error(w.error);
                        yield break;
                    }
                    done(w.downloadHandler);
                }
            }
            finally
            {
                if (showStatus)
                {
                    EditorUtility.ClearProgressBar();
                }
            }
        }
        public static IEnumerator InvokePostRequest(
            string url,
            string infoMessage,
            Dictionary<string, string> formDictionary,
            Action<DownloadHandler> done,
            Action<string> error,
            bool showStatus = true)
        {
            try
            {
                var form = new WWWForm();
                foreach (var kvp in formDictionary)
                {
                    form.AddField(kvp.Key, kvp.Value);
                }
                using (UnityWebRequest w = UnityWebRequest.Post(url, form))
                {
                    w.SetRequestHeader("User-Agent", "Mozilla/5.0");
                    yield return w.SendWebRequest();
                    if (showStatus)
                    {
                        EditorUtility.DisplayProgressBar("Downloading", infoMessage, 0f);
                    }
                    while (w.isDone == false)
                    {
                        if (showStatus)
                        {
                            EditorUtility.DisplayProgressBar("Downloading", infoMessage, w.downloadProgress);
                        }
                        yield return null;
                    }
                    if (w.isHttpError)
                    {
                        error(w.downloadHandler.text);
                        yield break;
                    }
                    if (w.isNetworkError)
                    {
                        error(w.error);
                        yield break;
                    }
                    done(w.downloadHandler);
                }
            }
            finally
            {
                if (showStatus)
                {
                    EditorUtility.ClearProgressBar();
                }
            }
        }
    }
}
