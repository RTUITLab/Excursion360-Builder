using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BackgroundAudio : MonoBehaviour
{
    [SerializeField] private string id;

    /// <summary>
    /// Идентификатор аудио наполнения
    /// </summary>
    public string Id
    {
        get
        {
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString();
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
                return id;
            }
            else
            {
                return id;
            }
        }

        set
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
            id = value ?? Id;
        }
    }

    public bool loopAudios;
    public AudioClip[] audios;
}
