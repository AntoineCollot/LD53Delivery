using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerState : MonoBehaviour
{
    public static PlayerState Instance;
    public CompositeState freezeInputsState;
    public CompositeState freezePositionState;
    public CompositeState freezeGroundDetectionState;
    public CompositeState forceDefaultGravityState;
    Rigidbody2D body;

    private void Awake()
    {
        freezeInputsState = new CompositeState();
        freezePositionState = new CompositeState();
        freezeGroundDetectionState = new CompositeState();
        forceDefaultGravityState = new CompositeState();

        Instance = this;

        body = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (freezePositionState.IsOn)
            body.constraints = RigidbodyConstraints2D.FreezeAll;
        else
        {
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.WakeUp();
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(PlayerState), true)]
public class PlayerStateEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlayerState behaviour = target as PlayerState;

        if (behaviour.freezeInputsState != null)
            EditorGUILayout.Toggle("Freeze Inputs", behaviour.freezeInputsState.IsOn);
        if (behaviour.freezePositionState != null)
            EditorGUILayout.Toggle("Freeze Position", behaviour.freezePositionState.IsOn);

        // Write back changed values
        // This also handles all marking dirty, saving, undo/redo etc
        serializedObject.ApplyModifiedProperties();
    }
}
#endif