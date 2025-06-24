using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Packages.Excursion360_Builder.Editor.WebBuild.RemoteItems;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Packages.Excursion360_Builder.Editor.LivePreview
{
    internal class DotnetHelpers
    {
        public static bool CheckDotNetInstalled()
        {
            try
            {
                var proc = Process.Start(new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "--list-sdks",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                proc.WaitForExit();
                var sdks = proc.StandardOutput.ReadToEnd();
                var versions = sdks.Split('\n')
                    .Select(v => Regex.Match(v, @"^((?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+))"))
                    .Where(m => m.Success)
                    .Select(m => new
                    {
                        Major = m.Groups["major"].Value,
                        Minor = m.Groups["minor"].Value,
                        Patch = m.Groups["patch"].Value
                    })
                    .ToList();
                return versions.Any(v => v.Major == "9");
            }
            catch (Exception ex) when (ex is Win32Exception || ex is FileNotFoundException)
            {
                Debug.LogError("not found dotnet");
                return false;
            }
        }
        public static void BuildLivePreviewBackend(
            string projectFolder,
            string outputFolder,
            WebViewerBuildPack selectedBuildPack)
        {
            try
            {
                var systemRuntime = GetSystemRuntime();
                var arguments = $"publish {projectFolder}/Web/Web.csproj " +
                                $"-r {systemRuntime} " +
                                $"-o {outputFolder} " +
                                $"-c Release " +
                                $"--self-contained true " +
                                $"-p:PublishSingleFile=true " +
                                $"-p:DebugType=None " +
                                $"-p:DebugSymbols=False" +
                                $"";
                var proc = Process.Start(new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                });
                proc.WaitForExit();
                if (proc.ExitCode != 0)
                {
                    throw new Exception($"Can't build live preview {proc.StandardOutput.ReadToEnd()}");
                }
                TourExporter.UnpackViewer(selectedBuildPack, Path.Combine(outputFolder, "wwwroot"));
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                throw;
            }
        }
        private static string GetSystemRuntime()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    return Environment.Is64BitOperatingSystem ? "win-x64" : "win-x86";
                case RuntimePlatform.LinuxEditor:
                    return "linux-x64";
                case RuntimePlatform.OSXEditor:
                    return "osx-x64";
                default: throw new Exception("Unsupported platform");
            }
        }
    }
}