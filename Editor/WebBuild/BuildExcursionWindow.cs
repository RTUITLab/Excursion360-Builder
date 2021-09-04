using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Packages.Excursion360_Builder.Editor.RemoteItemsControllers;
using Packages.Excursion360_Builder.Editor.Viewer;
using Packages.Excursion360_Builder.Editor.WebBuild.RemoteItems;
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
    class BuildExcursionWindow : EditorWindow
    {
        private string outFolderPath;
        private int imageCroppingLevel;

        private bool showAdditional;
        private bool showViewerSelecting;
        private bool showDesktopClientSelecting;

        private WebViewerBuildPack selectedViewer;
        private DesktopClientBuildPack selectedDesktopClient;

        private DesktopClientBuildsGUI desktopClientBuildsGUI = new DesktopClientBuildsGUI();
        private ViewerBuildsGUI viewerBuildsGUI = new ViewerBuildsGUI();

        private void OnEnable()
        {
            outFolderPath = ProjectEditorPrefs.BuildLocation;
            imageCroppingLevel = ProjectEditorPrefs.ImageCroppingLevel;
            desktopClientBuildsGUI.Initialize();
            viewerBuildsGUI.Initialize();
        }

        private void OnGUI()
        {
            DrawBuildGUI();
            DrawAdditionalData();
        }

        private void DrawAdditionalData()
        {
            showAdditional = EditorGUILayout.Foldout(showAdditional, "Additional info");
            if (showAdditional)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Project id", ProjectEditorPrefs.ProjectId);
                EditorGUILayout.LabelField("Build num", ProjectEditorPrefs.BuildNum.ToString());
                EditorGUI.indentLevel--;
            }

        }

        private void DrawBuildGUI()
        {
            selectedViewer = viewerBuildsGUI.CurrentPack;
            showViewerSelecting = EditorGUILayout.Foldout(showViewerSelecting, $"Viewer: {selectedViewer?.Version ?? "NOT SELECTED"}");
            if (showViewerSelecting)
            {
                EditorGUI.indentLevel++;
                viewerBuildsGUI.Draw();
                EditorGUI.indentLevel--;
            }
            selectedDesktopClient = desktopClientBuildsGUI.CurrentPack;
            showDesktopClientSelecting = EditorGUILayout.Foldout(showDesktopClientSelecting, $"Desktop client: {selectedDesktopClient?.Version ?? "NOT SELECTED"}");
            if (showDesktopClientSelecting)
            {
                EditorGUI.indentLevel++;
                desktopClientBuildsGUI.Draw();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Exporting excursion", EditorStyles.boldLabel);
            DrawExportingSection();

            EditorGUILayout.Space();
            DrawExportButton(selectedViewer, selectedDesktopClient);
        }

        private void DrawExportingSection()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Target path");
            outFolderPath = EditorGUILayout.TextField(outFolderPath);
            if (GUILayout.Button("..."))
            {
                var newOutFolderPath = EditorUtility.OpenFolderPanel("Select folder to export", outFolderPath, "");
                if (string.IsNullOrEmpty(newOutFolderPath) && !string.IsNullOrEmpty(outFolderPath))
                {
                    newOutFolderPath = outFolderPath;
                }
                if (newOutFolderPath != outFolderPath)
                {
                    outFolderPath = newOutFolderPath;
                    ProjectEditorPrefs.BuildLocation = outFolderPath;
                }

                Repaint();
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();
            var newImageCroppingLevel = EditorGUILayout.IntSlider("Image cropping level", imageCroppingLevel, ImageCropper.MIN_PARTS_COUNT, ImageCropper.MAX_PARTS_COUNT);
            if (newImageCroppingLevel != imageCroppingLevel)
            {
                imageCroppingLevel = newImageCroppingLevel;
                ProjectEditorPrefs.ImageCroppingLevel = imageCroppingLevel;
            }

        }

        private void DrawExportButton(WebViewerBuildPack selectedViewer, DesktopClientBuildPack selectedDesktopClient)
        {
            if (GUILayout.Button("Export"))
            {
                if (selectedViewer == null || selectedDesktopClient == null)
                {
                    EditorUtility.DisplayDialog("Error", $"Please, select viewer and desktop client versions", "Ok");
                    return;
                }
                if (!TourExporter.TryGetTargetFolder(outFolderPath))
                {
                    return;
                }
                TourExporter.ExportTour(new TourExporter.ExportOptions(selectedViewer, selectedDesktopClient, outFolderPath, imageCroppingLevel));
            }
        }
    }
}
