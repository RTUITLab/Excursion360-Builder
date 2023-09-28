using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packages.Excursion360_Builder.Editor.Protocol
{
    [Serializable]
    public class BackgroundAudioInfo
    {
        public string id;
        public bool loopAudios;
        public List<string> audios;
    }
}
