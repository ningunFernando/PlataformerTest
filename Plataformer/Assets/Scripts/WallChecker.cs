using System;
using UnityEngine;

public class WallChecker : MonoBehaviour
{
    public static event Action OnWallEnter;
    public static event Action OnWallExit;
    
    [Header("Wall Check Settings")]
    public Transform wallCheckPoint;
    public float checkDistance;
    public LayerMask wallLayer;
    
    public bool wasTouchingWall;

    private void FixedUpdate()
    {
            bool isTouchingWall = Physics.Raycast(wallCheckPoint.position, 
                transform.forward, out RaycastHit hit, 
                checkDistance, wallLayer);
            
            Debug.DrawRay(wallCheckPoint.position,
                transform.forward * checkDistance,
                isTouchingWall ? Color.green : Color.red);

            /*if (isTouchingWall)
            {
                Debug.Log($"[WallChecker] Wall entered: {hit.collider.gameObject.name} at {hit.distance}m");
            }
            else
            {
                Debug.Log("[WallChecker] Wall exited.");
            }*/

            if (isTouchingWall && !wasTouchingWall)
            {
                Debug.Log("[WallChecker] OnWallEnter Invoked");
                OnWallEnter?.Invoke();
            }

            if (!isTouchingWall && wasTouchingWall)
            {
                Debug.Log("[WallChecker] OnWallExit Invoked");
                OnWallExit?.Invoke();
            }
            
            wasTouchingWall = isTouchingWall;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (wallCheckPoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            wallCheckPoint.position,
            wallCheckPoint.position + transform.forward * checkDistance
        );
    }
}
