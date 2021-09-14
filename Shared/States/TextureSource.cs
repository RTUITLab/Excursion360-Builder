using System.Collections;

#if UNITY_EDITOR
using UnityEngine;
#endif

[DisallowMultipleComponent]
public abstract class TextureSource : MonoBehaviour
{
    public enum Type
    {
        Image
    }

    public Texture LoadedTexture { protected set; get; }

    public bool InUse { 
        set
        {
            if (value == _inUse)
                return;
            _inUse = value;

            if (value)
                OnStartUsing();
            else
                OnStopUsing();
        }
        get
        {
            return _inUse;
        }
    }

    private bool _inUse = false;

    public abstract Type SourceType { get; }

    public abstract IEnumerator LoadTexture();

    protected virtual void OnStartUsing()
    {
    }

    protected virtual void OnStopUsing()
    {
    }

#if UNITY_EDITOR
    public abstract string Export(string destination, string stateName);
    /// <summary>
    /// Get path to resource file
    /// </summary>
    /// <returns></returns>
    public abstract string GetAssetPath();
#endif
}
