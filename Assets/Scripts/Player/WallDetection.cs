using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallDetection : MonoBehaviour
{
    PlayerController player;

    bool isCurrentlyOnWall = false;

    bool isLeft = false;

    Vector3 wallNormal;

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
        if (!player.IsOnWall())
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
        CalculateWallNormal(wall);
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

        if (angle < -45 && angle > -135)
            isLeft = true;
        else
            isLeft = false;
    }

    private void CalculateWallNormal(Collider wall)
    {
        Vector3 direction = player.transform.position - wall.transform.position;
        Vector3 left = Vector3.Cross(direction, Vector3.up).normalized;

        wallNormal = isLeft ? left : -left;
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
}
