using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Packages.Excursion360_Builder.Editor.SpellCheck
{
    public class SpellCheckWindow : EditorWindow
    {
        private bool showExceptions;
        private bool showStats;

        private void OnGUI()
        {

            showExceptions = EditorGUILayout.Foldout(showExceptions, "Exceptions");
            if (showExceptions)
            {
                EditorGUI.indentLevel++;
                DrawExceptions();
                EditorGUI.indentLevel--;
            }

            showStats = EditorGUILayout.Foldout(showStats, "Stats");
            if (showStats)
            {
                EditorGUI.indentLevel++;
                var (apiUsage, cacheUsage) = SpellCheckCache.StatsToday;
                EditorGUILayout.LabelField("Api hits today", apiUsage.ToString());
                EditorGUILayout.LabelField("Cache hits today", cacheUsage.ToString());
                EditorGUI.indentLevel--;
            }
        }

        private void DrawExceptions()
        {
            var exceptions = SpellCheckCache.GetExceptions();
            if (exceptions.Length == 0)
            {
                EditorGUILayout.LabelField("Add word to exception, when see error");
                return;
            }
            foreach (var exception in exceptions)
            {
                EditorGUILayout.BeginHorizontal();

                if (Buttons.Delete())
                {
                    SpellCheckCache.RemoveFromExceptions(exception);
                    Repaint();
                }
                EditorGUILayout.LabelField(exception);

                EditorGUILayout.EndHorizontal();
            }
        }

        private void PrintResultFor(string label, string row, string newRow, ref string result)
        {
            if (row != newRow)
            {
                var info = SpellCheckCache.QueueChecking(label, row, Repaint);
                if (info == null)
                {
                    result = "Loading...";
                }
                else if (info.Length == 0)
                {
                    result = "No errors";
                }
                else
                {
                    result = string.Join(",", info[0].s ?? new string[0]);
                }
            }
            EditorGUILayout.LabelField(label, result);
        }
    }
}
