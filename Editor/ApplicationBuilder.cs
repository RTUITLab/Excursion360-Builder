using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;

public class ApplicationBuilder
{
    public enum BuildType
    {
        Desktop,
        Android
    }

    private const string BUILD_PATH_DESKTOP = "Builds/Desktop/tour.exe";
    private const string BUILD_PATH_ANDROID = "Builds/Android/tour.apk";

    public static void Build(BuildType buildType)
    {
        if (!EditorSceneManager.EnsureUntitledSceneHasBeenSaved("Scene must be saved before creating build"))
            return;

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
        {
            scenes = new[] { EditorSceneManager.GetActiveScene().path },
            options = BuildOptions.ShowBuiltPlayer
        };

        switch(buildType)
        {
            case BuildType.Desktop:
                PrepareDesktopBuild(ref buildPlayerOptions);
                break;

            case BuildType.Android:
                PrepareAndroidBuild(ref buildPlayerOptions);
                break;
        }

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.LogError("Build failed");
        }
    }

    static void PrepareDesktopBuild(ref BuildPlayerOptions options)
    {
        options.locationPathName = BUILD_PATH_DESKTOP;
        options.target = BuildTarget.StandaloneWindows;

        DisablePointersExcept<DesktopPointer>();
    }

    static void PrepareAndroidBuild(ref BuildPlayerOptions options)
    {
        options.locationPathName = BUILD_PATH_ANDROID;
        options.target = BuildTarget.Android;
    }

    static void DisablePointersExcept<T>()
    {
        Tour viewSphere = GameObject.FindObjectOfType<Tour>();
        foreach (Transform child in viewSphere.transform)
        {
            Pointer pointer = child.GetComponent<Pointer>();
            if (pointer == null)
                return;

            if (!(pointer is T))
                child.gameObject.SetActive(false);
        }
    }
}

#endif
