using System;
using UnityEngine;

namespace Packages.tour_creator.Editor.Protocol
{
    [Serializable]
    public class ContentItem
    {
        public Quaternion orientation;
        public ContentItemType contentType;
        public float multipler;
        public string image;
    }

    public enum ContentItemType
    {
        Unknown,
        Image
    }
}
