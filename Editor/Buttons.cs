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
        public static bool Delete()
        {
            return GUILayout.Button(EditorGUIUtility.IconContent("d_TreeEditor.Trash"), GUILayout.Width(30));
        }
        public static bool Warning()
        {
            return GUILayout.Button(EditorGUIUtility.IconContent("Warning"), GUILayout.Width(30));
        }
        public static bool Loading()
        {
            return GUILayout.Button(EditorGUIUtility.IconContent("Loading"), GUILayout.Width(30));
        }
        public static bool Valid()
        {
            return GUILayout.Button(EditorGUIUtility.IconContent("d_Valid"), GUILayout.Width(30));
        }
    }
}
