using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using Packages.tour_creator.Editor.WebBuild;
using Excursion360_Builder.Shared.States.Items.Field;
using Packages.Excursion360_Builder.Editor.WebBuild;
using System.Text.RegularExpressions;
using Packages.Excursion360_Builder.Editor.WebBuild.RemoteItems;
using Packages.Excursion360_Builder.Editor;

#if UNITY_EDITOR
using UnityEditor;
using Exported = Packages.tour_creator.Editor.Protocol;

internal class TourExporter
{
    internal class GenerateTourOptions
    {
        public ResourceHandlePath ResourceHandlePath { get; }
        public int ImageCroppingLevel { get; }
        protected GenerateTourOptions(ResourceHandlePath resourceHandlePath, int imageCroppingLevel)
        {
            ResourceHandlePath = resourceHandlePath;
            ImageCroppingLevel = imageCroppingLevel;
        }
        public static GenerateTourOptions ForPreview()
        {
            return new GenerateTourOptions(ResourceHandlePath.PublishPath, 0);
        }
    }
    internal class ExportOptions : GenerateTourOptions
    {
        public string FolderPath { get; }
        public WebViewerBuildPack ViewerPack { get; }
        public DesktopClientBuildPack DesktopClientBuildPack { get; }

        public ExportOptions(
            WebViewerBuildPack viewerPack,
            DesktopClientBuildPack desktopClientBuildPack,
            string folderPath,
            int imageCroppingLevel)
            : base(ResourceHandlePath.CopyToDist, imageCroppingLevel)
        {
            FolderPath = folderPath;
            ViewerPack = viewerPack;
            DesktopClientBuildPack = desktopClientBuildPack;
        }
    }
    public static void ExportTour(ExportOptions exportOptions)
    {
        if (Tour.Instance == null)
        {
            EditorUtility.DisplayDialog("Error", "There is no tour object on this scene!", "Ok");
            return;
        }
        if (string.IsNullOrWhiteSpace(Tour.Instance.title))
        {
            EditorUtility.DisplayDialog("Error", "You must provide tour title", "Ok");
            return;
        }

        var excursionFolder = Path.Combine(exportOptions.FolderPath, Tour.Instance.title);
        Directory.CreateDirectory(excursionFolder);


        try
        {
            UnpackViewer(exportOptions.ViewerPack, excursionFolder);

            if (exportOptions.DesktopClientBuildPack != null)
            {
                UnpackDesktopClient(exportOptions.DesktopClientBuildPack, exportOptions.FolderPath);
            }

            if (!CopyLogo(excursionFolder, out var logoFileName))
            {
                return;
            }

            CreateConfigFile(excursionFolder, logoFileName);
            ProjectEditorPrefs.IncrementBuildNum();
            Exported.Tour tour = GenerateTour(excursionFolder, exportOptions);

            // Serialize and write
            File.WriteAllText(excursionFolder + "/tour.json", JsonUtility.ToJson(tour, true));
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("Error", $"Error while exporting tour\n{ex.Message}\n{ex.StackTrace}", "Ok");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
        // Finish
    }


    public static Exported.Tour GenerateTour(string folderPath, GenerateTourOptions generateTourOptions)
    {
        State firstState = Tour.Instance.firstState;
        if (firstState == null)
        {
            EditorUtility.DisplayDialog("Error", "First state is not selected!", "Ok");
            return null;
        }

        // Find all states
        State[] states = GameObject.FindObjectsOfType<State>();
        if (states.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "There is no states on this scene to export!", "Ok");
            return null;
        }

        Exported.Tour tour = PrepateTour(Tour.Instance);
        Debug.Log($"Found {states.Length} states");
        // Pre process states
        UpdateProcess(0, states.Length, "Exporting", "");

        for (int i = 0; i < states.Length; ++i)
        {
            var state = states[i];

            try
            {
                if (!TryHandleState(
                    state,
                    folderPath,
                    generateTourOptions.ResourceHandlePath,
                    generateTourOptions.ImageCroppingLevel,
                    out var exportedState))
                {
                    EditorUtility.DisplayDialog("Error", $"Error while exporting state {state.title}", "Ok");
                    return null;
                }

                tour.states.Add(exportedState);
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Error while exporting state {state.title}\n{ex.Message}", "Ok");
                return null;
            }


            UpdateProcess(i + 1, states.Length, "Exporting", $"{i + 1}/{states.Length}: {state.title}");
        }
        PatchViewer(folderPath, tour);
        return tour;
    }
    /// <summary>
    /// Apply tour title and hash
    /// </summary>
    /// <param name="folderPath">Folder of viewer with target files and viewer</param>
    /// <param name="title">Title of tour</param>
    private static void PatchViewer(string folderPath, Exported.Tour tour)
    {
        var indexFileName = "index.html";
        var titleTemplate = "%TITLE_TEMPLATE%";

        var indexFile = Path.Combine(folderPath, indexFileName);
        if (!File.Exists(indexFile))
        {
            Debug.LogWarning($"Can't find {indexFileName} at {folderPath} for patching");
            return;
        }
        var indexFileContent = File.ReadAllText(indexFile);
        if (indexFileContent.Contains(titleTemplate))
        {
            indexFileContent = indexFileContent.Replace(titleTemplate, tour.title);
        }
        else
        {
            Debug.LogWarning($"Can't find title template {titleTemplate} in index file. Please, use latest viewer.");
        }
        PatchJsFile(folderPath, $"{tour.id}-{tour.versionNum}", ref indexFileContent);
        File.WriteAllText(indexFile, indexFileContent);
    }

