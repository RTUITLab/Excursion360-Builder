using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packages.tour_creator.Editor.Protocol
{
    [Serializable]
    class GroupStateLink : StateItem
    {
        public string title;
        public List<string> stateIds;
        public List<string> infos;
        public List<GroupStateLinkRotationOverride> groupStateRotationOverrides;
        public float minimizeScale;
    }

    [Serializable]
    class GroupStateLinkRotationOverride
    {
        public string stateId;
        public float rotationAfterStepAngle;
    }
}
