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
        private const string PROJECT_ID = nameof(ProjectEditorPrefs) + nameof(PROJECT_ID);
        private const string BUILD_NUM = nameof(ProjectEditorPrefs) + nameof(BUILD_NUM);

        private const string BUILD_LOCATION = nameof(ProjectEditorPrefs) + nameof(BUILD_LOCATION);
        private const string CROPPING_LEVEL = nameof(ProjectEditorPrefs) + nameof(CROPPING_LEVEL);

        public static string ProjectId
        {
            get
            {
                if (!PlayerPrefs.HasKey(PROJECT_ID))
                {
                    PlayerPrefs.SetString(PROJECT_ID, Guid.NewGuid().ToString());
                    PlayerPrefs.Save();
                }
                return PlayerPrefs.GetString(PROJECT_ID);
            }
        }

        public static int BuildNum
        {
            get
            {
                if (!PlayerPrefs.HasKey(BUILD_NUM))
                {
                    PlayerPrefs.SetInt(BUILD_NUM, 0);
                }
                return PlayerPrefs.GetInt(BUILD_NUM);
            }
            private set
            {
                PlayerPrefs.SetInt(BUILD_NUM, BuildNum + 1);
                PlayerPrefs.Save();
            }
        }
        public static int IncrementBuildNum()
        {
            return ++BuildNum;
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
