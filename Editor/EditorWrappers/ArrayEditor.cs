using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Packages.Excursion360_Builder.Editor.EditorWrappers
{
    internal class ArrayEditor
    {
        public static List<T> EditList<T>(List<T> source, Func<T, int, T> itemEditor)
        {
            var updated = new List<T>(source);
            if (source.Count == 0)
            {
                EditorGUILayout.BeginHorizontal();
                if (Buttons.Plus())
                {
                    updated = new List<T> { default };
                }
                EditorGUILayout.EndHorizontal();
            }
            for (int iSrc = 0, iUpd = 0; iSrc < source.Count; iSrc++, iUpd++)
            {
                EditorGUILayout.BeginHorizontal();
                updated[iUpd] = itemEditor(source[iSrc], iSrc);

                EditorGUILayout.BeginVertical(Buttons.LittleButtonWidth);
                if (iSrc == 0)
                {
                    if (Buttons.Plus())
                    {
                        updated.Insert(0, default);
                        iUpd++; // На этой итерации обновленный список "отстает" по индексам
                    }
                }
                if (Buttons.Delete())
                {
                    updated = new List<T>(source.Where((_, index) => index != iSrc));
                    iUpd--; // На этой итерации элементы обновленного списка теперь смещены на 1 назад
                }
                if (Buttons.Plus())
                {
                    updated = new List<T>(source);
                    updated.Insert(iSrc + 1, default);
                    iUpd++; // Парные элементы теперь находятся впереди
                }
                // Сделано только перемещение вверх для простоты реализации + не найдена иконка для стрелки "вниз"
                if (iSrc > 0)
                {
                    if (Buttons.Up())
                    {
                        updated = new List<T>(source);
                        (updated[iUpd - 1], updated[iUpd]) = (updated[iUpd], updated[iUpd - 1]);
                    }
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }
            return updated;
        }
    }
}
