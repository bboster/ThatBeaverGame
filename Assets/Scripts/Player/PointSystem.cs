using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PointSystem : MonoBehaviour
{
    [SerializeField] private int currentPoints;
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] private TMP_Text pointsGameOver;
    [SerializeField] private FractureableSO fracture;
    [SerializeField] private GameObject plusOne;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        pointsText.text = "Points: " + currentPoints.ToString();
        pointsGameOver.text = "Points: " + currentPoints.ToString();
    }

    public void AddPoint(int point)
    {
        currentPoints += fracture.pointsToAward;
    }
}
