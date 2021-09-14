using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Packages.tour_creator.Editor.Protocol
{
    [Serializable]
    public class Tour
    {
        public string id;
        public string title;

        public DateTimeOffset BuildTime
        {
            get => DateTimeOffset.TryParse(buildTime, out var parsed) ? parsed : DateTimeOffset.MinValue;
            set => buildTime = value.ToString("O");
        }
        [SerializeField]
        private string buildTime;


        public int versionNum;
        public string tourProtocolVersion;

        public string firstStateId;
        public List<State> states;
        public Color[] colorSchemes;
    }
}
