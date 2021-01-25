using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packages.Excursion360_Builder.Editor.WebBuild
{
    public class BuildPack
    {
        public int Id { get; set; }
        public string Version { get; set; }
        public DateTime PublishDate { get; set; }
        public string Location { get; set; }
        public bool IsFolded { get; set; }
        public BuildPackStatus Status { get; set; }
    }

    public enum BuildPackStatus
    {
        NotLoaded,
        Loading,
        Loaded,
        LoadingError
    }
}
