using Excursion360_Builder.Shared.States.Items.Field;
using Packages.Excursion360_Builder.Editor;
using Packages.Excursion360_Builder.Editor.SpellCheck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Excursion360_Builder.Editor.States.Items
{
    class FieldItemEditor : EditorBase
    {
        public void Draw(State state, Action repaintAction)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUI.indentLevel * 15);
            if (GUILayout.Button("Add"))
            {
                var fieldItem = Undo.AddComponent<FieldItem>(state.gameObject);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            var fieldItems = state.GetComponents<FieldItem>();
            foreach (var fieldItem in fieldItems)
            {
                var groupConnectionTitle = GetTitleStringOf(fieldItem.title);
                fieldItem.isOpened = EditorGUILayout.Foldout(fieldItem.isOpened, groupConnectionTitle, true);
                if (fieldItem.isOpened)
                {
                    DrawFieldItem(state, fieldItem, repaintAction);
                }
            }
            EditorGUI.indentLevel--;
        }

        private void DrawFieldItem(State state, FieldItem fieldItem, Action repaintAction)
        {
            EditorGUI.indentLevel++;

            Undo.RecordObject(fieldItem, "Edit group connection title");
            EditorGUILayout.BeginHorizontal();

            fieldItem.title = SpellCheckHintsContent.DrawTextField(
                $"{fieldItem.GetInstanceID()}_{nameof(fieldItem.title)}",
                "Title",
                fieldItem.title,
                repaintAction,
                n => { fieldItem.title = n; });

            //fieldItem.title = EditorGUILayout.TextField("Title:", fieldItem.title);
            if (Buttons.Delete())
            {
                Undo.DestroyObjectImmediate(fieldItem);
            }
            EditorGUILayout.EndHorizontal();

            RenderPositionVertexes(state, fieldItem);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUI.indentLevel * 15);
            fieldItem.attachmentsTabIndex = GUILayout.Toolbar(fieldItem.attachmentsTabIndex, new string[] {
                "Images",
                "Video",
                "Text",
                "Audio"
            });
            EditorGUILayout.EndHorizontal();
            var serializedObject = new SerializedObject(fieldItem);

            switch (fieldItem.attachmentsTabIndex)
            {
                case 0:
                    var imagesProperty = serializedObject.FindProperty(nameof(fieldItem.images));
                    EditorGUILayout.PropertyField(imagesProperty);
                    break;
                case 1:
                    var videosProperty = serializedObject.FindProperty(nameof(fieldItem.videos));
                    EditorGUILayout.PropertyField(videosProperty, new GUIContent("ONLY FIRST VIDEO WILL BE USED! (now)"));
                    break;
                case 2:
                    var textProperty = serializedObject.FindProperty(nameof(fieldItem.text));
                    EditorGUILayout.PropertyField(textProperty);
                    break;
                case 3:
                    var audiosProperty = serializedObject.FindProperty(nameof(fieldItem.audios));
                    EditorGUILayout.PropertyField(audiosProperty, new GUIContent("ONLY FIRST AUDIO WILL BE USED! (now)"));
                    break;
                default:
                    break;
            }
            serializedObject.ApplyModifiedProperties();
            EditorGUI.indentLevel--;

        }

        private static void RenderPositionVertexes(State state, FieldItem fieldItem)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUI.indentLevel * 15);
            for (int i = 0; i < fieldItem.vertices.Length; i++)
            {
                var vertex = fieldItem.vertices[i];
                var value = StateItemPlaceEditor.EditableItem == (object)vertex;
                Debug.Log($"item {i} value: {value}");
                if (GUILayout.Toggle(value, vertex.index.ToString(), Styles.ToggleButtonStyleNormal))
                {
                    // Clicked to true
                    StateItemPlaceEditor.EnableEditing(state, vertex, Color.green);
                }
                else // disabled
                {
                    if (StateItemPlaceEditor.EditableItem == (object)vertex)
                    {
                        StateItemPlaceEditor.CleadEditing();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}