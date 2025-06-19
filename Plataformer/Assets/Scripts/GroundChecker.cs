using System;
using UnityEngine;

[ExecuteInEditMode]
public class GroundChecker : MonoBehaviour
{
    public static event Action OnGroundEnter;
    public static event Action OnGroundExit;
    
    [Header("Ground Check Settings")]
    public Transform groundCheckPoint;
    public LayerMask groundLayer;
    public float checkDistance;
    
    private bool wasGrounded;

    private void FixedUpdate()
    {
        RaycastHit hitInfo;
        bool isGrounded = Physics.Raycast(groundCheckPoint.position, 
            Vector3.down, out hitInfo, 
            checkDistance, groundLayer);
        
        Debug.DrawRay(
            groundCheckPoint.position,
            Vector3.down * checkDistance,
            isGrounded ? Color.green : Color.red
        );
        
        
        if (isGrounded)
        {
            Debug.Log($"[GroundChecker] Touching Floor");
        }
        else
        {
            Debug.Log("[GroundChecker] in the air");
        }
        

        if (isGrounded && !wasGrounded)
        {
            Debug.Log("[GroundChecker] OnGroundEnter Invoked");
            OnGroundEnter?.Invoke();
        }

        if (!isGrounded && wasGrounded)
        {
            Debug.Log("[GroundChecker] OnGroundExit  Invoked");
            OnGroundExit?.Invoke();
        }
        
        wasGrounded = isGrounded;
    }

    private void OnDrawGizmosSelected()
    {
       
    }
}

