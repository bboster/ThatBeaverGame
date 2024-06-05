using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyPlayerCard : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField]
    GameObject waitingForPlayerPanel;
    [SerializeField]
    GameObject playerDataPanel;

    [Header("Data Display")]
    [SerializeField]
    TMP_Text playerDisplayNameText;
    [SerializeField]
    Image playerDisplayImage;
    [SerializeField]
    ToggleText isReadyToggle;
}
