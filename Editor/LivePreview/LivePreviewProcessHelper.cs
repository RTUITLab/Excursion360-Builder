using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Exported = Packages.tour_creator.Editor.Protocol;
namespace Packages.Excursion360_Builder.Editor.LivePreview
{
    class LivePreviewProcessHelper
    {
        public static Process StartLivePreviewBackend(string executableFolder, string executablePath)
        {
            if (FindExistingProcess(executablePath, out var process))
            {
                return process;
            }
            process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = $"StartupOptions:AssetsPath={Application.dataPath} StartupOptions:ParentProcessId={Process.GetCurrentProcess().Id}",
                    WorkingDirectory = executableFolder,
                    RedirectStandardOutput = false,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            return process;
        }

        public static IEnumerator SendCameraRotation(Quaternion rotation)
        {
            using (UnityWebRequest request = new UnityWebRequest(
                "http://localhost:5000/api/interop/rotateCamera", "POST"
                ))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(rotation));
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                yield return request.SendWebRequest();
                var row = request.downloadHandler.text;
            }
        }

        public static IEnumerator OpenTour(Exported.Tour tour)
        {
            using (UnityWebRequest request = new UnityWebRequest(
                "http://localhost:5000/api/interop/openTour", "POST"
                ))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(tour));
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                yield return request.SendWebRequest();
                var row = request.downloadHandler.text;
            }
        }

        private static bool FindExistingProcess(string executablePath, out Process process)
        {
            var targetProcess = Process.GetProcesses()
                .Where(p => p.ProcessName.Contains("Web"))
                .Select(p =>
                {
                    try { return new { process = p, module = p.MainModule }; } catch { return null; }
                })
                .Where(m => m != null)
                .FirstOrDefault(m => m.module.FileName == executablePath);
            if (targetProcess != null)
            {
                process = targetProcess.process;
                return true;
            }
            else
            {
                process = null;
                return false;
            }
        }
    }
}
