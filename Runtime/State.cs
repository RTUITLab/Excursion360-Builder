using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Packages.Excursion360_Builder.Runtime;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Represents some place
/// </summary>
[ExecuteInEditMode]
public class State : MonoBehaviour
{
    /// <summary>
    /// Name of this place
    /// </summary>
    [SpellCheck]
    public string title;

    public BackgroundAudio backgroundAudio;

    private Renderer _renderer;
    private MaterialPropertyBlock _materialProperties;

    [SerializeField] private string id;

    /// <summary>
    /// Идентификатор состояния, меняется сценой, в которой оно содержится
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

    void Awake()
    {
        if (
#if UNITY_EDITOR
            EditorApplication.isPlaying ||
#endif
            Application.isPlaying)
        {
            GetComponent<Renderer>().enabled = false;
        }

#if UNITY_EDITOR
        ReloadTexture();
#endif
    }

#if UNITY_EDITOR

    void Update()
    {
        name = title;
    }

    public void Reload()
    {
        ReloadTexture();
    }

    public void ReloadTexture()
    {
        TextureSource textureSource = GetComponent<TextureSource>();
        if (textureSource == null)
            return;

        StartCoroutine(LoadTexture(textureSource));
    }

    private IEnumerator LoadTexture(TextureSource textureSource)
    {
        yield return StartCoroutine(textureSource.LoadTexture());

        if (_renderer == null)
            _renderer = GetComponent<Renderer>();

        if (_materialProperties == null)
            _materialProperties = new MaterialPropertyBlock();

        _renderer.GetPropertyBlock(_materialProperties);
        _materialProperties.SetTexture("_MainTex", textureSource.LoadedTexture);
        _renderer.SetPropertyBlock(_materialProperties);
    }
#endif
}
