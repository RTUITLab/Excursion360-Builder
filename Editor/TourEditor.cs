using UnityEngine;
using UnityEngine.Assertions;
using Packages.tour_creator.Editor;
using Packages.Excursion360_Builder.Editor.WebBuild;
using Packages.Excursion360_Builder.Editor.SceneRenderers;
using System;
using Packages.Excursion360_Builder.Editor.SpellCheck;

#if UNITY_EDITOR
using Packages.Excursion360_Builder.Editor.LivePreview;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class TourEditor
{
    /// <summary>
    /// Main viewer prefab
    /// </summary>
    public static Lazy<GameObject> ViewSpherePrefab;

    /// <summary>
    /// Prefab used when spawning new state from State Editor window
    /// </summary>
    public static Lazy<GameObject> StatePrefab;

    /// <summary>
    /// State graph renderer object
    /// </summary>
    public static StateGraphRenderer StateGraphRenderer;

    public static ViewDirectionRenderer ViewDirectionRenderer;

    private const string GROUP_NAME = "Tour Creator";

    private const string MENU_ITEM_NEW_TOUR = GROUP_NAME + "/New Tour";
    private const string MENU_ITEM_STATE_EDITOR = GROUP_NAME + "/State Editor";
    private const string MENU_ITEM_LIVE_PREVIEW = GROUP_NAME + "/Live Preview";

    private const string MENU_ITEM_SHOW_CONNECTIONS = GROUP_NAME + "/Show Connections";
    private const string MENU_ITEM_SHOW_LABELS = GROUP_NAME + "/Show Labels";
    private const string MENU_ITEM_SHOW_ITEMS = GROUP_NAME + "/Show Items";

    private const string MENU_ITEM_BUILD_DESKTOP = GROUP_NAME + "/Build For Desktop (TODO)";
    private const string MENU_ITEM_BUILD_ANDROID = GROUP_NAME + "/Build For Android (TODO)";
    private const string MENU_ITEM_BUILD_WEB = GROUP_NAME + "/Build tour";
    
    
    private const string MENU_ITEM_SPELL_CHECK = GROUP_NAME + "/Spell check";

    private static bool _areConnectionsVisible;
    private static bool _areLabelsVisible;
    private static bool _areItemsVisible;

    static TourEditor()
    {
        Undo.undoRedoPerformed += () =>
        {
            var states = GameObject.FindObjectsOfType<State>();
            foreach (var state in states)
            {
                state.Reload();
            }
        };

        // Find view sphere prefab
        ViewSpherePrefab = new Lazy<GameObject>(
            () => 
            AssetDatabase.LoadAssetAtPath<GameObject>("Packages/com.rexagon.tour-creator/Prefabs/ViewSphere.prefab"));

        // Find state prefab
        StatePrefab = new Lazy<GameObject>(() =>
            AssetDatabase.LoadAssetAtPath<GameObject>("Packages/com.rexagon.tour-creator/Prefabs/State.prefab"));

        // Create renderer
        StateGraphRenderer = new StateGraphRenderer();
        ViewDirectionRenderer = new ViewDirectionRenderer();
        SceneView.duringSceneGui += StateGraphRenderer.RenderStateGraph;
        SceneView.duringSceneGui += ViewDirectionRenderer.Render;

        // Load settings
        _areConnectionsVisible = EditorPrefs.GetBool(MENU_ITEM_SHOW_CONNECTIONS, true);
        _areLabelsVisible = EditorPrefs.GetBool(MENU_ITEM_SHOW_LABELS, false);
        _areItemsVisible = EditorPrefs.GetBool(MENU_ITEM_SHOW_ITEMS, false);

        EditorApplication.delayCall += () =>
        {
            SetConnectionsVisible(_areConnectionsVisible);
            SetLabelsVisible(_areLabelsVisible);
            SetItemsVisible(_areItemsVisible);
            SceneView.RepaintAll();
        };
    }


    [MenuItem(MENU_ITEM_NEW_TOUR, false, 0)]
    static void MenuItemNewTour()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        RenderSettings.skybox = null;

        foreach (var gameObject in scene.GetRootGameObjects())
        {
            GameObject.DestroyImmediate(gameObject);
        }

        PrefabUtility.InstantiatePrefab(ViewSpherePrefab.Value);
    }

    [MenuItem(MENU_ITEM_STATE_EDITOR, false, 1)]
    static void MenuItemShowStateEditorWindow()
    {
        EditorWindow.GetWindow<StateEditorWindow>("State Editor");
    }
    
    [MenuItem(MENU_ITEM_LIVE_PREVIEW, false, 1)]
    static void MenuItemShowLivePreviewEditorWindow()
    {
        EditorWindow.GetWindow<LivePreviewWindow>("Live preview");
    }

    [MenuItem(MENU_ITEM_SHOW_CONNECTIONS, false, 20)]
    private static void MenuItemToggleShowConnections()
    {
        SetConnectionsVisible(!_areConnectionsVisible);
    }

    [MenuItem(MENU_ITEM_SHOW_LABELS, false, 21)]
    private static void MenuItemToggleShowLabels()
    {
        SetLabelsVisible(!_areLabelsVisible);
    }

    [MenuItem(MENU_ITEM_SHOW_ITEMS, false, 21)]
    private static void MenuItemToggleShowItems()
    {
        SetItemsVisible(!_areItemsVisible);
    }


    [MenuItem(MENU_ITEM_SPELL_CHECK, false, 30)]
    private static void MenuItemOpenSpellCheck()
    {
        EditorWindow.GetWindow<SpellCheckWindow>("Spell check");
    }

    //[MenuItem(MENU_ITEM_BUILD_DESKTOP, false, 40)]
    //private static void MenuItemBuildDesktop()
    //{
    //    EditorUtility.DisplayDialog("Not supported", "Desktop build not supported yet", "Ok");
    //    return;
    //    ApplicationBuilder.Build(ApplicationBuilder.BuildType.Desktop);
    //}

    //[MenuItem(MENU_ITEM_BUILD_ANDROID, false, 41)]
    //private static void MenuItemBuildAndroid()
    //{
    //    EditorUtility.DisplayDialog("Not supported", "Android build not supported yet", "Ok");
    //    return;
    //    ApplicationBuilder.Build(ApplicationBuilder.BuildType.Android);
    //}

    [MenuItem(MENU_ITEM_BUILD_WEB, false, 42)]
    private static void MenuItemBuildWeb()
    {
        EditorWindow.GetWindow<BuildExcursionWindow>("Build manager");
    }

    private static void SetConnectionsVisible(bool visible)
    {
        SetMenuItemEnabled(MENU_ITEM_SHOW_CONNECTIONS, visible);
        StateGraphRenderer.showConnections = visible;
        _areConnectionsVisible = visible;
        SceneView.RepaintAll();
    }

    private static void SetLabelsVisible(bool visible)
    {
        SetMenuItemEnabled(MENU_ITEM_SHOW_LABELS, visible);
        StateGraphRenderer.showLabels = visible;
        _areLabelsVisible = visible;
        SceneView.RepaintAll();
    }

    private static void SetItemsVisible(bool visible)
    {
        SetMenuItemEnabled(MENU_ITEM_SHOW_ITEMS, visible);
        StateGraphRenderer.showItems = visible;
        _areItemsVisible= visible;
        SceneView.RepaintAll();
    }

    private static void SetMenuItemEnabled(string menuItem, bool enabled)
    {
        Menu.SetChecked(menuItem, enabled);
        EditorPrefs.SetBool(menuItem, enabled);
    }
}

#endif
