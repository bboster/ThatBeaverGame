using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallDetection : MonoBehaviour
{
    [Header("Previous Wall Checking")]
    [SerializeField]
    bool onlyCompareWallNormals = false;
    //[SerializeField]
    //float wallDifferenceThreshold = 0.1f;

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
        /*if (currentWall == null)
            currentWall = new(other.transform.position);
        else
            currentWall.SetWallPosition(other.transform.position);*/

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
        CheckWallDirection(wall);
        player.SetTouchedWall(true);
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

            /*
            if (currentWall == null)
                currentWall = new(wallNormal, wall.transform.position);
            else
                currentWall?.SetWallNormal(wallNormal);
            */

            currentWall = new(wall.gameObject, wallHit.normal);
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
        //Debug.Log("Current Wall: " + !(currentWall == null));
        //Debug.Log("Null Check...");
        if (previousWall == null || 
            currentWall == null)
            return false;

        //Debug.Log("Checking Colliders...");
        if (!ReferenceEquals(previousWall.Object, currentWall.Object))
            return false;

        bool isPrevious;

        if (onlyCompareWallNormals)
            isPrevious = previousWall.Normal == currentWall.Normal;
        else
            isPrevious = previousWall.Equals(currentWall);

        //Debug.Log("Current Wall Normal: " + currentWall.Normal + " | Previous Wall Normal: " + previousWall.Normal);
        /*int currentTri = currentWall.HitTriangleIdx;
        int previousTri = previousWall.HitTriangleIdx;

        isPrevious = currentTri == previousTri;*/

        //Debug.Log("Current Wall Tri: " + currentTri + " | Previous Wall Tri: " + previousTri);

        return isPrevious;

        /*
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
        return isPrevious;*/
    }

    public void ResetStoredWalls()
    {
        currentWall = null;
        previousWall = null;
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

    public int HitTriangleIdx { get; private set; }

    public GameObject Object { get; private set; }

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

    public WallContainer(GameObject obj, int hitTriangle)
    {
        Object = obj;
        HitTriangleIdx = hitTriangle;
    }

    public WallContainer(GameObject obj, Vector3 normal)
    {
        Object = obj;
        Normal = normal;
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
