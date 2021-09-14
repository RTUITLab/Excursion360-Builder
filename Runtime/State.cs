using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Packages.Excursion360_Builder.Runtime;

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

    private Renderer _renderer;
    private MaterialPropertyBlock _materialProperties;

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
