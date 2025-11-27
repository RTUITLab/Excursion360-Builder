using Packages.Excursion360_Builder.Editor.Protocol;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Packages.tour_creator.Editor.Protocol
{
    [Serializable]
    public class Tour
    {
        public string Id { get; set;  }
        public string Title { get; set;  }

        public DateTimeOffset BuildTime { get; set; }


        public int VersionNum;
        public string TourProtocolVersion;

        public string FirstStateId;
        public bool FastReturnToFirstStateEnabled;
        public List<State> States;
        public List<BackgroundAudioInfo> BackgroundAudios;
        public Color[] ColorSchemes;
    }
}
