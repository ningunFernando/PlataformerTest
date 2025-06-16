using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    private InputSystem_Actions inputSystem;
    
    private Vector2 moveInput;
    private bool dashInput;
    private bool jumpInput;
    private void Awake()
    {
        inputSystem = new InputSystem_Actions();
    }

    void Start()
    {
        Debug.Log("Si va jalando");
    }

    private void OnEnable()
    {
        inputSystem.Enable();
        inputSystem.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputSystem.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        
        inputSystem.Player.Jump.performed += ctx => jumpInput = true;
        inputSystem.Player.Jump.canceled += ctx => jumpInput = false;
    }

    private void OnDisable()
    {
       inputSystem.Disable();
    }


    void Update()
    {
        if (moveInput != Vector2.zero)
        {
            Debug.Log("Si va jalando");
        }

        if (jumpInput)
        {
            Debug.Log("Salto");
        }
    }
}
