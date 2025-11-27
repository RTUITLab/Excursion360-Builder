using System;

namespace Packages.tour_creator.Editor.Protocol
{
    /// <summary>
    /// Аудио сопровождение для интерактивного элемента
    /// </summary>
    [Serializable]
    public class FieldItemAudioContent
    {
        /// <summary>
        /// Путь к ресурсу - аудио фаллу
        /// </summary>
        public string src;
        /// <summary>
        /// Длительность аудио файла
        /// </summary>
        public float duration;
    }
}
