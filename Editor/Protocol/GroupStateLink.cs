using System;
using System.Collections.Generic;

namespace Packages.tour_creator.Editor.Protocol
{
    public enum GroupConnectionProtocolViewMode
    {
        ShowByClickOnItem,
        AlwaysShowOnlyButtons,
    }
    [Serializable]
    public class GroupStateLink : StateItem
    {
        public string title;
        public List<string> stateIds;
        public List<string> infos;
        public GroupConnectionProtocolViewMode viewMode;
        public List<GroupStateLinkRotationOverride> groupStateRotationOverrides;
        public float minimizeScale;
    }

    [Serializable]
    public class GroupStateLinkRotationOverride
    {
        public string stateId;
        public float rotationAfterStepAngle;
    }
}
