using System;
using Excursion360_Builder.Runtime.Markers;
using Excursion360_Builder.Shared.States.Items.Field;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Video;
using Object = UnityEngine.Object;

/**
 * @brief Main object
 */
public class Tour : MonoBehaviour
{
    public static Tour Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<Tour>();

            return _instance;
        }
    }

    private static Tour _instance;

    [SerializeField]
    [HideInInspector]
    private string _id;
    /// <summary>
    /// Идентификатор тура, хранится в исходном коде конкретного тура для того, чтобы он был общим между всеми разраобтчиками
    /// </summary>
    public string Id => string.IsNullOrWhiteSpace(_id) ? _id = Guid.NewGuid().ToString() : _id;
    [SerializeField]
    [HideInInspector]
    private int _buildNum;
    /// <summary>
    /// Порядковый номер сборки тура, используется для именования результирующий файлов (позволяет избежаьт кеширования на стороне клиента)
    /// Не может быть уменьшен
    /// </summary>
    public int BuidNum
    {
        get => _buildNum;
        set
        {
            if (value < _buildNum)
            {
                throw new ArgumentException("BuildNum can't minimize", nameof(value));
            }
            _buildNum = value;
        }
    }

    public string title = ">>place tour name here<<";

    public State firstState;

    public ConnectionMarker connectionMarkerPrefab;
    public GroupConnectionMarker groupConnectionMarkerPrefab;
    public FieldItemMarker baseFieldItemGameObject;

    public Texture defaultTexture;
    [Tooltip("Use SVG icon")]
    public DefaultAsset logoTexture;

    public Texture2D bottomImageTexture;
    public double bottomImageSize = 5;

    private void OnValidate()
    {
        ValidateTexture();
        ValidateTitle();

    }

    private void ValidateTitle()
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return;
        }
        try
        {
            System.IO.Path.GetFullPath(title);
        }
        catch
        {
            title = "";
            EditorUtility.DisplayDialog("Incorrect title", $"Title >>{title}<< must be OK for folder name", "Ok");
        }
    }

    private void ValidateTexture()
    {
        if (!logoTexture)
        {
            return;
        }

        var path = AssetDatabase.GetAssetPath(logoTexture);
        if (System.IO.Path.GetExtension(path) == ".svg")
        {
            return;
        }
        EditorUtility.DisplayDialog("Incorrect logo", "You must use svg icon", "Ok");
        logoTexture = default;
        return;
    }

    public VideoPlayerPool videoPlayerPool
    {
        get
        {
            if (_videoPlayerPool == null)
            {
                var pool = new GameObject();
                pool.name = "__video_pool__";
                _videoPlayerPool = pool.AddComponent<VideoPlayerPool>();
            }

            return _videoPlayerPool;
        }
    }

    /**
     * @brief Transition speed in seconds
     */
    public float transitionSpeed = 2.0f;

    public ColorScheme[] colorSchemes = new ColorScheme[] { new ColorScheme { color = Color.red, name = "default" } };

    private State _currentState = null;
    private TextureSource _currentTextureSource = null;

    private State _nextState = null;
    private TextureSource _nextTextureSource = null;

    private float _transition;

    private Renderer _renderer;
    private MaterialPropertyBlock _materialProperties;
    private VideoPlayerPool _videoPlayerPool;

    private readonly List<Marker> _markers = new List<Marker>();

    void Start()
    {
        _renderer = GetComponentInChildren<Renderer>();
        Assert.IsNotNull(_renderer);

        Assert.IsNotNull(firstState);

        Assert.IsTrue(colorSchemes.Length > 0, "Need minimum 1 element in colors collection");
        _currentState = firstState;
        _currentTextureSource = PrepareState(_currentState);

        SpawnConnections();
    }

    void OnDestroy()
    {
        ClearConnections();
    }

    void Update()
    {
        if (_nextState != null)
        {
            _transition += Time.deltaTime;

            if (_transition >= 1.0f)
            {
                _currentTextureSource.InUse = false;
                _currentState = _nextState;
                _currentTextureSource = _nextTextureSource;
                _nextState = null;
                _nextTextureSource = null;
                _transition = 0.0f;
                SpawnConnections();
            }
        }

        UpdateMaterial();
    }

    public void StartTransition(State nextState)
    {
        if (_nextState != null || _transition > 0.0f)
            return;


        ClearConnections();

        _nextState = nextState;
        _nextTextureSource = PrepareState(_nextState);

        _transition = 0.0f;
    }

    public void SpawnConnections()
    {
        var connections = _currentState.GetComponents<Connection>();

        foreach (var connection in connections)
        {
            ConnectionMarker marker = Instantiate(connectionMarkerPrefab, transform);
            marker.name = "Marker to " + connection.GetDestenationTitle();
            marker.connection = connection;
            marker.transform.localPosition = connection.Orientation * Vector3.forward;
            var markerRenderer = marker.GetComponent<Renderer>();
            markerRenderer.material.SetColor("_Color", colorSchemes[connection.colorScheme].color);
            _markers.Add(marker);
        }

        var groupConnections = _currentState.GetComponents<GroupConnection>();

        foreach (var groupConnection in groupConnections)
        {
            GroupConnectionMarker marker = Instantiate(groupConnectionMarkerPrefab, transform);
            marker.name = "Group Marker to " + groupConnection.title;
            marker.groupConnection = groupConnection;
            marker.transform.localPosition = groupConnection.Orientation * Vector3.forward;
            var markerRenderer = marker.GetComponent<Renderer>();
            markerRenderer.material.SetColor("_Color", Color.blue);
            _markers.Add(marker);
        }

        var fieldItems = _currentState.GetComponents<FieldItem>();


        foreach (var fieldItem in fieldItems)
        {
            var fieldItemMarker = Instantiate(baseFieldItemGameObject, transform);
            fieldItemMarker.fieldItem = fieldItem;
            var vertices = new Vector3[]
            {
                fieldItem.vertices[0].Orientation * Vector3.forward,
                fieldItem.vertices[1].Orientation * Vector3.forward,
                fieldItem.vertices[2].Orientation * Vector3.forward,
                fieldItem.vertices[3].Orientation * Vector3.forward
            };
            var tris = new int[]
            {
                0, 1, 2,
                0, 2, 3
            };
            MeshRenderer meshRenderer = fieldItemMarker.gameObject.AddComponent<MeshRenderer>();
            var mat = AssetDatabase.LoadAssetAtPath<Material>(
                "Packages/com.rexagon.tour-creator/Materials/FieldItem.mat");
            meshRenderer.sharedMaterial = mat;
            MeshFilter meshFilter = fieldItemMarker.gameObject.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh
            {
                vertices = vertices,
                triangles = tris
            };
            meshFilter.mesh = mesh;
            fieldItemMarker.gameObject.AddComponent<MeshCollider>();
        }
    }

    public void ClearConnections()
    {
        foreach (var marker in _markers)
        {
            Destroy(marker.gameObject);
        }

        _markers.Clear();
    }

    private void UpdateMaterial()
    {
        if (_currentState == null || _transition != 0.0f && _nextState == null)
            return;

        if (_materialProperties == null)
            _materialProperties = new MaterialPropertyBlock();

        _renderer.GetPropertyBlock(_materialProperties);

        // Set main texture and orientation
        _materialProperties.SetTexture("_MainTex", _currentTextureSource.LoadedTexture);

        var mr = _currentState.transform.rotation;
        _materialProperties.SetVector("_MainOrientation", new Vector4(
            mr.x, mr.y, mr.z, mr.w));

        if (_nextState != null && _nextTextureSource != null)
        {
            // Set next texture and orientation
            _materialProperties.SetTexture("_NextTex", _nextTextureSource.LoadedTexture);

            var nr = _nextState.transform.rotation;
            _materialProperties.SetVector("_NextOrientation", new Vector4(
                nr.x, nr.y, nr.z, nr.w));
        }

        // Set transition
        _materialProperties.SetFloat("_Transition", _transition);

        _renderer.SetPropertyBlock(_materialProperties);
    }

    private TextureSource PrepareState(State state)
    {
        var textureSource = state.GetComponent<TextureSource>();
        textureSource.InUse = true;
        StartCoroutine(textureSource.LoadTexture());

        var connections = state.GetComponents<Connection>();
        foreach (var connection in connections)
        {
            if (connection.Destination)
                StartCoroutine(connection.Destination.GetComponent<TextureSource>().LoadTexture());
        }

        return textureSource;
    }
}