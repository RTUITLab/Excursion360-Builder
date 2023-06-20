using Packages.Excursion360_Builder.Editor.Protocol;
using Packages.tour_creator.Editor.WebBuild;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Packages.Excursion360_Builder.Editor.WebBuild
{
    public class ImageCropper
    {
        public const int MIN_PARTS_COUNT = 5;
        public const int MAX_PARTS_COUNT = 20;
        public static void HandleImage(
            string sourceImagePath,
            string targetFolderPath,
            int partsCount)
        {
            if (partsCount < MIN_PARTS_COUNT || partsCount > MAX_PARTS_COUNT) throw new ArgumentOutOfRangeException($"Incorrect parts count (must be in [{MIN_PARTS_COUNT} - {MAX_PARTS_COUNT}])", nameof(partsCount));
            if (!File.Exists(sourceImagePath)) throw new ArgumentException($"File not found {sourceImagePath}", nameof(sourceImagePath));

            if (Directory.Exists(targetFolderPath))
            {
                Directory.Delete(targetFolderPath, true);
            }
            Directory.CreateDirectory(targetFolderPath);
            using (var sourceImage = Image.FromFile(sourceImagePath).FixOrientation())
            {
                var lowQualityImage = CreateLowQualityImage(sourceImage, targetFolderPath);

                var partHeight = (int)Math.Floor((double)sourceImage.Height / partsCount);
                var partWidth = (int)Math.Floor((double)sourceImage.Width / partsCount);


                var top = CreateAllWidthRect(sourceImage, targetFolderPath, 0, partHeight, "top");
                var downPositionStart = (partsCount - 1) * partHeight;
                var down = CreateAllWidthRect(sourceImage, targetFolderPath, downPositionStart, sourceImage.Height - downPositionStart, "down");


                var rectangles = new List<ImagePart> { top, down };


                using (var targetImage = new Bitmap(partWidth, partHeight))
                {

                    targetImage.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
                    using (var drawerOnNewImage = System.Drawing.Graphics.FromImage(targetImage))
                    {

                        drawerOnNewImage.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        drawerOnNewImage.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        drawerOnNewImage.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;


                        int num = 0;
                        for (int x = 0; x < sourceImage.Width; x += partWidth)
                        {
                            if (x + partWidth < sourceImage.Width && x + partWidth * 2 > sourceImage.Width)
                            {
                                partWidth = (sourceImage.Width - x) / 2;
                            }
                            if (partWidth == 0)
                            {
                                throw new Exception("zero width");
                            }
                            for (int y = partHeight; y < downPositionStart; y += partHeight)
                            {
                                var src = new Rectangle(
                                    x, y,
                                    Math.Min(partWidth, sourceImage.Width - x),
                                    Math.Min(partHeight, sourceImage.Height - y));

                                var dest = new Rectangle(0, 0, src.Width, src.Height);


                                drawerOnNewImage.DrawImage(sourceImage, dest, src, GraphicsUnit.Pixel);
                                var imageRoute = $"{num++}.jpg";
                                var imageLocation = Path.Combine(targetFolderPath, imageRoute);
                                targetImage.SaveCompressed(imageLocation);
                                rectangles.Add(new ImagePart(src.X, src.Y, src.Width, src.Height, imageRoute, new FileInfo(imageLocation).Length));
                            }
                        }

                    }
                    File.WriteAllText(Path.Combine(targetFolderPath, "meta.json"), JsonUtility.ToJson(new ImageMetaInfo(
                      sourceImage.Width,
                      sourceImage.Height,
                      lowQualityImage,
                      rectangles,
                      new FileInfo(sourceImagePath).Length)));
                }
            }
        }

        private static ImagePart CreateAllWidthRect(Image sourceImage, string targetLocation, int yStart, int height, string name)
        {
            using (var targetImage = new Bitmap(sourceImage.Width, height))
            {
                targetImage.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
                using (var drawerOnNewImage = System.Drawing.Graphics.FromImage(targetImage))
                {
                    drawerOnNewImage.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    drawerOnNewImage.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    drawerOnNewImage.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    drawerOnNewImage.DrawImage(sourceImage,
                      new Rectangle(0, 0, targetImage.Width, height),
                      new Rectangle(0, yStart, sourceImage.Width, height),
                      GraphicsUnit.Pixel
                    );
                    var imagePath = $"{name}.jpg";
                    var imageLocation = Path.Combine(targetLocation, imagePath);
                    targetImage.SaveCompressed(imageLocation, quality: 30, maxTextureSize: -1);

                    var allWithRect = new ImagePart(0, yStart, targetImage.Width, height, imagePath, new FileInfo(imageLocation).Length);
                    return allWithRect;
                }
            }
        }

        private static ImagePart CreateLowQualityImage(Image sourceImage, string targetLocation)
        {
            var imageRoute = $"lq.jpg";
            var imageLocation = Path.Combine(targetLocation, imageRoute);
            var size = sourceImage.SaveCompressed(imageLocation);
            return new ImagePart(0, 0, size.Width, size.Height, "lq.jpg", new FileInfo(imageLocation).Length);
        }
    }
}