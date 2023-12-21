using Packages.Excursion360_Builder.Editor.WebBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Packages.Excursion360_Builder.Editor
{
    static class ProjectEditorPrefs
    {
        private const string BUILD_LOCATION = nameof(ProjectEditorPrefs) + nameof(BUILD_LOCATION);
        private const string CROPPING_LEVEL = nameof(ProjectEditorPrefs) + nameof(CROPPING_LEVEL);

        public static string ProjectId => Tour.Instance.Id;

        public static int BuildNum
        {
            get => Tour.Instance.BuidNum;
            private set => Tour.Instance.BuidNum = value;
        }
        public static DateTimeOffset BuildDate
        {
            get => Tour.Instance.BuildDate;
            private set => Tour.Instance.BuildDate = value;
        }
        public static string BuildViewerVersion
        {
            get => Tour.Instance.BuildViewerVersion;
            private set => Tour.Instance.BuildViewerVersion = value;
        }
        public static int IncrementBuildNum()
        {
            return ++BuildNum;
        }
        public static void BuildSuccess(string viewerVersion)
        {
            BuildDate = DateTimeOffset.UtcNow;
            BuildViewerVersion = viewerVersion;
        }

        public static string BuildLocation
        {
            get => PlayerPrefs.GetString(BUILD_LOCATION, "");
            set {
                PlayerPrefs.SetString(BUILD_LOCATION, value);
                PlayerPrefs.Save();
            }
        }
        public static int ImageCroppingLevel
        {
            get => PlayerPrefs.GetInt(CROPPING_LEVEL, ImageCropper.MIN_PARTS_COUNT);
            set
            {
                PlayerPrefs.SetInt(CROPPING_LEVEL, value);
                PlayerPrefs.Save();
            }
        }
    }
}
