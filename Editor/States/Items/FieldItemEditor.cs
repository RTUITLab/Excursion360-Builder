using Excursion360_Builder.Shared.States.Items.Field;
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
        public void Draw(State state)
        {
            if (GUILayout.Button("Add"))
            {
                Undo.AddComponent<FieldItem>(state.gameObject);
            }

            EditorGUILayout.Space();

            var fieldItems = state.GetComponents<FieldItem>();
            foreach (var fieldItem in fieldItems)
            {
                var groupConnectionTitle = GetTitleStringOf(fieldItem.title);
                GUILayout.Label(groupConnectionTitle, EditorStyles.boldLabel);

                EditorGUI.indentLevel++;

                Undo.RecordObject(fieldItem, "Edit group connection title");

                fieldItem.title = EditorGUILayout.TextField("Title:", fieldItem.title);

                EditorGUILayout.BeginHorizontal();

                for (int i = 0; i < fieldItem.vertices.Length; i++)
                {
                    var vertex = fieldItem.vertices[i];
                    var buttonStyle = Styles.ToggleButtonStyleNormal;
                    if (StateItemPlaceEditor.EditableItem == (object) vertex)
                        buttonStyle = Styles.ToggleButtonStyleToggled;
                    if (GUILayout.Button(vertex.index.ToString(), buttonStyle))
                    {
                        if (StateItemPlaceEditor.EditableItem == (object) vertex)
                        {
                            StateItemPlaceEditor.CleadEditing();
                        }
                        else
                        {
                            StateItemPlaceEditor.EnableEditing(state, vertex, Color.green);
                        }
                    }
                }

                EditorGUILayout.EndHorizontal();
                var previewTexture = fieldItem.texture;
                if (previewTexture == null)
                {
                    previewTexture = EditorGUIUtility.whiteTexture;
                }

                fieldItem.texture = EditorGUI.ObjectField(EditorGUILayout.GetControlRect(false, 150.0f),
                    fieldItem.texture, typeof(Texture2D), false) as Texture;
            }
        }
    }
}