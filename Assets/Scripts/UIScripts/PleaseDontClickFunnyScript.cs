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
