using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ValidateUI : MonoBehaviour
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
        inputs.Gameplay.ValidateUI.performed += OnValidateUI;
    }

    private void OnValidateUI(InputAction.CallbackContext obj)
    {
        GetComponent<Button>().onClick.Invoke();
    }

    protected virtual void OnDisable()
    {
        inputs.Disable();
        inputs.Gameplay.ValidateUI.performed -= OnValidateUI;
    }
}
