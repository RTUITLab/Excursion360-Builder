//using System.IO;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Video;

//#if UNITY_EDITOR
//using UnityEditor;
//#endif

//public class FileVideoSource : TextureSource
//{
//    public enum PlayMode
//    {
//        RestartOnEnter,
//        LoopInBackground
//    }

//    public VideoClip videoClip;
//    public PlayMode playMode = PlayMode.RestartOnEnter;

//    private VideoClip _currentVideoClip = null;

//    private VideoPlayer _currentVideoPlayer = null;
//    private RenderTexture _currentRenderTexture = null;

//    public override Type SourceType => Type.Video;


//    public override IEnumerator LoadTexture()
//    {
//        if (videoClip == null)
//        {
//            LoadedTexture = null;
//            _currentRenderTexture = null;
//            _currentVideoClip = null;
//            yield break;
//        }

//        if (_currentVideoClip == null || _currentVideoClip != videoClip)
//        {
//            _currentVideoClip = videoClip;
//            _currentRenderTexture = new RenderTexture((int)videoClip.width, (int)videoClip.height, 0,
//                RenderTextureFormat.ARGB32);

//#if UNITY_EDITOR
//            if (!EditorApplication.isPlaying)
//                GeneratePreview();
//#endif
//        }

//        LoadedTexture = _currentRenderTexture;

//        if (InUse && _currentRenderTexture != null && _currentVideoPlayer && !_currentVideoPlayer.isPlaying)
//        {
//            _currentVideoPlayer.targetTexture = _currentRenderTexture;
//        }
//    }

//    protected override void OnStartUsing()
//    {
//        if (_currentVideoPlayer != null && playMode == PlayMode.LoopInBackground)
//            return;

//        if (_currentVideoPlayer == null)
//            _currentVideoPlayer = Tour.Instance.videoPlayerPool.Aquire();

//        _currentVideoPlayer.renderMode = VideoRenderMode.RenderTexture;
//        _currentVideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
//        _currentVideoPlayer.isLooping = playMode == PlayMode.LoopInBackground;

//        _currentVideoPlayer.clip = videoClip;

//        _currentVideoPlayer.time = 0;
//        _currentVideoPlayer.Play();

//        Graphics.Blit(Texture2D.blackTexture, _currentRenderTexture);
//    }

//    protected override void OnStopUsing()
//    {
//        if (playMode != PlayMode.LoopInBackground)
//        {
//            Tour.Instance.videoPlayerPool.Release(_currentVideoPlayer);
//        }
//    }

//#if UNITY_EDITOR
//    public override string Export(string destination, string stateName)
//    {
//        string path = AssetDatabase.GetAssetPath(videoClip);
//        string filename = stateName + Path.GetExtension(path);

//        File.Copy(path, Path.Combine(destination, filename), true);
//        return filename;
//    }
//#endif

//    private void GeneratePreview()
//    {
//        GameObject previewGeneratorObject = new GameObject();
//        previewGeneratorObject.name = "__preview_generator__";
//        previewGeneratorObject.SetActive(false);

//        var previewGenerator = previewGeneratorObject.AddComponent<VideoPreviewGenerator>();
//        previewGenerator.videoClip = videoClip;
//        previewGenerator.renderTexture = _currentRenderTexture;

//        previewGeneratorObject.SetActive(true);
//    }

//    public override string GetAssetPath()
//    {
//        return AssetDatabase.GetAssetPath(videoClip);
//    }
//}
