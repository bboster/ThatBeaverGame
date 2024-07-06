using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;

public class ServerListManager : SteamworksManager
{
    private const string BeaverKey = "BeaverGame";
    private const string BeaverValue = "3876";

    private Lobby[] availableLobbies;

    private List<LobbyTileUI> availableLobbyUI = new List<LobbyTileUI>();

    private async void Start()
    {
        await GenerateLobbyList();
        await CreateListUI();
    }

    private async Task<bool> GenerateLobbyList()
    {
        LobbyQuery query = SteamMatchmaking.LobbyList.WithSlotsAvailable(1).WithKeyValue(BeaverKey, BeaverValue).FilterDistanceFar();
        availableLobbies = await query.RequestAsync();

        if (availableLobbies.Length == 0)
        {
            Debug.Log("No Available Lobbies... Expanding Search...");
            query = SteamMatchmaking.LobbyList.WithSlotsAvailable(1).WithKeyValue(BeaverKey, BeaverValue);
            availableLobbies = await query.RequestAsync();
        }

        if (availableLobbies.Length == 0)
        {
            Debug.Log("No Available Lobbies Worldwide!");
            return false;
        }

        Debug.Log("Available Lobbies: " + availableLobbies.Length);
        Debug.Log(availableLobbies[0].Owner.Name);
        return true;
    }

    /// <summary>
    /// Creates the UI tiles of lobbies available to join.
    /// </summary>
    /// <returns></returns>
    private Task CreateListUI()
    {
        //Loop through each entry in available lobbies and create an entry in the list.
        foreach(Lobby l in availableLobbies)
        {
            LobbyTileUI t = new LobbyTileUI(l.Id.Value.ToString(), l.Owner.Name, true, l.MemberCount, l.MaxMembers);

            availableLobbyUI.Add(t);
            Debug.Log(t.lobbyHost);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates displayed server info in server list every second.
    /// </summary>
    /// <returns></returns>
    private IEnumerator UpdateExistingServerInfo()
    {
        for(; ; )
        {
            //Loop through existing servers and update their info.

            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// Called when the refresh button is clicked.
    /// </summary>
    /// <returns></returns>
    public async Task<bool> InfoRefresh()
    {
        //Loop through each entry in available lobbies and update server info in UI.

        return true;
    }
}
