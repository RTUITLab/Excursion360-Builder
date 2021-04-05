using System.IO;
using System.Collections;
using UnityEditor;

#if UNITY_EDITOR
using UnityEngine;
#endif

public class FileImageSource : TextureSource
{
    public Texture texture = null;

    public override Type SourceType => Type.Image;

    public override IEnumerator LoadTexture()
    {
        LoadedTexture = texture;
        yield break;
    }

#if UNITY_EDITOR
    public override string Export(string destination, string stateName)
    {
        string path = AssetDatabase.GetAssetPath(texture);
        string filename = stateName + Path.GetExtension(path);

        File.Copy(path, Path.Combine(destination, filename), true);
        return filename;
    }

    public override string GetAssetPath()
    {
        return AssetDatabase.GetAssetPath(texture);
    }
#endif
}
