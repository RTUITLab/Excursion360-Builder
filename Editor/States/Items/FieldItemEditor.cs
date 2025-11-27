using Excursion360_Builder.Shared.States.Items.Field;
using Packages.Excursion360_Builder.Editor;
using Packages.Excursion360_Builder.Editor.EditorWrappers;
using Packages.Excursion360_Builder.Editor.SpellCheck;
using Packages.tour_creator.Editor.WebBuild;
using System;
using System.Linq;
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

            var fieldItems = state.GetComponents<FieldItem>();

            RenderShowHideAllButton(fieldItems);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            foreach (var (fieldItem, i) in fieldItems.Select((f, i) => (f, i)))
            {
                var title = fieldItem.title ?? "";
                if (fieldItem.HasNoContent)
                {
                    title = "[!] " + title;
                }
                var fieldItemTitle = GetTitleStringOf(title);
                if (!string.IsNullOrEmpty(fieldItem.debugTitle))
                {
                    fieldItemTitle = $"[{fieldItem.debugTitle}] {fieldItemTitle}";
                }
                if (TourEditor.StateGraphRenderer.showIndexNamesForFieldItems)
                {
                    fieldItemTitle = $"#{i} {fieldItemTitle}";
                }
                fieldItem.isOpened = EditorGUILayout.Foldout(fieldItem.isOpened, fieldItemTitle, true);
                if (fieldItem.isOpened)
                {
                    DrawFieldItem(state, fieldItem, repaintAction);
                }
            }
            if (GUILayout.Button("Добавить интерактивный элемент"))
            {
                var created = Undo.AddComponent<FieldItem>(state.gameObject);
                created.isOpened = true;
            }
            EditorGUI.indentLevel--;
        }

        private static void RenderShowHideAllButton(FieldItem[] fieldItems)
        {
            var targetState = !fieldItems.FirstOrDefault().hideInDebug;
            if (GUILayout.Button(targetState ? "Скрыть всё" : "Показать всё"))
            {
                foreach (var fieldItem in fieldItems)
                {
                    fieldItem.hideInDebug = targetState;
                }
            }
        }

        private void DrawFieldItem(State state, FieldItem fieldItem, Action repaintAction)
        {
            EditorGUI.indentLevel++;

            Undo.RecordObject(fieldItem, "Редактирование интерактивного элемента");
            EditorGUILayout.BeginHorizontal();


            fieldItem.title = SpellCheckHintsContent.DrawTextField(
                $"{fieldItem.GetInstanceID()}_{nameof(fieldItem.title)}",
                "Название",
                fieldItem.title,
                repaintAction,
                n => { fieldItem.title = n; });
            if (Buttons.Delete())
            {
                Undo.DestroyObjectImmediate(fieldItem);
            }
            EditorGUILayout.EndHorizontal();
            fieldItem.debugTitle = SpellCheckHintsContent.DrawTextField(
                $"{fieldItem.GetInstanceID()}_{nameof(fieldItem.debugTitle)}",
                "Для редактора",
                fieldItem.debugTitle,
                repaintAction,
                n => { fieldItem.debugTitle = n; });

            fieldItem.hideInDebug = !EditorGUILayout.Toggle("Отображать контур", !fieldItem.hideInDebug);

            if (!fieldItem.hideInDebug)
            {
                RenderPositionVertexes(state, fieldItem);
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUI.indentLevel * 15);
            fieldItem.attachmentsTabIndex = GUILayout.Toolbar(fieldItem.attachmentsTabIndex, new string[] {
                $"Изображения {fieldItem.images.Count}",
                "Видео",
                "Текст",
                $"Аудио {fieldItem.audios.Count}"
            });
            EditorGUILayout.EndHorizontal();

            if (fieldItem.attachmentsTabIndex == 0)
            {
                var result = ArrayEditor.EditList(
                    source: 
                        fieldItem.images.Select((image, i) => (image, audio: fieldItem.imageAudios.GetByIndexOrDefault(i), text: fieldItem.imageTexts.GetByIndexOrDefault(i))).ToList(),
                    itemEditor:
                        (t, i) => RenderImageWithAudioAndText(fieldItem, t, i, repaintAction));
                fieldItem.images = result.Select(r => r.image).ToList();
                fieldItem.imageAudios = result.Select(r => r.audio).ToList();
                fieldItem.imageTexts = result.Select(r => r.text).ToList();
            }
            else
            {
                var serializedObject = new SerializedObject(fieldItem);
                switch (fieldItem.attachmentsTabIndex)
                {
                    case 1:
                        var videosProperty = serializedObject.FindProperty(nameof(fieldItem.videos));
                        EditorGUILayout.PropertyField(videosProperty, new GUIContent("ТОЛЬКО ПЕРВОЕ ВИДЕО БУДЕТ ИСПОЛЬЗОВАНО (пока)"));
                        break;
                    case 2:
                        var textProperty = serializedObject.FindProperty(nameof(fieldItem.text));
                        EditorGUILayout.PropertyField(textProperty, new GUIContent("Текст-описание всего интерактивного элемента"));
                        break;
                    case 3:
                        var audiosProperty = serializedObject.FindProperty(nameof(fieldItem.audios));
                        EditorGUILayout.PropertyField(audiosProperty, new GUIContent("ТОЛЬКО ПЕРВОЕ АУДИО БУДЕТ ИСПОЛЬЗОВАНО! (now)"));
                        break;
                    default:
                        break;
                }
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUI.indentLevel--;

        }

        private (Texture texture, AudioClip audio, string text) RenderImageWithAudioAndText(
            FieldItem fieldItem, 
            (Texture texture, AudioClip audio, string text) elements, 
            int index, 
            Action repaintAction)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            var text = SpellCheckHintsContent.DrawTextField(
                $"{fieldItem.GetInstanceID()}_{nameof(fieldItem.title)}_{index}_{elements.text}",
                null,
                elements.text,
                repaintAction,
                n => { fieldItem.imageTexts.SetByIndexWithChangeLengthToRequired(index, n); },
                placeholder: "Подпись к изображению");
            var selectedAudio = (AudioClip)EditorGUILayout.ObjectField("", elements.audio, typeof(AudioClip), true);
            EditorGUILayout.EndVertical();
            
            var selectedTexture = (Texture)EditorGUILayout.ObjectField("", elements.texture, typeof(Texture), true, GUILayout.Width(95));

            EditorGUILayout.EndHorizontal();

            return (selectedTexture, selectedAudio, text);
        }

        private static void RenderPositionVertexes(State state, FieldItem fieldItem)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUI.indentLevel * 15);
            for (int i = 0; i < fieldItem.vertices.Length; i++)
            {
                var vertex = fieldItem.vertices[i];
                var value = StateItemPlaceEditor.EditableItem == (object)vertex;

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