using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallDetection : MonoBehaviour
{
    PlayerController player;

    bool isCurrentlyOnWall = false;

    bool isLeft = false;

    private void Start()
    {
        player = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        player.SetTouchedWall(true);
        isCurrentlyOnWall = true;

        CheckWallDirection(other);
    }

    private void OnTriggerStay(Collider other)
    {
        isCurrentlyOnWall = true;

        if (!player.IsOnWall())
        {
            player.SetTouchedWall(true);
            CheckWallDirection(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        isCurrentlyOnWall = false;
        player.SetTouchedWall(false);
    }

    public bool IsCurrentlyOnWall()
    {
        return isCurrentlyOnWall;
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

    public bool IsWallLeft()
    {
        return isLeft;
    }
}
