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

        ResetStoredWalls();
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

            currentWall = new(wall.gameObject, wallHit.normal);
        }
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
        if (previousWall == null || 
            currentWall == null)
            return false;

        if (!ReferenceEquals(previousWall.Object, currentWall.Object))
            return false;

        bool isPrevious;

        if (onlyCompareWallNormals)
            isPrevious = previousWall.Normal == currentWall.Normal;
        else
            isPrevious = previousWall.Equals(currentWall);

        //Debug.Log("Current Wall Normal: " + currentWall.Normal + " | Previous Wall Normal: " + previousWall.Normal);

        return isPrevious;
    }

    public void ResetStoredWalls()
    {
        currentWall = null;
        previousWall = null;
    }
}

public class WallContainer{
    public Vector3 Normal { get; private set; }

    public GameObject Object { get; private set; }

    public WallContainer(GameObject obj, Vector3 normal)
    {
        Object = obj;
        Normal = normal;
    }
}
