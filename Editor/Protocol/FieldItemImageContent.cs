using System;

namespace Packages.tour_creator.Editor.Protocol
{
    /// <summary>
    /// Наполнение интерактивного контента - изображения
    /// </summary>
    [Serializable]
    public class FieldItemImageContent
    {
        /// <summary>
        /// Путь к изображению - обязательный параметр
        /// </summary>
        public string imageSrc;

        /// <summary>
        /// Описание изображения для отображения в интерфейсе - опциональный параметр
        /// </summary>
        public string description;
        /// <summary>
        /// Аудио сопровождение для конкретного изображения - опциональный параметр
        /// </summary>
        public FieldItemAudioContent audio;
    }
}