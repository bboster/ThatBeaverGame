using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PointSystem : MonoBehaviour
{
    [SerializeField] private int currentPoints;
    [SerializeField] private TMP_Text pointsText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        pointsText.text = "Points: " + currentPoints.ToString();
    }

    public void AddPoint()
    {
        currentPoints++;
    }
}
