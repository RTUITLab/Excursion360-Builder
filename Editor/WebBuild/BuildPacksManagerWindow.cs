using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Packages.Excursion360_Builder.Editor.Viewer;
using Packages.tour_creator.Editor.WebBuild;
using Packages.tour_creator.Editor.WebBuild.GitHubAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Packages.Excursion360_Builder.Editor.WebBuild
{
    class BuildPacksManagerWindow : EditorWindow
    {
        private string outFolderPath;

        private void OnGUI()
        {
            var selectedViewer = ViewerBuildsGUI.Draw();
            if (selectedViewer == null)
            {
                return;
            }
            EditorGUILayout.Space();
            DrawExportingSection(selectedViewer);
        }

        private void DrawExportingSection(BuildPack selectedViewer)
        {
            EditorGUILayout.LabelField("Exporting excursion", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Target path");
            outFolderPath = EditorGUILayout.TextField(outFolderPath);
            if (GUILayout.Button("..."))
            {
                outFolderPath = EditorUtility.OpenFolderPanel("Select folder to export", outFolderPath, "");
                Repaint();
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Export"))
            {
                if (!TourExporter.TryGetTargetFolder(outFolderPath))
                {
                    return;
                }
                TourExporter.ExportTour(selectedViewer, outFolderPath);
            }
        }
    }
}
