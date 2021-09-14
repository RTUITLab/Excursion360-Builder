using System;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

public class TextureSourceEditor
{
    private List<Type> _textureSourceTypes;

    private State _currentState;
    private Type _currentTextureSourceType;
    private Editor _currentTextureSourceEditor;

    public TextureSourceEditor()
    {
        Undo.undoRedoPerformed += Reload;

        _textureSourceTypes = new List<Type>();

        var parentType = typeof(TextureSource);

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (type.IsSubclassOf(parentType))
                {
                    _textureSourceTypes.Add(type);
                }
            }
        }
    }

    public void Draw(State state)
    {
        UpdateState(state);

        EditorGUI.BeginChangeCheck();

        _currentTextureSourceEditor.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck())
            _currentState.ReloadTexture();
    }

    public void ShowContextMenu(State state)
    {
        UpdateState(state);

        GenericMenu menu = new GenericMenu();

        foreach (var type in _textureSourceTypes)
        {
            AddItem(menu, type);
        }

        menu.ShowAsContext();
    }

    void AddItem(GenericMenu menu, Type type)
    {
        menu.AddItem(new GUIContent(type.Name), _currentTextureSourceType != null && _currentTextureSourceType.Equals(type), 
            OnTypeSelected, type);
    }

    void OnTypeSelected(object type)
    {
        if (_currentState == null)
            return;

        if (_currentTextureSourceType != null && _currentTextureSourceType.Equals(type))
            return;

        Undo.RecordObject(_currentState, "Undo state changes");

        TextureSource textureSource = _currentState.GetComponent<TextureSource>();
        if (textureSource)
            Undo.DestroyObjectImmediate(textureSource);

        Undo.AddComponent(_currentState.gameObject, type as Type);
        _currentState.ReloadTexture();

        UpdateEditor(_currentState);
    }

    void UpdateState(State state)
    {
        if (_currentState == null || _currentState != state)
            UpdateEditor(state);
    }

    void UpdateEditor(State state)
    {
        _currentState = state;

        TextureSource textureSource = state.GetComponent<TextureSource>();
        if (textureSource == null)
            textureSource = Undo.AddComponent<FileImageSource>(state.gameObject);

        _currentTextureSourceType = textureSource.GetType();

        if (_currentTextureSourceEditor != null)
            GameObject.DestroyImmediate(_currentTextureSourceEditor);

        _currentTextureSourceEditor = Editor.CreateEditor(textureSource);
    }

    void Reload()
    {
        if (_currentState != null)
            UpdateEditor(_currentState);
    }
}
