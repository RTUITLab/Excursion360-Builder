using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Packages.Excursion360_Builder.Editor.States.Items
{
    class ConnectionsToStateEditor
    {
        private readonly Dictionary<State, Connection[]> connectionsTo = new Dictionary<State, Connection[]>();
        private readonly Dictionary<State, GroupConnection[]> groupConnectionsTo = new Dictionary<State, GroupConnection[]>();


        private Connection[] LoadConnections(State state)
        {
            return GameObject.FindObjectsOfType<Connection>()
                    .Where(c => c.Destination == state)
                    .ToArray();
        }
        private GroupConnection[] LoadGroupConnections(State state)
        {
            return GameObject.FindObjectsOfType<GroupConnection>()
                    .Where(c => c.states.Contains(state))
                    .ToArray();
        }
        public void Draw(State state)
        {
            if (!connectionsTo.ContainsKey(state) || !groupConnectionsTo.ContainsKey(state))
            {
                connectionsTo.Add(state, LoadConnections(state));
                groupConnectionsTo.Add(state, LoadGroupConnections(state));
            }
            if (GUILayout.Button("Refresh"))
            {
                connectionsTo[state] = LoadConnections(state);
                groupConnectionsTo[state] = LoadGroupConnections(state);
            }
            EditorGUILayout.LabelField("just connections");
            foreach (var connection in connectionsTo[state])
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(connection.Origin.title, EditorStyles.boldLabel);
                var overrideButtonText = connection.rotationAfterStepAngleOverridden ? "clean" : "override";
                if (GUILayout.Button(overrideButtonText, "Button"))
                {
                    connection.rotationAfterStepAngleOverridden = !connection.rotationAfterStepAngleOverridden;
                    if (connection.rotationAfterStepAngleOverridden)
                    {
                        StartEditRotationOverride(state, connection);
                    }
                }
                if (connection.rotationAfterStepAngleOverridden)
                {
                    GUILayout.Label(connection.rotationAfterStepAngle.ToString());

                    var isEditable = TourEditor.ViewDirectionRenderer.CurrentEditableObject == connection;

                    if (GUILayout.Toggle(isEditable, "edit", "Button") != isEditable)
                    {
                        if (isEditable)
                        {
                            TourEditor.ViewDirectionRenderer.ClearEditing();
                        }
                        else
                        {
                            StartEditRotationOverride(state, connection);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.LabelField("group connections");
            foreach (var groupConnection in groupConnectionsTo[state])
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(groupConnection.Origin.title, EditorStyles.boldLabel);

                var isConnectionOverrided = groupConnection.rotationAfterStepAngles.Any(p => p.state == state);

                var overrideButtonText = isConnectionOverrided ? "clean" : "override";
                if (GUILayout.Button(overrideButtonText, "Button"))
                {
                    if (isConnectionOverrided)
                    {
                        groupConnection
                            .rotationAfterStepAngles.RemoveAll(p => p.state == state);
                    }
                    else
                    {
                        var newPair = new StateRotationAfterStepAnglePair
                        {
                            state = state,
                            rotationAfterStepAngle = 0
                        };
                        groupConnection
                            .rotationAfterStepAngles
                            .Add(newPair);
                        TourEditor.ViewDirectionRenderer.SetEditing(
                                                        state,
                                                        () => newPair.rotationAfterStepAngle,
                                                        angle =>
                                                        {
                                                            newPair.rotationAfterStepAngle = angle;
                                                        },
                                                        groupConnection
                                                    );
                    }
                }

                // After button click
                isConnectionOverrided = groupConnection.rotationAfterStepAngles.Any(p => p.state == state);

                if (isConnectionOverrided)
                {
                    var currentEditablePair = groupConnection
                            .rotationAfterStepAngles
                            .First(p => p.state == state);
                    GUILayout.Label(currentEditablePair.rotationAfterStepAngle.ToString());

                    var isEditable = TourEditor.ViewDirectionRenderer.CurrentEditableObject == groupConnection;

                    if (GUILayout.Toggle(isEditable, "edit", "Button") != isEditable)
                    {
                        if (isEditable)
                        {
                            TourEditor.ViewDirectionRenderer.ClearEditing();
                        }
                        else
                        {
                            TourEditor.ViewDirectionRenderer.SetEditing(
                                                        state,
                                                        () => currentEditablePair.rotationAfterStepAngle,
                                                        angle =>
                                                        {
                                                            currentEditablePair.rotationAfterStepAngle = angle;
                                                        },
                                                        groupConnection
                                                    );
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

        }

        private static void StartEditRotationOverride(State state, Connection connection)
        {
            TourEditor.ViewDirectionRenderer.SetEditing(
                                            state,
                                            () => connection.rotationAfterStepAngle,
                                            angle => connection.rotationAfterStepAngle = angle,
                                            connection
                                        );
        }
    }
}
