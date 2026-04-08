using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    private float moveSpeed;
    // Distance when move component think we are on target position (needed for smooth walking)
    private float reachTargetDistance;
    // Distance when move component actually stops movement
    private float stopDistance;
    
    private bool movingToTarget;
    
    private Vector3 targetPosition;
    public Vector3? NextTargetPosition;
    
    public bool ReachedTarget { get; private set; }

    public void Setup(float speed, float stopDist,  float reachDist)
    {
        moveSpeed = speed;
        stopDistance = stopDist;
        reachTargetDistance = reachDist;
        
        ReachedTarget = false;
        movingToTarget = false;
    }

    public void MoveTo(Vector3 position)
    {
        targetPosition = position;
        movingToTarget = true;
        ReachedTarget = false;
    }

    public void Stop()
    {
        movingToTarget = false;
        ReachedTarget = false;
    }

    void Update()
    {
        if (!movingToTarget) return;

        Vector3 delta = targetPosition - transform.position;
        float distance = delta.magnitude;

        /*
         // TODO return when need smooth moving
         if (distance <= reachTargetDistance)
        {
            if (NextTargetPosition != null)
            {
                targetPosition = NextTargetPosition.Value;
                NextTargetPosition = null;
            } 
        }*/
        
        if (distance <= stopDistance)
        {
            ReachPoint();
            return;
        }

        transform.position += delta.normalized * (moveSpeed * Time.deltaTime);
    }
    
    private void ReachPoint()
    {
        movingToTarget = false;
        ReachedTarget = true;
    }
}
