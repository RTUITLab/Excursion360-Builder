using System;
using System.Collections.Generic;

namespace Packages.tour_creator.Editor.Protocol
{
    [Serializable]
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
        /// <summary>
        /// коеффициент уменьшения размера рамки
        /// </summary>
        public float minimizeScale;
        /// <summary>
        /// Позиция заголовка группового перехода относительно точки <see cref="StateItem.rotation"/>
        /// </summary>
        public float titleYPosition;
    }

    [Serializable]
    public class GroupStateLinkRotationOverride
    {
        public string stateId;
        public float rotationAfterStepAngle;
    }
}
