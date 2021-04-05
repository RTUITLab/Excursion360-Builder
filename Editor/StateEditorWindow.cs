using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Excursion360_Builder.Editor.States.Items;
using Packages.Excursion360_Builder.Editor.Extensions;
using Packages.Excursion360_Builder.Editor;
using Packages.Excursion360_Builder.Editor.States.Items;
using Packages.Excursion360_Builder.Editor.LivePreview;

#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.SceneManagement;

public class StateEditorWindow : EditorWindow
{
    private List<State> _selectedStates = new List<State>();

    private bool _connectionsEditMode = false;

    private TextureSourceEditor _textureSourceEditor = new TextureSourceEditor();

    private Vector2 _itemsScroll = Vector2.zero;

    private readonly GroupConnectionEditor groupConnectionEditor = new GroupConnectionEditor();
    private readonly FieldItemEditor fieldItemEditor = new FieldItemEditor();
    private readonly ConnectionsToStateEditor connectionsToStateEditor = new ConnectionsToStateEditor();

    private bool connectionsFromOpened = true;
    private bool connectionsToOpened = true;
    private bool groupConnectionsOpened = true;
    private bool fieldItemsOpened = true;

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        _selectedStates = _selectedStates.Where(s => (bool)(s)).ToList();
        // Draw pages
        if (_selectedStates.Count == 0 || _selectedStates.Count > 2)
        {
            DrawIdlePageGUI();
        }
        else if (_selectedStates.Count == 1)
        {
            DrawStateEditPageGUI();
        }
        else
        {
            DrawConnectionEditGUI();
        }

        // Force update scene on change (for savig)
        if (GUI.changed)
        {
            foreach (var selection in _selectedStates)
            {
                EditorUtility.SetDirty(selection);
                EditorSceneManager.MarkSceneDirty(selection.gameObject.scene);
            }
        }

        // Draw state creation button
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Create new state", GUILayout.Height(50)))
        {
            var newObject = PrefabUtility.InstantiatePrefab(TourEditor.StatePrefab.Value) as GameObject;
            SelectObject(newObject);
            Undo.RegisterCreatedObjectUndo(newObject, "Undo state creation");
        }

        EditorGUILayout.Space();

