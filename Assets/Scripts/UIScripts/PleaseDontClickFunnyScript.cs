using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PleaseDontClickFunnyScript : MonoBehaviour
{
    [Header("Blocker Image Game Object")]
    [SerializeField] private Image blockerImage;

    [Header("Blocker Images")]
    [SerializeField] private Sprite original;
    [SerializeField] private Sprite damaged1;
    [SerializeField] private Sprite damaged2;

    [Header("Reference")]
    public UIButtonShake shake;

    private int damageTickCount = 0;

    // Depending on how many times the player has clicked the cover, it will
    // break more and more depending on damageTickCount. If it reaches 3,
    // the cover is broken forever (until the scene reloads).
    public void damage()
    { 
        if (damageTickCount == 0)
        {
            blockerImage.GetComponent<Image>().sprite = damaged1;
            damageTickCount++;
        }
        else if (damageTickCount == 1)
        {
            blockerImage.GetComponent<Image>().sprite = damaged2;
            damageTickCount++;
        }
        else
        {
            Destroy(this.gameObject);
            Debug.Log("'Stop! Stop! He's Already Dead!'");
        }
    }
}
