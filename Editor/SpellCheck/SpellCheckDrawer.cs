using Packages.Excursion360_Builder.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Packages.Excursion360_Builder.Editor.SpellCheck
{
    internal class SpellCheckHintsContent : PopupWindowContent
    {
        public static string DrawTextField(
            string uniqueFieldKey,
            string currentContent,
            Action rerenderCallback,
            Action<string> updateTextCallback,
            bool multiline = false)
            => DrawTextField(uniqueFieldKey, default, currentContent, rerenderCallback, updateTextCallback, multiline);
        public static string DrawTextField(
            string uniqueFieldKey,
            string label,
            string currentContent,
            Action repaintCallback,
            Action<string> updateTextCallback,
            bool multiline = false)
        {
            var currentRect = EditorGUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUI.indentLevel * 20);

            var result = SpellCheckCache.QueueChecking(uniqueFieldKey, currentContent, repaintCallback);
            if (result == null)
            {
                if (Buttons.Loading())
                {
                }
            }
            else if (result.Length == 0)
            {
                Buttons.Valid();
            }
            else
            {
                if (Buttons.Warning())
                {
                    var popupContent = new SpellCheckHintsContent(currentContent, result, (newRow) =>
                    {
                        updateTextCallback(newRow);
                        repaintCallback();
                    });
                    PopupWindow.Show(
                        new Rect(currentRect.x + 30, currentRect.y, currentRect.width, currentRect.height),
                        popupContent);
                }
            }
            currentContent =
                multiline ? EditorGUILayout.TextArea(currentContent) :
                string.IsNullOrEmpty(label)
                ? EditorGUILayout.TextField(currentContent) :
                  EditorGUILayout.TextField(label, currentContent);
            EditorGUILayout.EndHorizontal();
            return currentContent;
        }

        private RowResponse[] responses;
        private readonly Action<string> changeTextCallback;
        private string row;
        private string richRow;
        
        public SpellCheckHintsContent(string row, RowResponse[] responses, Action<string> changeTextCallback)
        {
            this.row = row;
            this.responses = responses;
            this.changeTextCallback = changeTextCallback;
            var builder = new StringBuilder();
            int currentIndex = 0;
            foreach (var response in responses)
            {
                builder.Append(row, currentIndex, response.pos - currentIndex);
                currentIndex = response.pos;
                builder.Append("<color=red><b>");
                builder.Append(row, currentIndex, response.len);
                currentIndex += response.len;
                builder.Append("</b></color>");
            }
            richRow = builder.ToString();
        }

        public override Vector2 GetWindowSize()
        {
            return Vector2.one * (450);
        }

        public override void OnGUI(Rect rect)
        {
            GUILayout.Label(richRow, Styles.RichLabelStyle);
            EditorGUILayout.Space(15);
            foreach (var error in responses)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(error.word);

                if (GUILayout.Button("Add to exceptions"))
                {
                    SpellCheckCache.AddToExceptions(error.word);
                    changeTextCallback(row);
                    editorWindow.Close();
                }

                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                foreach (var hint in error.s)
                {
                    if (GUILayout.Button(hint, GUILayout.Width(10 * hint.Length)))
                    {
                        var newText = ChangeText(error, hint);
                        changeTextCallback(newText);
                        editorWindow.Close();
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        private string ChangeText(RowResponse responseToChange, string newValue)
        {
            var builder = new StringBuilder();
            builder.Append(row, 0, responseToChange.pos);
            builder.Append(newValue);
            builder.Append(row, responseToChange.pos + responseToChange.len, row.Length - responseToChange.pos - responseToChange.len);
            return builder.ToString();
        }
    }
}
