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
        private string example;
        private string example2;

        private string result;
        private string result2;

        private void OnGUI()
        {
            var newExample = EditorGUILayout.TextField("examnple", example);
            var newExample2 = EditorGUILayout.TextField("examnple2", example2);


            PrintResultFor("result 1", example, newExample, ref result);
            PrintResultFor("result 2", example2, newExample2, ref result2);

            example = newExample;
            example2 = newExample2;

            var (apiUsage, cacheUsage) = SpellCheckCache.StatsToday;
            EditorGUILayout.LabelField("Api hits", apiUsage.ToString());
            EditorGUILayout.LabelField("Cache hits", cacheUsage.ToString());
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
