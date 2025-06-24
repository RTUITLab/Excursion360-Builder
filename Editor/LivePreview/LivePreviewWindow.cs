using Packages.Excursion360_Builder.Editor.Viewer;
using Packages.Excursion360_Builder.Editor.WebBuild.RemoteItems;
using Packages.tour_creator.Editor.WebBuild;
using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Packages.Excursion360_Builder.Editor.LivePreview
{
    public class LivePreviewWindow : EditorWindow
    {
        private bool isDotNetInstalled;
        private Process previewBackendProcess;
        private WebViewerBuildPack selectedBuildPack;

        private readonly ViewerBuildsGUI viewerBuildsGUI = new ViewerBuildsGUI();


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
                var tour = TourExporter.GenerateTour(resourceFolder, TourExporter.GenerateTourOptions.ForPreview());
                if (tour == null)
                {
                    EditorUtility.DisplayDialog("Error", "Can't create tour", "Ok");
                    return;
                }
                tour.firstStateId = state.GetExportedId();
                tour.fastReturnToFirstStateEnabled = false; // Нет необходимости в возврате на время тестирования
                BackgroundTaskInvoker.StartBackgroundTask(LivePreviewProcessHelper.OpenTour(tour, SceneView.lastActiveSceneView.rotation));
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
            viewerBuildsGUI.Initialize();
        }

        private void OnGUI()
        {
            if (previewBackendProcess != null)
            {
                GUILayout.Label(previewBackendProcess.HasExited ? $"exited {previewBackendProcess.ExitCode}" : $"running {(DateTime.Now - previewBackendProcess.StartTime):hh\\:mm\\:ss}");
            }

            if (previewBackendProcess != null && !previewBackendProcess.HasExited)
            {
                DrawRunnedProcess();
                return;
            }

            if (!isDotNetInstalled)
            {
                DrawInstallDotNetMessage();
                return;
            }
            viewerBuildsGUI.Draw();
            selectedBuildPack = viewerBuildsGUI.CurrentPack;

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
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Stop"))
            {
                previewBackendProcess.Kill();
            }
            if (GUILayout.Button("Open Preview page"))
            {
                Application.OpenURL($"http://localhost:5000/index.html");
            }
            EditorGUILayout.EndHorizontal();
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
            GUILayout.Label("Please, install .NET 9 SDK and restart Unity");
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