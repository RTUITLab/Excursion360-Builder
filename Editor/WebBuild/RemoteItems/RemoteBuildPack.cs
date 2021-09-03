using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packages.Excursion360_Builder.Editor.WebBuild.RemoteItems
{
    class RemoteBuildPack
    {
        /// <summary>
        /// Release ig on github
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Tag with release on github
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// Puclish date of that release
        /// </summary>
        public DateTime PublishDate { get; set; }

        /// <summary>
        /// Is item folded while rendered
        /// </summary>
        public bool IsFolded { get; set; }
        /// <summary>
        /// Status of loading inner data
        /// </summary>
        public BuildPackStatus Status { get; set; }
    }
}
