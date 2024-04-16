using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallDetection : MonoBehaviour
{
    [Header("Previous Wall Checking")]
    [SerializeField]
    bool onlyCompareWallNormals = false;
    [SerializeField]
    float wallDifferenceThreshold = 0.1f;

    // Player Assignments
    PlayerController player;

    bool isCurrentlyOnWall = false;

    // Wall Assignments
    bool isLeft = false;

    Vector3 wallNormal;

    RaycastHit wallHit;

    WallContainer currentWall;

    WallContainer previousWall;

    private void Start()
    {
        player = GetComponentInParent<PlayerController>();

        currentWall = new();
        previousWall = new();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currentWall == null)
            currentWall = new(other.transform.position);
        else
            currentWall.SetWallPosition(other.transform.position);

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
        previousWall = currentWall;
        currentWall = null;
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
        {
            wallNormal = wallHit.normal;

            if (currentWall == null)
                currentWall = new(wallNormal, wall.transform.position);
            else
                currentWall?.SetWallNormal(wallNormal);
        }

        //Debug.Log("Wall Normal: " + wallNormal);
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

    public bool IsOnPreviousWall()
    {
        if (previousWall == null || previousWall.Normal == Vector3.zero || 
            currentWall == null || currentWall.Normal == Vector3.zero)
            return false;

        bool isPrevious;

        /*if (onlyCompareWallNormals)
            isPrevious = previousWall.Normal == currentWall?.Normal;
        else
            isPrevious = previousWall.Equals(currentWall);*/

        Vector3 currentWallId = currentWall.Normal + currentWall.Position;
        Vector3 previousWallId = previousWall.Normal + previousWall.Position;
        Vector3 swappedCurrentWallId = VectorUtils.SwapXAndZ(currentWall.Normal) + currentWall.Position;

        if (onlyCompareWallNormals)
        {
            isPrevious = wallDifferenceThreshold < Mathf.Abs(Vector3.Distance(currentWallId, previousWallId));
            if (!isPrevious)
                isPrevious = wallDifferenceThreshold < Mathf.Abs(Vector3.Distance(swappedCurrentWallId, previousWallId));
        } 
        else
            isPrevious = previousWall.Equals(currentWall);

        Debug.Log("Current Wall Normal: " + currentWallId + " | Previous Wall Normal: " + previousWallId);
        //Debug.Log(isPrevious);
        return isPrevious;
    }

    public void ResetCurrentWall()
    {
        currentWall = null;
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

public class WallContainer{
    public Vector3 Normal { get; private set; }

    public Vector3 Position { get; private set; }

    public WallContainer(Vector3 wallNormal, Vector3 wallPosition)
    {
        Normal = wallNormal;
        Position = wallPosition;
    }

    public WallContainer(Vector3 wallPosition)
    {
        Position = wallPosition;
    }

    public WallContainer()
    {

    }

    public void SetWallNormal(Vector3 newNormal)
    {
        Normal = newNormal;
    }

    public void SetWallPosition(Vector3 newPosition)
    {
        Position = newPosition;
    }
}
