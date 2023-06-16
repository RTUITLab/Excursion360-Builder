using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Packages.Excursion360_Builder.Editor.EditorWrappers
{
    internal class ArrayEditor
    {
        public static List<T> EditList<T>(List<T> source, Func<T, int, T> itemEditor)
        {
            var updated = source;
            if (source.Count == 0)
            {
                EditorGUILayout.BeginHorizontal();
                if (Buttons.Plus())
                {
                    updated = new List<T> { default };
                }
                EditorGUILayout.EndHorizontal();
            }
            for (int i = 0; i < source.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                source[i] = itemEditor(source[i], i);

                EditorGUILayout.BeginVertical(Buttons.LittleButtonWidth);
                if (i == 0)
                {
                    if (Buttons.Plus())
                    {
                        updated = new List<T>(source);
                        updated.Insert(0, default);
                    }
                }
                if (Buttons.Delete())
                {
                    updated = new List<T>(source.Where((_, index) => index != i));
                }
                if (Buttons.Plus())
                {
                    updated = new List<T>(source);
                    updated.Insert(i + 1, default);
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }
            return updated;
        }
    }
}
