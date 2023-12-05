using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Excursion360_Builder.Shared.States.Items.Field;
using Packages.Excursion360_Builder.Editor.Extensions;

#if UNITY_EDITOR
using UnityEditor;


public class StateGraphRenderer
{
    /// <summary>
    /// If it is not null, connections will only be rendered for this state
    /// </summary>
    public State targetState = null;

    /// <summary>
    /// If true, labels will be rendered
    /// </summary>
    public bool showLabels;

    /// <summary>
    /// If true, connections will be rendered
    /// </summary>
    public bool showConnections;

    /// <summary>
    /// If true, items will be rendered
    /// </summary>
    public bool showItems;

    private GUIStyle _labelsStyle;
    private GUIStyle _labelsEditModeStyle;
    private GUIStyle _groupItemLabelStyle;

    public void RenderStateGraph(SceneView sceneview)
    {
        if (UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null || EditorApplication.isPlaying)
            return;


        if (_labelsStyle == null)
        {
            _labelsStyle = new GUIStyle();
            _labelsStyle.normal.textColor = Color.green;
        }

        if (_labelsEditModeStyle == null)
        {
            _labelsEditModeStyle = new GUIStyle();
            _labelsEditModeStyle.normal.textColor = Color.blue;
        }

        if (_groupItemLabelStyle == null)
        {
            _groupItemLabelStyle = new GUIStyle();
            _groupItemLabelStyle.normal.textColor = Color.green;
        }

        var states = UnityEngine.Object.FindObjectsOfType<State>();

        foreach (var state in states)
        {
            if (targetState != null && state != targetState)
                continue;

            RenderConnections(state);
            RenderGroupConnections(state);
            RenderItems(state);
            RenderFieldItems(state);
        }
    }

    private void RenderGroupConnections(State state)
    {
        var groupConnections = state.GetComponents<GroupConnection>();
        foreach (var item in groupConnections)
        {
            var itemPosition = item.transform.position + item.Orientation * Vector3.forward;
            Handles.color = Color.blue;
            Handles.DotHandleCap(
                0,
                itemPosition,
                item.transform.rotation,
                0.05f,
                EventType.Repaint
            );
            var firstConnectionPosition = state.transform.position + item.Orientation * Vector3.forward;
            foreach (var targetState in item.states)
            {
                Vector3 secondConnectionPosition;
                if (targetState == null)
                {
                    secondConnectionPosition = firstConnectionPosition + Vector3.up * 5;
                }
                else
                {

                    secondConnectionPosition = targetState.transform.position;

                    var backConnection = targetState.GetComponents<Connection>().FirstOrDefault(c => c.Destination == state);

                    if (backConnection == null)
                    {
                        var backGroupConnection = targetState.GetComponents<GroupConnection>().FirstOrDefault(gc => gc.states.Any(s => s == state));
                        if (backGroupConnection != null)
                        {
                            secondConnectionPosition += backGroupConnection.Orientation * Vector3.forward;
                        }
                    }
                    else
                    {
                        secondConnectionPosition += backConnection.Orientation * Vector3.forward;
                    }
                }

                if (showConnections)
                {
                    Handles.color = Color.yellow;
                    Handles.DrawLine(firstConnectionPosition, secondConnectionPosition);
                }
            }
        }
    }

    private void RenderItems(State state)
    {
        var items = state.GetComponents<ContentItem>();
        foreach (var item in items)
        {
            Debug.Log("Found item!");
            var itemPosition = item.transform.position + item.Orientation * Vector3.forward;
            Handles.color = Color.cyan;
            Handles.DotHandleCap(
                0,
                itemPosition,
                item.transform.rotation,
                0.05f,
                EventType.Repaint
            );
        }

    }

    private void RenderConnections(State state)
    {
        var connections = state.GetComponents<Connection>();

        foreach (var connection in connections)
        {
            var firstConnectionPosition = connection.GetOriginPosition();
            var secondConnectionPosition = connection.GetDestinationPosition(5);
            var backConnection = connection.GetBackConnection();
            var lineColor = Color.yellow;
            var dotColor = Color.red;
            if (!backConnection)
            {
                var backGroupConnection = connection.GetBackGroupConnection();
                if (backGroupConnection)
                {
                    secondConnectionPosition = backGroupConnection.GetOriginPosition();
                }
            }
            else
            {
                secondConnectionPosition = backConnection.GetOriginPosition();
            }

            if (!connection.Destination)
            {
                lineColor = dotColor = Color.black;
            }

            if (showConnections)
            {
                Handles.color = lineColor;
                Handles.DrawLine(firstConnectionPosition, secondConnectionPosition);

                Handles.color = dotColor;
                Handles.DotHandleCap(
                    0,
                    firstConnectionPosition,
                    Quaternion.identity,
                    0.07f,
                    EventType.Repaint
                );
            }

            // Draw label on connections when target is selected
            if (targetState != null && showLabels)
            {
                Handles.Label(firstConnectionPosition, connection.Origin.title, _labelsEditModeStyle);
            }
        }

        // Draw all labels when no target selected
        if (targetState == null && showLabels)
        {
            Handles.color = Color.blue;
            Handles.Label(state.transform.position, state.title, _labelsStyle);
        }
    }


    private void RenderFieldItems(State state)
    {
        var filedItems = state.GetComponents<FieldItem>();
        foreach (var item in filedItems)
        {
            if (item.hideInDebug)
            {
                continue;
            }
            if (item.vertices.Length < 2)
            {
                Debug.LogWarning("Invalid field vertex count");
            }
            for (int i = 0; i < item.vertices.Length; i++)
            {
                FieldVertex vertex = item.vertices[i];

                var position = state.gameObject.transform.position + vertex.Orientation * Vector3.forward;
                Handles.color = Color.green;
                if (StateItemPlaceEditor.EditableItem != vertex) // не рисуется квадрат, чтобы можно было попасть точно в угол объекта
                {
                    Handles.DotHandleCap(
                        0,
                        position,
                        Quaternion.identity,
                        0.01f,
                        EventType.Repaint
                    );
                }
                Handles.Label(position, vertex.index.ToString(), _labelsEditModeStyle);

                var preItem = item.vertices[i == 0 ? item.vertices.Length - 1 : i - 1];
                var prePosition = state.gameObject.transform.position + preItem.Orientation * Vector3.forward;
                Handles.color = Color.yellow;
                Handles.DrawLine(position, prePosition);
            }
            var centralPosition = item.vertices
                .Select(v => state.gameObject.transform.position + v.Orientation * Vector3.forward)
                .Aggregate((prev, next) => prev + next);
            centralPosition /= item.vertices.Length;
            Handles.Label(centralPosition, item.title, _groupItemLabelStyle);
        }
    }

    private void RenderViewPoint(State state)
    {

    }


}

#endif
