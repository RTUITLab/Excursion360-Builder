using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Packages.Excursion360_Builder.Editor.LivePreview
{
    public class LivePreviwWindow : EditorWindow
    {
        private bool isDotNetInstalled;
        private Process previewBackendProcess;

        private string ProjectFolder =>
            Path.GetFullPath("Packages/com.rexagon.tour-creator/.LiveViewer/Excursion360-Builder");

        private string OutputFolder =>
            Path.GetFullPath($"{ProjectFolder}/output");

        private List<string> logs = new List<string>();
        private ConcurrentQueue<Action> actionsToMainThread = new ConcurrentQueue<Action>();

        private void Awake()
        {
            EditorApplication.update += CustomUpdate;
        }

        private void OnDestroy()
        {
            EditorApplication.update -= CustomUpdate;
        }

        // TODO Check works
        private void CustomUpdate()
        {
            while (actionsToMainThread.TryDequeue(out var action))
            {
                action();
            }
        }

        private void OnEnable()
        {
            isDotNetInstalled = DotnetHelpers.CheckDotNetInstalled();
        }

        private void OnGUI()
        {
            if (previewBackendProcess != null)
            {
                DrawRunnedProcess();
                return;
            }

            if (!isDotNetInstalled)
            {
                DrawInstallDotNetMessage();
                return;
            }

            if (!File.Exists(GetExecutablePath()))
            {
                DrawBuildPreviewBackend();
                return;
            }

            DrawReadyToStart();
        }

        private void DrawReadyToStart()
        {
            GUILayout.Label("You can start live preview backend");
            if (GUILayout.Button("Start live preview backend"))
            {
                StartLivePreviewBackend();
            }

            lock (logs)
            {
                foreach (var log in logs)
                {
                    GUILayout.Label(log);
                }
            }
        }

        private void StartLivePreviewBackend()
        {
            logs.Clear();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = GetExecutablePath(),
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.OutputDataReceived += (sender, args) =>
            {
                lock (logs)
                {
                    logs.Add(args.Data);
                }
                actionsToMainThread.Enqueue(Repaint);
            };
            process.Start();
            process.BeginOutputReadLine();
            previewBackendProcess = process;
        }

        private void DrawRunnedProcess()
        {
            GUILayout.Label(previewBackendProcess.HasExited ? $"exited {previewBackendProcess.ExitCode}" : "running");
            if (!previewBackendProcess.HasExited)
            {
                if (GUILayout.Button("Stop"))
                {
                    previewBackendProcess.Kill();
                    previewBackendProcess = null;
                }
            }
            else
            {
                if (GUILayout.Button("Start again"))
                {
                    StartLivePreviewBackend();
                }
            }
            lock (logs)
            {
                foreach (var log in logs)
                {
                    GUILayout.Label(log);
                }
            }
        }

        private void DrawBuildPreviewBackend()
        {
            GUILayout.Label("Please, build live preview backend");
            if (GUILayout.Button("Build"))
            {
                DotnetHelpers.BuildLivePreviewBackend(ProjectFolder, OutputFolder);
            }
        }

        private void DrawInstallDotNetMessage()
        {
            GUILayout.Label("Please, install .NET 5 SDK and restart Unity");
            if (GUILayout.Button("Download page"))
            {
                Application.OpenURL("https://dotnet.microsoft.com/download/dotnet/5.0");
            }
        }

        private string GetExecutablePath()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    return $"{OutputFolder}/Web.exe";
                default: throw new Exception($"Platform {Application.platform} is not supported yet");
            }
        }
    }
}