        // Force redraw all
        Repaint();
        SceneView.RepaintAll();
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        _selectedStates = GetSlectedStates();
    }

    private void DrawIdlePageGUI()
    {
        GUILayout.Label("Select one state for editing it", EditorStyles.boldLabel);
        GUILayout.Label("Select two states for editing connections", EditorStyles.boldLabel);
    }

    private void DrawStateEditPageGUI()
    {
        if (EditorApplication.isPlaying)
            return;

        var state = _selectedStates[0];

        if (_connectionsEditMode)
            TourEditor.StateGraphRenderer.targetState = state;

        // Draw title edit field
        GUILayout.Label("State title: ", EditorStyles.boldLabel);
        state.title = EditorGUILayout.TextField(state.title);
        EditorGUILayout.Space();

        // Draw panorama texture edit field
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Panorama: ", EditorStyles.boldLabel);

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Change texture source"))
            _textureSourceEditor.ShowContextMenu(state);

        EditorGUILayout.EndHorizontal();

        _textureSourceEditor.Draw(state);

        EditorGUILayout.Space();

        // Draw panorama preview

        var previewTexture = state.GetComponent<TextureSource>().LoadedTexture;
        if (previewTexture == null)
        {
            previewTexture = EditorGUIUtility.whiteTexture;
        }
        EditorGUI.DrawPreviewTexture(EditorGUILayout.GetControlRect(false, 150.0f), previewTexture, null, ScaleMode.ScaleToFit);


        EditorGUILayout.Space();

        GUILayout.Label("Actions: ", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();

        

        if (GUILayout.Button("Focus camera", GUILayout.Height(50)))
        {
            FocusCamera(state.gameObject);
            SelectObject(state.gameObject);
        }

        if (GUILayout.Button("Open preview", GUILayout.Height(50)))
        {
            var window = EditorWindow.GetWindow<LivePreviwWindow>("Live preview");
            window.OpenState(state);
        }


        GUILayout.EndHorizontal();



        if (GUILayout.Toggle(_connectionsEditMode, "Edit mode", "Button", GUILayout.Height(50)) != _connectionsEditMode)
        {
            if (_connectionsEditMode)
            {
                TourEditor.StateGraphRenderer.targetState = null;
            }
            else
            {
                TourEditor.StateGraphRenderer.targetState = state;
            }

            _connectionsEditMode = !_connectionsEditMode;
        }

        EditorGUILayout.Space();

        _itemsScroll = EditorGUILayout.BeginScrollView(_itemsScroll);
        // Draw connections list
        connectionsFromOpened = EditorGUILayout.Foldout(connectionsFromOpened, "Connections from that:", true);
        if (connectionsFromOpened)
        {
            var connections = state.GetComponents<Connection>();
            EditorGUI.indentLevel++;
            foreach (var connection in connections)
            {

                var destinationTitle = connection.GetDestenationTitle() ?? "No destenation";
                EditorGUILayout.LabelField(destinationTitle, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                var serializedObject = new SerializedObject(connection);
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(connection.Destination)));
                serializedObject.ApplyModifiedProperties();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(EditorGUI.indentLevel * 20);
                if (connection.Destination)
                {
                    if (GUILayout.Button("move to"))
                    {
                        FocusCamera(connection.Destination.gameObject);
                        SelectObject(connection.Destination.gameObject);
                    }
                    var toggled = StateItemPlaceEditor.EditableItem == (object)connection;
                    if (GUILayout.Toggle(toggled, "edit", "Button") != toggled)
                    {
                        if (StateItemPlaceEditor.EditableItem == (object)connection)
                        {
                            StateItemPlaceEditor.CleadEditing();
                        }
                        else
                        {
                            StateItemPlaceEditor.EnableEditing(state, connection, Color.green);
                        }
                    }
                }
                if (Buttons.Delete())
                {
                    Undo.DestroyObjectImmediate(connection);
                }
                EditorGUILayout.EndHorizontal();

                var schemes = Tour.Instance.colorSchemes;
                var schemeNames = schemes.Select(s => s.name).ToArray();
                connection.colorScheme = EditorGUILayout.Popup(new GUIContent("Color scheme"), connection.colorScheme, schemeNames.Select(sn => new GUIContent(sn)).ToArray());
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
            EditorGUI.indentLevel--;
        }

        connectionsToOpened = EditorGUILayout.Foldout(connectionsToOpened, "Connections to that:", true);
        if (connectionsToOpened)
        {
            connectionsToStateEditor.Draw(state);
        }

        groupConnectionsOpened = EditorGUILayout.Foldout(groupConnectionsOpened, "Group connections:", true);
        if (groupConnectionsOpened)
        {
            groupConnectionEditor.Draw(state);
        }

        fieldItemsOpened = EditorGUILayout.Foldout(fieldItemsOpened, "Field items", true);
        if (fieldItemsOpened)
        {
            fieldItemEditor.Draw(state);
        }
        EditorGUILayout.EndScrollView();

        // TODO Content draw
        //itemsOpened = EditorGUILayout.Foldout(itemsOpened, "Items: ", true);
        //if (itemsOpened)
        //{
        //    connectionsOpened = false;
        //    _connectionsEditMode = false;
        //    TourEditor.StateGraphRenderer.targetState = null;
        //    itemsEditor.Draw(state);
        //}

        EditorGUILayout.Space();

        GUILayout.FlexibleSpace();
    }

    void DrawConnectionEditGUI()
    {
        GUILayout.FlexibleSpace();

        foreach (var item in _selectedStates)
        {
            GUILayout.Label(item.title);
        }

        if (GUILayout.Button("Toggle connection", GUILayout.Height(50)))
        {
            CreateConnection(_selectedStates[0], _selectedStates[1]);
        }
    }

    public static Vector3 ReflectDirection(Vector3 inDirection, Vector3 normal)
    {
        var length = inDirection.magnitude;
        inDirection.Normalize();
        normal.Normalize();

        return (2 * (Vector3.Dot(inDirection, normal) * normal) - inDirection).normalized * -length;
    }

    public void OnInspectorUpdate()
    {
        Repaint();
    }

    public static void FocusCamera(GameObject obj)
    {
        var sceneView = SceneView.lastActiveSceneView;

        var offset = sceneView.pivot - sceneView.camera.transform.position;
        var cameraDistance = offset.magnitude;
        sceneView.pivot = obj.transform.position + sceneView.camera.transform.forward * cameraDistance;
    }

    private void SelectObject(GameObject obj)
    {
        Selection.objects = new Object[] { obj };
    }

    private List<State> GetSlectedStates()
    {
        var result = new List<State>();

        foreach (var selection in Selection.gameObjects)
        {
            var state = selection.GetComponent<State>();
            if (state != null)
                result.Add(state);
        }

        return result;
    }

    private void CreateConnection(State firstState, State secondState)
    {
        var firstStateConnections = firstState.GetComponents<Connection>();
        var seconsStateConnections = secondState.GetComponents<Connection>();

        Connection connectionFirst = null;
        foreach (var connecton in firstStateConnections)
        {
            if (connecton.Destination == null)
                continue;

            if (connecton.Destination == secondState)
                connectionFirst = connecton;
        }

        Connection connectionSecond = null;
        foreach (var connecton in seconsStateConnections)
        {
            if (connecton.Destination == null)
                continue;

            if (connecton.Destination == firstState)
                connectionSecond = connecton;
        }

        if (connectionFirst == null && connectionSecond == null)
        {
            var directionToSecond = secondState.transform.position - firstState.transform.position;

            connectionFirst = Undo.AddComponent<Connection>(firstState.gameObject);
            connectionSecond = Undo.AddComponent<Connection>(secondState.gameObject);

            connectionFirst.Destination = secondState;
            connectionFirst.Orientation = Quaternion.FromToRotation(Vector3.forward, directionToSecond);

            connectionSecond.Destination = firstState;
            connectionSecond.Orientation = Quaternion.FromToRotation(Vector3.forward, -directionToSecond);
        }
        else
        {
            if (connectionFirst != null)
                Undo.DestroyObjectImmediate(connectionFirst);

            if (connectionSecond != null)
                Undo.DestroyObjectImmediate(connectionSecond);
        }
    }
}

#endif
