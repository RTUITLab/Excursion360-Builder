using Packages.Excursion360_Builder.Editor.Viewer;
using Packages.Excursion360_Builder.Editor.WebBuild;
using Packages.tour_creator.Editor.WebBuild;
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
        private BuildPack selectedBuildPack;
        private string ProjectFolder =>
            Path.GetFullPath("Packages/com.rexagon.tour-creator/.LiveViewer/Excursion360-Builder");

        private string OutputFolder =>
            Path.GetFullPath($"{ProjectFolder}/output");

        public void OpenState(State state)
        {
            var resourceFolder = Path.Combine(OutputFolder, "wwwroot");
            Directory.CreateDirectory(resourceFolder);
            var tour = TourExporter.GenerateTour(resourceFolder, ResourceHandlePath.PublishPath);
            if (tour == null)
            {
                EditorUtility.DisplayDialog("Error", "Can't create tour", "Ok");
                return;
            }
            File.WriteAllText(Path.Combine(resourceFolder, "tour.json"), JsonUtility.ToJson(tour));
            if (previewBackendProcess != null && !previewBackendProcess.HasExited)
            {
                Debug.Log(Application.dataPath);
                Application.OpenURL($"http://localhost:5000/index.html#{state.GetExportedId()}");
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

            selectedBuildPack = ViewerBuildsGUI.Draw();

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
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Start live preview backend"))
            {
                previewBackendProcess = LivePreviewProcessHelper.StartLivePreviewBackend(
                    OutputFolder,
                    GetExecutablePath());
            }
            if (GUILayout.Button("Rebuild"))
            {
                DotnetHelpers.BuildLivePreviewBackend(ProjectFolder, OutputFolder, selectedBuildPack);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawRunnedProcess()
        {
            GUILayout.Label(previewBackendProcess.HasExited ? $"exited {previewBackendProcess.ExitCode}" : $"running {(DateTime.Now - previewBackendProcess.StartTime):hh\\:mm\\:ss}");

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
                previewBackendProcess = null;
            }
        }

        private void DrawBuildPreviewBackend()
        {
            GUILayout.Label("Please, build live preview backend");
            if (GUILayout.Button("Build"))
            {
                DotnetHelpers.BuildLivePreviewBackend(ProjectFolder, OutputFolder, selectedBuildPack);
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
                    return Path.Combine(OutputFolder, "Web.exe");
                default: throw new Exception($"Platform {Application.platform} is not supported yet");
            }
        }
    }
}