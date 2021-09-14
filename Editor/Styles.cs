using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class Styles
{
    static Styles()
    {
        ToggleButtonStyleNormal = "Button";
        ToggleButtonStyleToggled = new GUIStyle(ToggleButtonStyleNormal);
        ToggleButtonStyleToggled.normal.background = ToggleButtonStyleToggled.active.background;

        DeleteButtonStyle = new GUIStyle(ToggleButtonStyleNormal);
        DeleteButtonStyle.normal.textColor = Color.red;
        DeleteButtonStyle.active.textColor = Color.red;

        RichLabelStyle = new GUIStyle(GUI.skin.label)
        {
            richText = true
        };
    }
    public static readonly GUIStyle ToggleButtonStyleNormal;
    public static readonly GUIStyle ToggleButtonStyleToggled;

    public static readonly GUIStyle DeleteButtonStyle;

    public static readonly GUIStyle RichLabelStyle;
}

