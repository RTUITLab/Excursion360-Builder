using System;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR

class IfShowFirstRotationEditor : EditorBase
{
    public void Draw(State state, Action repaintAction)
    {
        var isEditing = TourEditor.ViewDirectionRenderer.CurrentEditableObject == state;
        if (GUILayout.Toggle(isEditing, $"Edit initial rotation ~{Math.Round(state.ifFirstStateRotationAngle, 2)}", Styles.ToggleButtonStyleNormal))
        {
            TourEditor.ViewDirectionRenderer.SetEditing(
                            state,
                            () => state.ifFirstStateRotationAngle,
                            angle => state.ifFirstStateRotationAngle = angle,
                            state);
        }
        else
        {
            if (isEditing)
            {
                TourEditor.ViewDirectionRenderer.ClearEditing();
            }

        }
    }
}

#endif
