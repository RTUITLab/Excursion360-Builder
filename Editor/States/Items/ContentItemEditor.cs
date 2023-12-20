using Packages.Excursion360_Builder.Editor.SpellCheck;
using Packages.Excursion360_Builder.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR

class ContentItemEditor : EditorBase
{
    public void Draw(State state, Action repaintAction)
    {

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add image"))
        {
            Undo.AddComponent<ImageContentItem>(state.gameObject);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUI.indentLevel++;
        foreach (var item in state.GetComponents<ContentItem>())
        {
            var titleName = GetTitleStringOf(item.debugTitle);
            if (item.isOpened = EditorGUILayout.Foldout(item.isOpened, $"[{titleName}]", true))
            {
                Undo.RecordObject(item, "Edit state item name");

                EditorGUILayout.BeginHorizontal();

                item.debugTitle = SpellCheckHintsContent.DrawTextField(
                    $"{item.GetInstanceID()}_{nameof(item.debugTitle)}",
                    "Debug title",
                    item.debugTitle,
                    repaintAction,
                    n => { item.debugTitle = n; });

                var value = StateItemPlaceEditor.EditableItem == (object)item;
                if (GUILayout.Toggle(value, "edit", Styles.ToggleButtonStyleNormal))
                {
                    StateItemPlaceEditor.EnableEditing(state, item, Color.green);
                }
                else
                {
                    if (StateItemPlaceEditor.EditableItem as UnityEngine.Object == item)
                    {
                        StateItemPlaceEditor.CleadEditing();
                    }
                }
                if (Buttons.Delete())
                {
                    Undo.DestroyObjectImmediate(item);
                }
                EditorGUILayout.EndHorizontal();
                item.multipler = EditorGUILayout.FloatField("multipler", item.multipler);
                if (item is ImageContentItem imageContentItem)
                {
                    imageContentItem.image = (Texture)EditorGUILayout.ObjectField($"Image", imageContentItem.image, typeof(Texture), true);
                }
            }
            EditorGUILayout.Space();
        }
        EditorGUI.indentLevel--;
    }
}

#endif
