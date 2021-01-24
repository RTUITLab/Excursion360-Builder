using System;
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

        private string ProjectFolder =>
            Path.GetFullPath("Packages/com.rexagon.tour-creator/.LiveViewer/Excursion360-Builder");
        private string OutputFolder =>
            Path.GetFullPath($"{ProjectFolder}/output");
        private void OnEnable()
        {
            isDotNetInstalled = DotnetHelpers.CheckDotNetInstalled();
        }

        private void OnGUI()
        {
            if (!isDotNetInstalled)
            {
                DrawInstallDotNetMessage();
                return;
            }
            var buildFolder = OutputFolder;
            if (!File.Exists(GetExecutablePath()))
            {
                DrawBuildPreviewBackend();
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
                case  RuntimePlatform.WindowsEditor:
                    return $"{OutputFolder}/Web.exe";
                default: throw new Exception($"Platform {Application.platform} is not supported yet");
            }
        }
    }
}