using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyTileData : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyName;
    [SerializeField] private TextMeshProUGUI hostName;
    [SerializeField] private TextMeshProUGUI curPlayers;
    [SerializeField] private TextMeshProUGUI joinable;

    public void CreateButton(string n, string h, string c, bool j)
    {
        lobbyName.text = n;
        hostName.text = h;
        curPlayers.text = c;

        if(j)
        {
            joinable.text = "Joinable";
            joinable.color = Color.green;
            GetComponent<Button>().interactable = true;
        }
        else
        {
            joinable.text = "Full";
            joinable.color = Color.red;
            GetComponent<Button>().interactable = false;
        }
    }
}
