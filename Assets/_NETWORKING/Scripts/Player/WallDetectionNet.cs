using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class WallDetectionNet : NetworkBehaviour
{
    [Header("Wall Exclusion")]
    [SerializeField]
    List<string> excludedTags = new();

    [SerializeField]
    float minWallHeight = 0.1f;

    [Header("Left Check")]
    [SerializeField]
    float leftMinAngle = 40;
    [SerializeField]
    float leftMaxAngle = 150;

    [Header("Previous Wall Checking")]
    [SerializeField]
    bool onlyCompareWallNormals = false;
    //[SerializeField]
    //float wallDifferenceThreshold = 0.1f;

    // Player Assignments
    PlayerControllerNet player;

    bool isCurrentlyOnWall = false;

    // Wall Assignments
    bool isLeft = false;

    Vector3 wallNormal;

    RaycastHit wallHit;

    WallContainer currentWall;

    WallContainer previousWall;

    private void Start()
    {
        player = GetComponentInParent<PlayerControllerNet>();

        ResetStoredWalls();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer)
            return;

        if (excludedTags.Contains(other.tag))
            return;

        OnWallTouch(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isLocalPlayer)
            return;

        if (excludedTags.Contains(other.tag))
            return;

        if (!player.IsOnWall() || wallNormal == Vector3.zero)
        {
            OnWallTouch(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isLocalPlayer)
            return;

        OnWallLeave();
        previousWall = currentWall;
        currentWall = null;
    }

    private void OnWallTouch(Collider wall)
    {
        if (wall.isTrigger)
            return;

        Renderer wallRenderer = wall.GetComponent<Renderer>();
        if (wallRenderer == null)
            return;

        Bounds wallBounds = wallRenderer.bounds;
        if (wallBounds.center.y + wallBounds.extents.y * wall.transform.lossyScale.y < minWallHeight)
            return;

        CheckWallDirection(wall);
        player.SetTouchedWall(true);


        //Debug.Log(wallBounds.center.y + wallBounds.extents.y * wall.transform.lossyScale.y);
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

        isLeft = angle < leftMaxAngle && angle > leftMinAngle;

        bool didHit = Physics.Raycast(transform.position, transform.right * (isLeft ? -1 : 1), out wallHit, 2);
        if (!didHit)
            wallNormal = Vector3.zero;
        else
        {
            wallNormal = wallHit.normal;

            currentWall = new(wall.gameObject, wallHit.normal);
        }

        //Debug.Log("Wall Left: " + isLeft);
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
