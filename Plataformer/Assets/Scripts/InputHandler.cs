using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputHandler : MonoBehaviour
{
    public static event Action<Vector2> OnMove;
    public static event Action OnJump;
    public static event Action OnDash;
    public static event Action<Vector2> OnLook;
    
    private InputSystem_Actions inputSystem;

    private void Awake()
    {
        Debug.Log("[InputHandler] Awake");
        inputSystem = new InputSystem_Actions();
        
        inputSystem.Enable();
    }

    private void OnEnable()
    {
        
        Debug.Log("[InputHandler] OnEnable");
        inputSystem.Player.Move.performed += MovePerformed;
        inputSystem.Player.Move.canceled += MoveCanceled;
        
        inputSystem.Player.Look.performed += LookPerformed;
        inputSystem.Player.Look.canceled += LookCanceled;
        
        inputSystem.Player.Jump.performed += JumpPerformed;
        inputSystem.Player.Attack.performed += DashPerformed;
    }

    private void OnDisable()
    {        
        Debug.Log("[InputHandler] OnDisable");
        inputSystem.Player.Move.performed   -= LookPerformed;
        inputSystem.Player.Move.canceled    -= LookCanceled;
        
        inputSystem.Player.Look.performed -= MovePerformed;
        inputSystem.Player.Look.canceled -= MoveCanceled;
        
        inputSystem.Player.Jump.performed   -= JumpPerformed;
        inputSystem.Player.Attack.performed   -= DashPerformed;

        inputSystem.Disable();
    }

    //Handlers
    private void MovePerformed(InputAction.CallbackContext ctx)
    {
        Vector2 move = ctx.ReadValue<Vector2>();
        //Debug.Log($"[InputHandler] MovePerformed → {move}");    
        OnMove?.Invoke(move);
    } 
    
    private void MoveCanceled(InputAction.CallbackContext ctx)=> OnMove?.Invoke(Vector2.zero);
    
    private void LookPerformed(InputAction.CallbackContext ctx)=> OnLook?.Invoke(ctx.ReadValue<Vector2>());
    
    private void LookCanceled(InputAction.CallbackContext ctx)=> OnMove?.Invoke(Vector2.zero);
    
    private void JumpPerformed(InputAction.CallbackContext ctx)=> OnJump?.Invoke();
    
    private void DashPerformed(InputAction.CallbackContext ctx)=> OnDash?.Invoke();
    
    
    
}
