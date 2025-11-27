using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

namespace Excursion360_Builder.Shared.States.Items.Field
{
    /// <summary>
    /// Интерактивная область, при нажатии раскрывается её наполнение
    /// </summary>
    public class FieldItem : MonoBehaviour
    {
        public FieldVertex[] vertices;

        public string title;
        public string debugTitle;

        /// <summary>
        /// Скрывать ли поле при отрисовке редактора.
        /// </br>
        /// Сделано как обратное, чтобы по умолчанию на всех существующих было false и не меняло поведение
        /// </summary>
        public bool hideInDebug;

        public Texture texture;
        /// <summary>
        /// Изображения, которые прикреплены к элементу
        /// </summary>
        public List<Texture> images = new List<Texture>();
        /// <summary>
        /// Аудио сопровождение для изображений, по соответствию индексов
        /// </summary>
        /// <remarks>
        /// Да, в идеале сделать бы некую структуру, определяющую как изображение так и аудио, но для упрощения миграции хранится в разных списках
        /// </remarks>
        public List<AudioClip> imageAudios = new List<AudioClip>();
        /// <summary>
        /// Текстовые сопровождения для изображений, по соответствию индексов
        /// </summary>
        /// <remarks>
        /// Да, в идеале сделать бы некую структуру, определяющую всё связанное с изображением, но для упрощения миграции хранится в разных списках
        /// </remarks>
        public List<string> imageTexts = new List<string>();


        /// <summary>
        /// Видео ролики, приложенные к элементу
        /// </summary>
        public List<VideoClip> videos = new List<VideoClip>();
        /// <summary>
        /// Общие аудио ролики, приложенные к элементу
        /// </summary>
        public List<AudioClip> audios = new List<AudioClip>();
        [TextArea(10, 120)]
        public string text;
        private void OnDrawGizmos()
        {
            // TODO Delete texture field after several releases
            if (texture)
            {
                images.Add(texture);
                texture = default;
            }
        }

        public FieldItem()
        {
            vertices = new FieldVertex[]
            {
                new FieldVertex{ index = 0 },
                new FieldVertex{ index = 1, Orientation = Quaternion.AngleAxis(45, Vector3.up) },
                new FieldVertex{ index = 2, Orientation = Quaternion.AngleAxis(45, Vector3.up) * Quaternion.AngleAxis(45, Vector3.right) },
                new FieldVertex{ index = 3, Orientation = Quaternion.AngleAxis(45, Vector3.right) }
            };
        }
#if UNITY_EDITOR
        public bool isOpened;
        public int attachmentsTabIndex;

        public bool HasNoContent =>
            (images.Count == 0 || images.Any(i => !i)) &&
            (videos.Count == 0 || videos.Any(v => !v)) &&
            (audios.Count == 0 || audios.Any(a => !a)) &&
            string.IsNullOrWhiteSpace(text);
#endif
    }
}