    private static void PatchJsFile(string folderPath, string tourHash, ref string indexFileContent)
    {
        var jsLinkedFiles = Directory.GetFiles(folderPath, "client.js*");
        var renamePatterns = jsLinkedFiles
            .Select(path => (path, lineEnd: Regex.Match(path, @"client.js(?<name>.+|)$").Groups["name"].Value))
            .Select(pair => (source: pair.path, target: Path.Combine(Path.GetDirectoryName(pair.path), $"{tourHash}.js{pair.lineEnd}")))
            .ToArray();
        foreach (var (source, target) in renamePatterns)
        {
            File.Move(source, target);
        }
        indexFileContent = indexFileContent.Replace("client.js", $"{tourHash}.js");
    }

    private static Exported.Tour PrepateTour(Tour tour)
    {
        EditorUtility.SetDirty(tour);
        return new Exported.Tour
        {
            id = ProjectEditorPrefs.ProjectId,
            title = tour.title,
            BuildTime = DateTimeOffset.Now,
            versionNum = ProjectEditorPrefs.BuildNum,
            tourProtocolVersion = "v0.9",
            firstStateId = tour.firstState.GetExportedId(),
            colorSchemes = tour.colorSchemes.Select(cs => cs.color).ToArray(),
            states = new List<Exported.State>(),
        };
    }

    private static bool TryHandleState(
        State state,
        string folderPath,
        ResourceHandlePath resourceHandlePath,
        int imageCroppingLevel,
        out Exported.State exportedState)
    {
        exportedState = default;
        TextureSource textureSource = state.GetComponent<TextureSource>();
        if (textureSource == null)
        {
            EditorUtility.DisplayDialog("Error", "State has no texture source!", "Ok");
            return false;
        }

        var stateId = state.GetExportedId();

        var textureFileLocation = textureSource.GetAssetPath();
        string url = null;
        string croppedImageUrl = null;
        switch (resourceHandlePath)
        {
            case ResourceHandlePath.CopyToDist:
                var stateFolderPath = Path.Combine(folderPath, stateId);
                ImageCropper.HandleImage(textureFileLocation, stateFolderPath, imageCroppingLevel);
                croppedImageUrl = stateId;
                break;
            case ResourceHandlePath.PublishPath:
                url = textureFileLocation;
                if (url.StartsWith("Packages"))
                {
                    EditorUtility.DisplayDialog("Error",
                        "You can't use texture from Packages while preview. Please use textures only from Assets",
                        "Ok");
                    return false;
                }

                break;
            default:
                throw new Exception($"incorrect ResourceHandlePath {resourceHandlePath}");
        }

        exportedState = new Exported.State
        {
            id = stateId,
            title = state.title,
            url = url,
            croppedImageUrl = croppedImageUrl,
            type = textureSource.SourceType.ToString().ToLower(),
            pictureRotation = state.transform.rotation,
            links = GetLinks(state),
            groupLinks = GetGroupLinks(state),
            fieldItems = GetFieldItems(state, folderPath, resourceHandlePath)
        };
        return true;
    }

