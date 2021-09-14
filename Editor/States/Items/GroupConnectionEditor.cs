using Packages.Excursion360_Builder.Editor;
using Packages.Excursion360_Builder.Editor.SpellCheck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR

class GroupConnectionEditor : EditorBase
{

    public void Draw(State state, Action repaintAction)
    {
        if (GUILayout.Button("Add"))
        {
            Undo.AddComponent<GroupConnection>(state.gameObject);
        }
        EditorGUILayout.Space();
        var groupConnections = state.GetComponents<GroupConnection>();
        foreach (var groupConnection in groupConnections)
        {
            var groupConnectionTitle = GetTitleStringOf(groupConnection.title);
            GUILayout.Label(groupConnectionTitle, EditorStyles.boldLabel);

            EditorGUI.indentLevel++;

            Undo.RecordObject(groupConnection, "Edit group connection title");
            EditorGUILayout.BeginHorizontal();
            groupConnection.title = SpellCheckHintsContent.DrawTextField(
                $"{groupConnection.GetInstanceID()}_{nameof(groupConnection.title)}",
                "Title",
                groupConnection.title,
                repaintAction,
                n => { groupConnection.title = n; });
            if (Buttons.Delete())
            {
                Undo.DestroyObjectImmediate(groupConnection);
                repaintAction();
                return;
            }
            EditorGUILayout.EndHorizontal();
            var buttonStyle = Styles.ToggleButtonStyleNormal;
            if (StateItemPlaceEditor.EditableItem == (object)groupConnection)
                buttonStyle = Styles.ToggleButtonStyleToggled;

            var isEditable = StateItemPlaceEditor.EditableItem == (object)groupConnection;

            if (GUI.Toggle(EditorGUI.IndentedRect(EditorGUILayout.GetControlRect()), isEditable, "edit", "Button") != isEditable)
            {
                if (isEditable)
                {
                    StateItemPlaceEditor.CleadEditing();
                }
                else
                {
                    StateItemPlaceEditor.EnableEditing(state, groupConnection, Color.green);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();


            var connections = state.GetComponents<Connection>();

            if (connections.Length > 0)
            {
                if (GUI.Button(EditorGUI.IndentedRect(EditorGUILayout.GetControlRect()), $"Add connection"))
                {
                    GenericMenu menu = new GenericMenu();

                    foreach (var connection in connections)
                    {
                        menu.AddItem(new GUIContent(connection.Destination.title), false, o =>
                        {
                            var selectedConnection = o as Connection;
                            Undo.RecordObject(groupConnection, "Add state reference");
                            groupConnection.states.Add(selectedConnection.Destination);

                            Undo.DestroyObjectImmediate(selectedConnection);

                        }, connection);
                    }
                    menu.ShowAsContext();
                }
            }
            else
            {
                GUILayout.Label("No available connections to add");
            }

            if (GUI.Button(EditorGUI.IndentedRect(EditorGUILayout.GetControlRect()), $"Add info"))
            {
                Undo.RecordObject(groupConnection, "Add info reference");
                groupConnection.infos.Add("");
            }
            for (int i = 0; i < groupConnection.infos.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                //groupConnection.infos[i] = EditorGUILayout.TextField("Info title: ", groupConnection.infos[i]);

                groupConnection.infos[i] = SpellCheckHintsContent.DrawTextField(
                    $"{groupConnection.GetInstanceID()}_{nameof(groupConnection.infos)}_{i}",
                    "Info title:",
                    groupConnection.infos[i],
                    repaintAction,
                    n => { groupConnection.infos[i] = n; });

                if (GUI.Button(EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(GUILayout.Width(80))), $"Delete", Styles.DeleteButtonStyle))
                {
                    groupConnection.infos.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("State references:");
            EditorGUI.indentLevel++;
            foreach (var stateReference in groupConnection.states)
            {
                var title = stateReference ? stateReference.title : "No destination";
                EditorGUILayout.LabelField(title);
                var line = EditorGUILayout.BeginHorizontal();

                GUILayout.Space(40);

                if (GUILayout.Button("Move to"))
                {
                    StateEditorWindow.FocusCamera(stateReference.gameObject);
                    Selection.objects = new UnityEngine.Object[] { stateReference.gameObject };
                }

                if (GUILayout.Button("To simple connection"))
                {
                    Undo.RecordObject(groupConnection, "Delete state reference");
                    groupConnection.states.Remove(stateReference);
                    var addedConnection = Undo.AddComponent<Connection>(state.gameObject);
                    addedConnection.Destination = stateReference;
                    break;
                }

                if (Buttons.Delete())
                {
                    Undo.RecordObject(groupConnection, "Delete state reference");
                    groupConnection.states.Remove(stateReference);
                    break;
                }
                EditorGUILayout.EndHorizontal();
                
            }
            EditorGUILayout.BeginHorizontal();
            groupConnection.minimizeScale = EditorGUILayout.Slider(
                "minimize scale",
                groupConnection.minimizeScale,
                1, 5);
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
        }
    }
}
#endif
