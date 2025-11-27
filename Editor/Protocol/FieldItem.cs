using Packages.Excursion360_Builder.Editor.Protocol;
using System;
using UnityEngine;

namespace Packages.tour_creator.Editor.Protocol
{
    /// <summary>
    /// Интерактивный элемент на сцене
    /// </summary>
    [Serializable]
    public class FieldItem
    {
        /// <summary>
        /// Название интерактивного элдемента
        /// </summary>
        public string title;
        /// <summary>
        /// Контурные точки, определяющие контуры плоскости интерактивного элемента
        /// </summary>
        /// <remarks>
        /// Задается как углы вращения, так клиент отображения может расположить их на любом
        /// "расстоянии" от пользователя. Главное - чтобы с точки зрения виртуальной камеры они
        /// были расположены в верном месте.
        /// </remarks>
        public Quaternion[] vertices;
        /// <summary>
        /// Набор блоков-изображений для отображения внутри интерактивного элемента
        /// </summary>
        public FieldItemImageContent[] imageContent;
        /// <summary>
        /// Список ссылок на ресурсы - видео ролики, отображаемые внутри интерактивного элемента
        /// </summary>
        public string[] videos;
        /// <summary>
        /// Текст-описание к интерактивному элементу
        /// </summary>
        public string text;
        /// <summary>
        /// Набор блоков-аудио для аудио сопровождения интерактивного элемента
        /// </summary>
        public FieldItemAudioContent[] audios;
    }
}