    private static List<Exported.FieldItem> GetFieldItems(State state, string folderPath,
        ResourceHandlePath resourceHandlePath)
    {
        var fieldItems = new List<Exported.FieldItem>();

        var unityFieldConnections = state.GetComponents<FieldItem>();

        foreach (var fieldItem in unityFieldConnections)
        {
            fieldItems.Add(new Exported.FieldItem
            {
                title = fieldItem.title,
                vertices = fieldItem.vertices.Select(v => v.Orientation).ToArray(),
                images = fieldItem.images.Select((texture, i) => ExportResource(
                        texture,
                        folderPath,
                        $"{fieldItem.GetExportedId()}_{i}",
                        resourceHandlePath))
                    .ToArray(),
                videos = fieldItem.videos.Select((video, i) => ExportResource(
                        video,
                        folderPath,
                        $"{fieldItem.GetExportedId()}_{i}",
                        resourceHandlePath))
                    .ToArray(),
                text = fieldItem.text,
                audios = fieldItem.audios.Select((audio, i) => new Exported.FieldItemAudioContent
                {
                    src = ExportResource(
                            audio,
                            folderPath,
                            $"{fieldItem.GetExportedId()}_{i}",
                            resourceHandlePath),
                    duration = audio.length
                })
                    .ToArray()
            });
        }

        return fieldItems;
    }

    private static string ExportResource(UnityEngine.Object resourceToExport, string destination, string fileName,
        ResourceHandlePath resourceHandlePath)
    {
        var path = AssetDatabase.GetAssetPath(resourceToExport);
        switch (resourceHandlePath)
        {
            case ResourceHandlePath.CopyToDist:
                var filename = fileName + Path.GetExtension(path);
                File.Copy(path, Path.Combine(destination, filename));
                return filename;
            case ResourceHandlePath.PublishPath:
                if (path.StartsWith("Packages"))
                {
                    throw new Exception(
                        $"You can't use texture from Packages while preview. Please use textures only from Assets");
                }

                return path;
            default:
                throw new Exception($"incorrect ResourceHandlePath {resourceHandlePath}");
        }
    }

    private static List<Exported.StateLink> GetLinks(State state)
    {
        var stateLinks = new List<Exported.StateLink>();
        var connections = state.GetComponents<Connection>();

        foreach (var connection in connections)
        {
            stateLinks.Add(new Exported.StateLink()
            {
                id = connection.Destination.GetExportedId(),
                rotation = connection.Orientation,
                colorScheme = connection.colorScheme,
                rotationAfterStepAngleOverridden = connection.rotationAfterStepAngleOverridden,
                rotationAfterStepAngle = connection.rotationAfterStepAngle
            });
        }

        return stateLinks;
    }

    private static List<Exported.GroupStateLink> GetGroupLinks(State state)
    {
        var stateLinks = new List<Exported.GroupStateLink>();
        var connections = state.GetComponents<GroupConnection>();
        if (connections == null || connections.Length == 0)
        {
            return stateLinks;
        }

        foreach (var connection in connections)
        {
            stateLinks.Add(new Exported.GroupStateLink()
            {
                title = connection.title,
                rotation = connection.Orientation,
                stateIds = connection.states.Select(s => s.GetExportedId()).ToList(),
                infos = connection.infos.ToList(),
                minimizeScale = connection.minimizeScale,
                groupStateRotationOverrides = connection
                    .rotationAfterStepAngles
                    .Select(p => new Exported.GroupStateLinkRotationOverride
                    {
                        stateId = p.state.GetExportedId(),
                        rotationAfterStepAngle = p.rotationAfterStepAngle
                    })
                    .ToList()
            });
        }

        return stateLinks;
    }

