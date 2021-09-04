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
    [CustomPropertyDrawer(typeof(SpellCheckAttribute))]
    class SpellCheckDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use SpellCheck on string");
                return;
            }

            EditorGUI.PropertyField(position, property);
        }
    }
}
