using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

namespace Excursion360_Builder.Shared.States.Items.Field
{
    public class FieldItem : MonoBehaviour
    {
        public FieldVertex[] vertices;

        public string title;

        public Texture texture;
        public List<Texture> images = new List<Texture>();
        public List<VideoClip> videos = new List<VideoClip>();
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
#endif
    }
}
