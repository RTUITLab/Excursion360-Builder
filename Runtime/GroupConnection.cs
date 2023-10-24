using Excursion360_Builder.Shared.States.Items;
using System;
using System.Collections.Generic;

public class GroupConnection : MonoBehaviourStateItem
{
    public State Origin => GetComponent<State>();
    public string title;
    public GroupConnectionViewMode viewMode;
    public List<State> states = new();
    public List<string> infos = new();
    public float minimizeScale;
    public List<StateRotationAfterStepAnglePair> rotationAfterStepAngles = new List<StateRotationAfterStepAnglePair>();
}

public enum GroupConnectionViewMode
{
    ShowByClickOnItem,
    AlwaysShowOnlyButtons,
}

[Serializable]
public class StateRotationAfterStepAnglePair
{
    public State state;
    public float rotationAfterStepAngle;
}

