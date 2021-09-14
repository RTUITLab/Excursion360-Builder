using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Packages.Excursion360_Builder.Editor.SpellCheck
{
    internal static class SpellCheckCache
    {
        private static readonly object lockObject = new object();

        private static string pathToCacheFile;
        private static ConcurrentDictionary<string, RowResponse[]> cacheDictionary = new ConcurrentDictionary<string, RowResponse[]>();
        private static ConcurrentDictionary<DateTime, int> apiUsageHistory = new ConcurrentDictionary<DateTime, int>();
        private static ConcurrentDictionary<string, object> exceptions = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// queue for checing. Key - unique kewy for each editor key, to replace entered text to same key
        /// </summary>
        private static ConcurrentDictionary<string, (string raw, Action notifyCallback)> checkingQueue = new ConcurrentDictionary<string, (string raw, Action notifyCallback)>();


        public static int StatsToday => apiUsageHistory.TryGetValue(DateTime.Now.Date, out var stats) ? stats : 0;

        private static DateTime lastUpdateTime;

        private static TimeSpan updateTime = TimeSpan.FromSeconds(2);

        static SpellCheckCache()
        {
            var pathToSpellCheckFolder = Path.Combine(Application.dataPath, "Tour creator", "spellCheck");
            if (!Directory.Exists(pathToSpellCheckFolder))
            {
                Directory.CreateDirectory(pathToSpellCheckFolder);
            }
            pathToCacheFile = Path.Combine(pathToSpellCheckFolder, "db.json");
            if (File.Exists(pathToCacheFile))
            {
                LoadDataFromFile();
            }
            else
            {
                File.WriteAllText(pathToCacheFile, "{}");
            }
            EditorApplication.update += TimerTick;
        }

        private static void TimerTick()
        {
            if (DateTime.Now - lastUpdateTime > updateTime)
            {
                lastUpdateTime = DateTime.Now;
                UpdateCache();
            }
        }

        private static void UpdateCache()
        {
            foreach (var queueKey in checkingQueue.Keys)
            {
                if (!checkingQueue.TryGetValue(queueKey, out var queuedTask))
                {
                    continue;
                }
                BackgroundTaskInvoker.StartBackgroundTask(InternalHandleRow(queuedTask.raw, () =>
                {
                    checkingQueue.TryRemove(queueKey, out _);
                    queuedTask.notifyCallback();
                }));
            }
        }

        public static RowResponse[] QueueChecking(string key, string row, Action notifyCallback)
        {
            if (string.IsNullOrWhiteSpace(row))
            {
                row = string.Empty;
            }
            if (cacheDictionary.TryGetValue(row, out var response))
            {
                return response.Where(r => !exceptions.ContainsKey(r.word)).ToArray();
            }
            
            checkingQueue.AddOrUpdate(key, (row, notifyCallback), (k, o) => (row, notifyCallback));
            return null;

        }


        public static string[] GetExceptions()
        {
            return exceptions.Keys.OrderBy(k => k).ToArray();
        }

        public static void AddToExceptions(string exception)
        {
            exceptions.TryAdd(exception, null);
            SaveDataToFile();
        }


        public static void RemoveFromExceptions(string exception)
        {
            exceptions.TryRemove(exception, out _);
            SaveDataToFile();
        }

        private static IEnumerator InternalHandleRow(string row, Action notifyAction)
        {
            if (cacheDictionary.TryGetValue(row, out var response))
            {
                notifyAction();
                yield break;
            }
            var spellCheckRequest = YandexSpellCheckApi.GetResultForRow(row, results =>
            {
                IncrementApiUsage();
                cacheDictionary.AddOrUpdate(row, results, (key, old) => results);
                SaveDataToFile();
                notifyAction();
            });
            while (spellCheckRequest.MoveNext())
            {
                yield return null;
            }
        }

        private static void IncrementApiUsage()
        {
            apiUsageHistory.AddOrUpdate(DateTimeOffset.Now.Date, 1, (d, old) => old + 1);
        }

        private static void LoadDataFromFile()
        {
            lock (lockObject)
            {
                try
                {
                    var fileContent = File.ReadAllText(pathToCacheFile);
                    var parsed = JsonUtility.FromJson<SpellCheckCacheFileModel>(fileContent);
                    if (parsed.spellCheckPairs != null)
                    {
                        cacheDictionary = new ConcurrentDictionary<string, RowResponse[]>(parsed.spellCheckPairs.ToDictionary(p => p.word, p => p.result));
                    }
                    if (parsed.apiUsageHistory != null)
                    {
                        apiUsageHistory = new ConcurrentDictionary<DateTime, int>(parsed.apiUsageHistory.ToDictionary(p => DateTime.Parse(p.date), p => (p.api)));
                    }
                    if (parsed.exceptions != null)
                    {
                        exceptions = new ConcurrentDictionary<string, object>(parsed.exceptions.ToDictionary(r => r, r => (object)null));
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Can't read spellcheck file");
                    Debug.LogError(ex);
                }
            }
        }

        private static void SaveDataToFile()
        {
            lock (lockObject)
            {
                var jsonModel = new SpellCheckCacheFileModel
                {
                    spellCheckPairs = cacheDictionary
                        .Select(kvp => new SpellCheckPair { word = kvp.Key, result = kvp.Value })
                        .ToArray(),
                    apiUsageHistory = apiUsageHistory
                        .Select(kvp => new ApiUsage { date = kvp.Key.ToString("O"), api = kvp.Value})
                        .ToArray(),
                    exceptions = exceptions.Keys.ToArray()
                };
                var jsonPresent = JsonUtility.ToJson(jsonModel);
                File.WriteAllText(pathToCacheFile, jsonPresent);
            }
        }
    }
}
