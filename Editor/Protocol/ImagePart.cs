using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packages.Excursion360_Builder.Editor.Protocol
{
    [Serializable]
    public class ImagePart
    {
        public int x;
        public int y;
        public int width;
        public int height;
        public string route;
        public long size;

        public ImagePart(int x, int y, int width, int height, string route, long size)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.route = route;
            this.size = size;
        }
    }
}
