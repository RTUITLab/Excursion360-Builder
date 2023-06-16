using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Packages.Excursion360_Builder.Editor
{
    class Buttons
    {
        public static readonly GUILayoutOption LittleButtonWidth = GUILayout.Width(30);
        public static bool Delete()
        {
            return GUILayout.Button(EditorGUIUtility.IconContent("d_TreeEditor.Trash"), LittleButtonWidth);
        }
        public static bool Warning()
        {
            return GUILayout.Button(EditorGUIUtility.IconContent("Warning"), LittleButtonWidth);
        }
        public static bool Loading()
        {
            return GUILayout.Button(EditorGUIUtility.IconContent("Loading"), LittleButtonWidth);
        }
        public static bool Valid()
        {
            return GUILayout.Button(EditorGUIUtility.IconContent("d_Valid"), LittleButtonWidth);
        }
        public static bool Plus()
        {
            return GUILayout.Button(EditorGUIUtility.IconContent("d_Toolbar Plus"), LittleButtonWidth);
        }
        public static bool Up()
        {
            return GUILayout.Button(EditorGUIUtility.IconContent("UpArrow"), LittleButtonWidth);
        }
    }
}