    private static void CreateConfigFile(string folderPath, string logoFileName)
    {
        var configuration = new Configuration
        {
            logoUrl = logoFileName,
            sceneUrl = ""
        };
        var stringConfig = JsonUtility.ToJson(configuration);
        File.WriteAllText(Path.Combine(folderPath, "config.json"), stringConfig);
    }

    private static bool CopyLogo(string folderPath, out string logoPath)
    {
        logoPath = "";
        var path = AssetDatabase.GetAssetPath(Tour.Instance.logoTexture);
        if (string.IsNullOrEmpty(path))
        {
            if (!EditorUtility.DisplayDialog("Warning",
                "There is no logo to navigate between locations. Are you sure you want to leave the default logo?",
                "Yes, continue", "Cancel"))
            {
                EditorUtility.DisplayDialog("Cancel", "Operation cancelled", "Ok");
                return false;
            }

            logoPath = "";
            return true;
        }

        var filename = $"logo_{Tour.Instance.logoTexture.GetInstanceID()}{Path.GetExtension(path)}";
        Debug.Log(Path.Combine(folderPath, filename));
        File.Copy(path, Path.Combine(folderPath, filename));
        logoPath = filename;
        return true;
    }

    public static void UnpackViewer(WebViewerBuildPack viewer, string folderPath)
    {
        using (var fileStream = File.OpenRead(viewer.ArchiveLocation))
        using (var zipInputStream = new ZipInputStream(fileStream))
        {
            while (zipInputStream.GetNextEntry() is ZipEntry zipEntry)
            {
                var entryFileName = zipEntry.Name;
                var buffer = new byte[4096];

                var fullZipToPath = Path.Combine(folderPath, entryFileName);
                var directoryName = Path.GetDirectoryName(fullZipToPath);
                if (!string.IsNullOrEmpty(directoryName))
                    Directory.CreateDirectory(directoryName);

                if (Path.GetFileName(fullZipToPath).Length == 0)
                {
                    continue;
                }

                using (FileStream streamWriter = File.Create(fullZipToPath))
                {
                    StreamUtils.Copy(zipInputStream, streamWriter, buffer);
                }
            }
        }
    }

    private static void UnpackDesktopClient(DesktopClientBuildPack desktopClientBuildPack, string folderPath)
    {
        foreach (var file in Directory
            .GetFiles(desktopClientBuildPack.FolderLocation)
            .Where(f => !f.EndsWith(".meta")))
        {
            File.Copy(file, Path.Combine(folderPath, Path.GetFileName(file)));
        }
    }


    public static bool TryGetTargetFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            EditorUtility.DisplayDialog("Error", "Selected directory does not exist", "Ок");
            return false;
        }

        if (!EditorUtility.DisplayDialog("Warning", "All files in the selected folder will be deleted, are you sure?",
            "Yes, delete", "Cancel"))
        {
            EditorUtility.DisplayDialog("Cancel", "Operation cancelled", "Ok");
            return false;
        }

        var files = Directory.GetFiles(folderPath);
        for (int i = 0; i < files.Length; i++)
        {
            var filePath = files[i];
            UpdateProcess(i, files.Length, "Deleting old files", filePath);
            try
            {
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Can't delete file {filePath}\n{ex.Message}\n{ex.StackTrace}",
                    "Ok");
                return false;
            }
        }
        var dirs = Directory.GetDirectories(folderPath);
        for (int i = 0; i < dirs.Length; i++)
        {
            var dirPath = dirs[i];
            UpdateProcess(i, files.Length, "Deleting old directoriy", dirPath);
            try
            {
                Directory.Delete(dirPath, true);
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Can't delete directory {dirPath}\n{ex.Message}\n{ex.StackTrace}",
                    "Ok");
                return false;
            }
        }

        return true;
    }

    static void UpdateProcess(int current, int target, string title, string message)
    {
        EditorUtility.DisplayProgressBar(title, message, current / (float)target);
    }
}

#endif