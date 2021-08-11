using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packages.Excursion360_Builder.Editor.Protocol
{
    [Serializable]
    public class ImageMetaInfo
    {
        public int width;
        public int height;
        public ImagePart lowQualityImage;
        public List<ImagePart> rectangles;
        public long originalSize;

        public ImageMetaInfo(int width, int height, ImagePart lowQualityImage, List<ImagePart> rectangles, long originalSize)
        {
            this.width = width;
            this.height = height;
            this.lowQualityImage = lowQualityImage;
            this.rectangles = rectangles;
            this.originalSize = originalSize;
        }

    }
}
