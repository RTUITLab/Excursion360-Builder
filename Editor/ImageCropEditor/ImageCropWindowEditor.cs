using Codice.CM.WorkspaceServer.DataStore;
using NUnit.Framework.Internal.Filters;
using Packages.Excursion360_Builder.Editor.Protocol;
using Packages.tour_creator.Editor.WebBuild;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

namespace Packages.Excursion360_Builder.Editor.ImageCropEditor
{
    [Serializable]
    internal class ImageCropPositionMeta
    {
        public Vector2[] positions;
    }
    internal class ImageCropWindowEditor : EditorWindow
    {
        private Texture2D image;
        private string imagePath;

        private Vector2 imageContainerPosition;
        private Texture2D previewImage;
        private string previewImagePath;

        private Vector2 scrollPosition;
        private GUIStyle noMarginStyle;
        private Vector2[] positions = new Vector2[4];
        private int? currentVertexIndex;

        private bool showPreview;

        private void OnGUI()
        {
            HeaderLine();


            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true));
            noMarginStyle = new GUIStyle()
            {
                margin = new RectOffset(0, 0, 0, 0),
            };
            EditorGUILayout.BeginVertical(noMarginStyle);
            if (showPreview)
            {
                GUILayout.Box(previewImage, noMarginStyle, GUILayout.Width(800));
            }
            else
            {
                Editor();
            }
            EditorGUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void Editor()
        {

            if (image == null)
            {
                return;
            }
            GUILayout.Box(image, noMarginStyle);
            var imageRect = GUILayoutUtility.GetLastRect();

            for (int i = 0; i < positions.Length; i++)
            {
                var first = positions[i];
                var second = positions[(i + 1) % positions.Length];
                Handles.color = UnityEngine.Color.yellow;
                Handles.DrawLine(first, second);
            }
            Handles.DrawLine(positions[0], positions[2]);
            Handles.DrawLine(positions[1], positions[3]);


            for (int i = 0; i < positions.Length; i++)
            {
                var rect = DrawCurrentEditingBox(positions[i], i);

                if (Event.current.type == EventType.MouseDown)
                {
                    if (rect.Contains(Event.current.mousePosition))
                    {
                        currentVertexIndex = i;
                    }
                    else if (imageRect.Contains(Event.current.mousePosition))
                    {
                        currentVertexIndex = positions
                            .Select((p, i) => (dist: Vector2.Distance(p, Event.current.mousePosition), i: i))
                            .OrderBy(p => p.dist)
                            .First()
                            .i;
                    }
                }
            }
            if (Event.current.type == EventType.MouseUp)
            {
                currentVertexIndex = null;
            }
            if (Event.current.type == EventType.MouseDrag && currentVertexIndex.HasValue)
            {
                var position = Event.current.mousePosition;
                if (position != positions[currentVertexIndex.Value])
                {
                    Repaint();
                }
                positions[currentVertexIndex.Value] = position;
            }
        }

        private void HeaderLine()
        {
            EditorGUILayout.BeginHorizontal();
            var newImage = (Texture2D)EditorGUILayout.ObjectField("Image", image, typeof(Texture2D), false);

            var text = (showPreview ? "editor" : "preview") + " (A)";
            if (GUILayout.Button(text, GUILayout.ExpandHeight(true))
                || (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.A))
            {
                showPreview = !showPreview;
                if (showPreview)
                {
                    var importer = AssetImporter.GetAtPath(imagePath);
                    importer.userData = JsonUtility.ToJson(new ImageCropPositionMeta { positions = positions });
                    importer.SaveAndReimport();
                    var previewImagePath = PreparePreviewImage(image);
                    if (!previewImage)
                    {
                        previewImage = new Texture2D(0, 0);
                    }
                    previewImage.LoadImage(File.ReadAllBytes(previewImagePath));
                }
            }
            if (image)
            {
                if (GUILayout.Button("Next (D)", GUILayout.ExpandHeight(true))
                    || (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.D))
                {
                    newImage = NextImage(newImage);
                }
            }
            if (showPreview && previewImage)
            {
                if (GUILayout.Button("Save (S)", GUILayout.ExpandHeight(true))
                    || (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.S))
                {
                    var sourcePath = AssetDatabase.GetAssetPath(image);
                    var newFileName = Path.GetFileNameWithoutExtension(sourcePath) + "_deskew" + Path.GetExtension(sourcePath);
                    var destPath = Path.Combine(Path.GetDirectoryName(sourcePath), newFileName);
                    if (File.Exists(destPath))
                    {
                        File.Delete(destPath);
                    }
                    File.Move(previewImagePath, destPath);
                    AssetDatabase.Refresh();
                    newImage = NextImage(newImage);
                }

            }
            EditorGUILayout.EndHorizontal();

            if (newImage != image)
            {
                image = newImage;
                if (image)
                {
                    showPreview = false;
                    positions[0] = new Vector2(image.width * 0.2f, image.height * 0.2f);
                    positions[1] = new Vector2(image.width * 0.8f, image.height * 0.2f);
                    positions[2] = new Vector2(image.width * 0.8f, image.height * 0.8f);
                    positions[3] = new Vector2(image.width * 0.2f, image.height * 0.8f);

                    imagePath = AssetDatabase.GetAssetPath(image);

                    var importer = AssetImporter.GetAtPath(imagePath);
                    try
                    {
                        var data = JsonUtility.FromJson<ImageCropPositionMeta>(importer.userData);
                        if (data?.positions?.Length == 4)
                        {
                            positions = data.positions;
                        }
                    }
                    catch { }

                }
            }
        }

        private Texture2D NextImage(Texture2D newImage)
        {
            var nextFile = Directory
                .GetFiles(Path.GetDirectoryName(imagePath))
                .Select(f => f.Replace(@"\", "/"))
                .Where(f => !f.EndsWith(".meta"))
                .Where(f => !f.Contains("_deskew."))
                .SkipWhile(p => p != Path.Combine(imagePath))
                .Skip(1)
                .FirstOrDefault();
            if (nextFile is not null)
            {
                newImage = AssetDatabase.LoadAssetAtPath<Texture2D>(nextFile);
                Repaint();
                showPreview = false;
            }

            return newImage;
        }

        private string PreparePreviewImage(Texture2D texture)
        {
            var path = AssetDatabase.GetAssetPath(texture);

            using var rawImage = System.Drawing.Image.FromFile(path).FixOrientation();

            var imagePositions = positions
                .Select(p => new Vector2(p.x * rawImage.Width / image.width, p.y * rawImage.Height / image.height))
                .ToArray();

            return previewImagePath = ImageSharpTry.ProcessAsPerspective(path, imagePositions);
        }

        private Rect DrawCurrentEditingBox(Vector2 currentPosition, int vertexNum)
        {
            var rect = new Rect(currentPosition.x - 15, currentPosition.y - 15, 30, 30);
            GUI.Box(rect, $"{vertexNum}");
            return rect;
        }
    }
}
