using Packages.Excursion360_Builder.Editor.Viewer;
using Packages.Excursion360_Builder.Editor.WebBuild.RemoteItems;
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
    public class LivePreviewWindow : EditorWindow
    {
        private bool isDotNetInstalled;
        private Process previewBackendProcess;
        private WebViewerBuildPack selectedBuildPack;
        private string ProjectFolder =>
            Path.GetFullPath("Packages/com.rexagon.tour-creator/.LiveViewer/Excursion360-Builder");
        private string ProjectVersionFile =>
            Path.GetFullPath("Packages/com.rexagon.tour-creator/.LiveViewer/Excursion360-Builder/Web/version.json");

        private string OutputFolder =>
            Path.GetFullPath($"{ProjectFolder}/output");

        private string OutputVersionFile =>
            Path.GetFullPath($"{ProjectFolder}/output/version.json");

        public void OpenState(State state)
        {
            try
            {

                if (previewBackendProcess == null || previewBackendProcess.HasExited)
                {
                    EditorUtility.DisplayDialog("Info", "Please, start preview viewer", "Ok");
                    Focus();
                    return;
                }
                var resourceFolder = Path.Combine(OutputFolder, "wwwroot");
                Directory.CreateDirectory(resourceFolder);
                var tour = TourExporter.GenerateTour(TourExporter.GenerateTourOptions.ForPreview(resourceFolder));
                if (tour == null)
                {
                    EditorUtility.DisplayDialog("Error", "Can't create tour", "Ok");
                    return;
                }
                tour.firstStateId = state.GetExportedId();
                BackgroundTaskInvoker.StartBackgroundTask(LivePreviewProcessHelper.SendCameraRotation(SceneView.lastActiveSceneView.rotation));
                BackgroundTaskInvoker.StartBackgroundTask(LivePreviewProcessHelper.OpenTour(tour));
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void OnDestroy()
        {
            if (previewBackendProcess != null && !previewBackendProcess.HasExited)
            {
                previewBackendProcess.Kill();
            }
            if (!File.Exists(OutputVersionFile))
            {
                Directory.Delete(OutputFolder, true);
            }
            if (File.ReadAllText(OutputVersionFile) != File.ReadAllText(ProjectVersionFile))
            {
                Directory.Delete(OutputFolder, true);
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
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Stop"))
                {
                    previewBackendProcess.Kill();
                    previewBackendProcess = null;
                }
                if (GUILayout.Button("Open Preview page"))
                {
                    Application.OpenURL($"http://localhost:5000/index.html");
                }
                EditorGUILayout.EndHorizontal();
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