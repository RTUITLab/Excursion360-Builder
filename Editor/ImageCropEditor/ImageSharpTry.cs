
using ImageMagick;
using Packages.tour_creator.Editor.WebBuild;
using System;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Packages.Excursion360_Builder.Editor.ImageCropEditor
{
    internal class ImageSharpTry
    {
        public static string ProcessAsPerspective(string sourceImagePath, UnityEngine.Vector2[] points)
        {
            var center = points.Aggregate((a, b) => a + b) / 4;
            points = points
                .OrderByDescending(v => UnityEngine.Vector3.SignedAngle(UnityEngine.Vector3.up, v - center, UnityEngine.Vector3.forward))
                .ToArray();

            var tempFile = Path.GetTempFileName();
            var jpgPath = Path.ChangeExtension(tempFile, ".jpg");
            File.Move(tempFile, jpgPath);
            System.Drawing.Size sourceSize;
            System.Drawing.Size compressedSize;
            using (var rawImage = System.Drawing.Image.FromFile(sourceImagePath).FixOrientation())
            {
                sourceSize = rawImage.Size;
                compressedSize = rawImage.SaveCompressed(jpgPath, quality: 100);
            }
            using var image = new MagickImage(jpgPath);

            for (int i = 0; i < points.Length; i++)
            {
                points[i] = points[i] * (compressedSize.Width / (float)sourceSize.Width);
            }
            
            var width = UnityEngine.Vector2.Distance(points[0], points[3]);
            var height = UnityEngine.Vector2.Distance(points[0], points[1]);

            image.Distort(DistortMethod.Perspective,
                points[0].x, points[0].y, 0, 0,
                points[1].x, points[1].y, 0, height,
                points[2].x, points[2].y, width, height,
                points[3].x, points[3].y, width, 0
                );
            image.Crop((int)width, (int)height);


            
            image.Write(jpgPath);
            UnityEngine.Debug.Log(jpgPath);

            return jpgPath;
        }

    }
}
