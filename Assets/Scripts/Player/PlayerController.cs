using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerController : MonoBehaviour
{
    [Header("Inputs")]
    protected InputMap inputs;

    protected virtual void Awake()
    {
        inputs = new InputMap();
    }

    protected virtual void OnEnable()
    {
        inputs.Gameplay.Enable();
    }

    protected virtual void OnDisable()
    {
        inputs.Disable();
    }
}
