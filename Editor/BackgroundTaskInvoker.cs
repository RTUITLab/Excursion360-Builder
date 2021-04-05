using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Packages.Excursion360_Builder.Editor
{
    class BackgroundTaskInvoker
    {
        public static void StartBackgroundTask(IEnumerator update)
        {
            void closureCallback()
            {
                try
                {
                    if (update.MoveNext() == false)
                    {
                        EditorApplication.update -= closureCallback;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    EditorApplication.update -= closureCallback;
                }
            }
            EditorApplication.update += closureCallback;
        }
    }
}
