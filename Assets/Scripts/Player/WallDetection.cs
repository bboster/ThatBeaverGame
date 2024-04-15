using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallDetection : MonoBehaviour
{
    PlayerController player;

    bool isCurrentlyOnWall = false;

    bool isLeft = false;

    Vector3 wallNormal;

    RaycastHit wallHit;

    private void Start()
    {
        player = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        OnWallTouch(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!player.IsOnWall() || wallNormal == Vector3.zero)
        {
            OnWallTouch(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        OnWallLeave();
    }

    private void OnWallTouch(Collider wall)
    {
        player.SetTouchedWall(true);
        CheckWallDirection(wall);
    }

    private void OnWallLeave()
    {
        isCurrentlyOnWall = false;
        player.SetTouchedWall(false);
        wallNormal = Vector3.zero;
    }

    private void CheckWallDirection(Collider wall)
    {
        Vector3 direction = transform.position - wall.transform.position;

        float angle = Vector3.SignedAngle(transform.parent.forward, direction, Vector3.up);

        //Debug.Log("Wall Angle: " + angle);

        if (angle < -40 && angle > -150)
            isLeft = false;
        else
            isLeft = true;

        bool didHit = Physics.Raycast(transform.position, transform.right * (isLeft ? -1 : 1), out wallHit, 2);
        if (!didHit)
            wallNormal = Vector3.zero;
        else
            wallNormal = wallHit.normal;

        //Debug.Log("Wall Is Left: " + isLeft + " | Did Hit: " + didHit);
    }

    public bool IsCurrentlyOnWall()
    {
        return isCurrentlyOnWall;
    }

    public bool IsWallLeft()
    {
        return isLeft;
    }

    public Vector3 GetWallNormal()
    {
        return wallNormal;
    }

    /*private void OnDrawGizmos()
    {
        // Left / Right
        Gizmos.color = Color.black;
        Gizmos.DrawLine(transform.position, transform.right * (isLeft ? -1 : 1));
        // Wall Normal
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, wallNormal);

        // Negative Wall Normal
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, -wallNormal);

        // Cross
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, Vector3.Cross(wallNormal, Vector3.up));

        // Negative Normal in Cross
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, Vector3.Cross(-wallNormal, Vector3.up));

        // Negative Cross
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, -Vector3.Cross(wallNormal, Vector3.up));

    }*/
}
