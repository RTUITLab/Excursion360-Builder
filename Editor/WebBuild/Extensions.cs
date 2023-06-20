using Excursion360_Builder.Shared.States.Items.Field;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Packages.tour_creator.Editor.WebBuild
{
    static class Extensions
    {
        public static string GetExportedId(this State state)
        {
            return "state_" + state.Id;
        }

        public static string GetExportedId(this FieldItem fieldItem)
        {
            return "field_item_" + fieldItem.GetInstanceID();
        }


        private static ImageCodecInfo JpegCodecInfo => ImageCodecInfo.GetImageEncoders().Single(e => e.MimeType == "image/jpeg");

        /// <summary>
        /// Сохраняет изоюражение в более низком качестве, и изменяя размер до <paramref name="maxTextureSize"/>
        /// </summary>
        /// <param name="image">Исходное изображение, которое нужно сохранить</param>
        /// <param name="location">Полный путь к файлу, куда нужно сохранить результат</param>
        /// <param name="quality">Парамтре качества изоюражения, от 0 до 100</param>
        /// <param name="maxTextureSize">Максимальная размерность стороны итогового результата. Если передать -1 размер не изменится</param>
        /// <exception cref="Exception">Выкидывает, если расширение файла не jpg</exception>
        /// <returns>Размер итогового изоюражения</returns>
        public static Size SaveCompressed(this Image image, string location, int quality = 50, int maxTextureSize = 4096)
        {
            if (Path.GetExtension(location).ToUpperInvariant() != ".JPG")
            {
                throw new Exception($"Can't compress to {Path.GetExtension(location)}, use 'jpg'");
            }
            using var parameters = GetLowQualityParameters(quality);
            var targetSize = FindCorrectSize(image.Size, maxTextureSize);
            if (targetSize == image.Size || maxTextureSize == -1)
            {
                image.Save(location, JpegCodecInfo, parameters);
                return image.Size;
            }
            else
            {
                using var resized = new Bitmap(image, targetSize);
                resized.Save(location, JpegCodecInfo, parameters);
                return resized.Size;
            }
        }
        /// <summary>
        /// Некоторые фотографии хранят изоюражение так, что System.Drawing неверно их читает, приходится фиксить
        /// 
        /// <see href="https://stackoverflow.com/a/23400751"/>
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static Image FixOrientation(this Image image)
        {
            if (Array.IndexOf(image.PropertyIdList, 274) > -1)
            {
                var orientation = (int)image.GetPropertyItem(274).Value[0];
                switch (orientation)
                {
                    case 1:
                        // No rotation required.
                        break;
                    case 2:
                        image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        break;
                    case 3:
                        image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case 4:
                        image.RotateFlip(RotateFlipType.Rotate180FlipX);
                        break;
                    case 5:
                        image.RotateFlip(RotateFlipType.Rotate90FlipX);
                        break;
                    case 6:
                        image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case 7:
                        image.RotateFlip(RotateFlipType.Rotate270FlipX);
                        break;
                    case 8:
                        image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                }
                // This EXIF data is now invalid and should be removed.
                image.RemovePropertyItem(274);
            }
            return image;
        }
        private static EncoderParameters GetLowQualityParameters(int quality)
        {
            var parameters = new EncoderParameters(1);
            parameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);
            return parameters;
        }
        private static Size FindCorrectSize(Size size, int maxDimension)
        {
            var max = Math.Max(size.Width, size.Height);
            if (max <= maxDimension)
            {
                return size;
            }
            var multipler = maxDimension / (double)max;
            return new Size(
                (int)Math.Ceiling(size.Width * multipler),
                (int)Math.Ceiling(size.Height * multipler));
        }
    }
}
