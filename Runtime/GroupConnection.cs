using Excursion360_Builder.Shared.States.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GroupConnection : MonoBehaviourStateItem
{
    public State Origin => GetComponent<State>();
    public string title;
    public List<State> states = new List<State>();
    public List<string> infos = new List<string>();
    public float minimizeScale;
    public List<StateRotationAfterStepAnglePair> rotationAfterStepAngles = new List<StateRotationAfterStepAnglePair>();
}

[Serializable]
public class StateRotationAfterStepAnglePair
{
    public State state;
    public float rotationAfterStepAngle;
}

