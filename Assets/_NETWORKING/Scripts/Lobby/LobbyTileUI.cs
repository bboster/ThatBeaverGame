using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyTileUI
{
    public string lobbyName;
    public string lobbyHost;

    public bool joinable;

    public int numPlayersJoined;
    public int numPlayersMax;

    public LobbyTileUI(string name, string host, bool joinable, int pJoined, int pMax)
    {
        lobbyName = name;
        lobbyHost = host;
        this.joinable = joinable;
        numPlayersJoined = pJoined;
        numPlayersMax = pMax;
    }
}
