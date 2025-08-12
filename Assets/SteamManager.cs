using UnityEngine;
using Steamworks;
using Steamworks.Data;
using TMPro;
using System;
using System.Threading.Tasks;
using UnityEngine.UI;

public class SteamManager : MonoBehaviour
{

    public TMP_InputField input;

    public TextMeshProUGUI lobbyIDText;
    public GameObject lobbyUI;
    public GameObject mainMenuUI;

    public GameObject playerCardPrefab;
    public Transform playerLobbyParent;

    ulong lobbyID;


    void Awake()
    {
        lobbyUI.SetActive(false);
        mainMenuUI.SetActive(true);
    }

    void OnEnable()
    {
        SteamMatchmaking.OnLobbyCreated += LobbyCreated;
        SteamMatchmaking.OnLobbyEntered += LobbyEntered;
        SteamMatchmaking.OnLobbyMemberLeave += LobbyLeft;
        SteamFriends.OnGameLobbyJoinRequested += GameLobbyJoinRequested;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;

    }

    private void LobbyLeft(Lobby lobby, Friend friend)
    {
        for (int i = 0; i < playerLobbyParent.childCount; i++)
        {
            LobbyPlayerCard lobbyPlayerCard = playerLobbyParent.GetChild(i).GetComponent<LobbyPlayerCard>();
            if (friend.Id == lobbyPlayerCard.GetPlayerID())
            {
                Destroy(playerLobbyParent.GetChild(i).gameObject);
            }
        }
    }

    void OnDisable()
    {
        SteamMatchmaking.OnLobbyCreated -= LobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= LobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested -= GameLobbyJoinRequested;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
    }

    private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
    {
        AddPlayerCard(friend);
    }

    private async void GameLobbyJoinRequested(Lobby lobby, SteamId id)
    {
        await lobby.Join();
    }

    private void LobbyEntered(Lobby lobby)
    {
        LobbyHolder.Instance.currentLobby = lobby;
        lobbyID = lobby.Id.Value;
        lobbyIDText.text = "Lobby: " + lobbyID.ToString();
        mainMenuUI.SetActive(false);
        lobbyUI.SetActive(true);

        foreach (var member in lobby.Members)
        {
            Debug.Log(member.Id);
            AddPlayerCard(member);
        }
    }

    private async void AddPlayerCard(Friend friend)
    {
        GameObject spawnedPlayerCard = Instantiate(playerCardPrefab, playerLobbyParent);
        LobbyPlayerCard lobbyPlayerCard = spawnedPlayerCard.GetComponent<LobbyPlayerCard>();
        lobbyPlayerCard.SetPlayerID(friend.Id);
        // Set name
        lobbyPlayerCard.UpdateName(friend.Name);

        // Get avatar
        var avatar = await friend.GetLargeAvatarAsync();
        if (avatar.HasValue)
        {
            Texture2D tex = new Texture2D((int)avatar.Value.Width, (int)avatar.Value.Height, TextureFormat.RGBA32, false);
            tex.LoadRawTextureData(avatar.Value.Data);
            tex.Apply();
            lobbyPlayerCard.UpdateAvatar(tex);
        }
    }

    private void LobbyCreated(Result result, Lobby lobby)
    {
        if (result == Result.OK)
        {
            lobby.SetPublic();
            lobby.SetJoinable(true);
        }
    }

    public void CopyLobbyID()
    {
        TextEditor editor = new TextEditor();
        editor.text = lobbyID.ToString();
        editor.SelectAll();
        editor.Copy();
    }

    public async void HostLobby()
    {
        await SteamMatchmaking.CreateLobbyAsync(10);
    }

    public async void JoinLobbyWithID()
    {
        ulong ID;
        if (!ulong.TryParse(input.text, out ID)) return;

        Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).RequestAsync();

        foreach (var lobby in lobbies)
        {
            if (lobby.Id == ID)
            {
                await lobby.Join();
                return;
            }
        }
    }

    public void LeaveLobby()
    {
        LobbyHolder.Instance.currentLobby.Leave();
        lobbyUI.SetActive(false);
        mainMenuUI.SetActive(true);
        for (int i = 0; i < playerLobbyParent.childCount; i++)
        {
            Destroy(playerLobbyParent.GetChild(i).gameObject);
        }
    }
}
