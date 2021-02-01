using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Packages.tour_creator.Editor.Protocol
{
    [Serializable]
    public class StateLink : StateItem
    {
        public string id = "state";
        public int colorScheme;
        public bool rotationAfterStepAngleOverridden;
        public float rotationAfterStepAngle;
    }
}
