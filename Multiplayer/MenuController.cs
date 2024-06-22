using Photon;
using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : PunBehaviour, IPunObservable
{
    [SerializeField] private string _version = "1";

    [Header("UserName")]
    [SerializeField] private byte _minSymbolsCount = 3;
    [SerializeField] private byte _maxSymbolsCount = 10;

    [Header("Input fields")]
    [SerializeField] private TMP_InputField _createRoom;
    [SerializeField] private TMP_InputField _maxPlayers;
    [SerializeField] private TMP_InputField _joinRoom;
    [SerializeField] private TMP_InputField _userName;

    [Header("Buttons")]
    [SerializeField] private Button _showCreateRoomButton;
    [SerializeField] private Button _createRoomButton;
    [SerializeField] private Button _joinRoomButton;

    [Header("Panels")]
    [SerializeField] private GameObject _createRoomPannel;

    private string _trimmedUserName;

    private StringBuilder sb = new();

    private void Start()
    {
        if (PhotonNetwork.connectedAndReady == false || PhotonNetwork.connected == false)
        {
            PhotonNetwork.ConnectUsingSettings(_version); 
        }

         PhotonNetwork.playerName = LoadNickName();

        if (PhotonNetwork.playerName != null)
        {
            _trimmedUserName = _userName.text = PhotonNetwork.playerName;
            CheckName();
        }

        if (_userName.text is not null or "" or "Enter username...")
        {
            _showCreateRoomButton.interactable = _createRoom.interactable =
                _joinRoomButton.interactable = _joinRoom.interactable = true;
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(TypedLobby.Default);
        Debug.LogError("Подключаемся");
    }

    public void CheckName()
    {
        if (_userName.text != null)
        {
            PhotonNetwork.playerName = _trimmedUserName = _userName.text = _userName.text.Trim().Replace(' ', '_');
        }

        if (_trimmedUserName is not ("" or "Enter username..."))
        {
            if (_minSymbolsCount <= _trimmedUserName.Length && _trimmedUserName.Length <= _maxSymbolsCount)
            {
                if (NicknamesHolder.userNames.Contains(_trimmedUserName) == false)
                {
                    _showCreateRoomButton.interactable = _createRoom.interactable =
                        _joinRoomButton.interactable = _joinRoom.interactable = true;
                }
            }
            else
            {
                PhotonNetwork.playerName = string.Empty;

                _showCreateRoomButton.interactable = _createRoom.interactable =
                    _joinRoomButton.interactable = _joinRoom.interactable = false;
            }
        }
        else
        {
            PhotonNetwork.playerName = string.Empty;

            _showCreateRoomButton.interactable = _createRoom.interactable =
                _joinRoomButton.interactable = _joinRoom.interactable = false;
        }

         SaveNickName(_trimmedUserName);
    }

    public void ShowOrHideCreateRoom(bool enable)
    {
        _createRoomPannel.SetActive(enable);
    }

    public void SetMaxPlayers()
    {
        for (int i = 0; i < _maxPlayers.text.Length; i++)
        {
            if (char.IsDigit(_maxPlayers.text[i]) == false)
            {
                _maxPlayers.text = "";
                _createRoomButton.interactable = false;
                return;
            }
        }

        if (int.TryParse(_maxPlayers.text, out int result))
        {
            if (result > 20 || result < 1)
            {
                _maxPlayers.text = "";
                _createRoomButton.interactable = false;
                return;
            }
        }

        StaticHolder.maxPlayers = (byte)result;
        _createRoomButton.interactable = true;
    }

    public void CreateGame()
    {
        if (_maxPlayers.text.Trim() == "1")
        {
            _maxPlayers.text = "";
            _createRoomButton.interactable = false;
            StaticHolder.maxPlayers = 0;
            return;
        }

        if (_createRoom.text.Trim() is not null and not ("Enter text..." or ""))
        {
            PhotonNetwork.playerName = _trimmedUserName;

            if (NicknamesHolder.userNames.Contains(_trimmedUserName) == false)
            {
                NicknamesHolder.userNames.Add(_trimmedUserName);
            }

            PhotonNetwork.CreateRoom(_createRoom.text, new RoomOptions { maxPlayers = StaticHolder.maxPlayers, CleanupCacheOnLeave = true, EmptyRoomTtl = 0 }, TypedLobby.Default);
            Debug.Log("Комната создана");
        }
    }

    public void JoinGame()
    {
        if (_joinRoom.text.Trim() is not null and not ("Enter text..." or ""))
        {
            PhotonNetwork.playerName = _trimmedUserName;

            if (NicknamesHolder.userNames.Contains(_trimmedUserName) == false)
            {
                NicknamesHolder.userNames.Add(_trimmedUserName);
            }

            PhotonNetwork.JoinRoom(_joinRoom.text);
            Debug.Log("Подключен к комнате");
        }
    }

    public override void OnJoinedRoom()
    {
        try
        {
            PhotonNetwork.LoadLevel("Game");
        }
        catch (Exception e) { print(e); }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            for (int i = 0; i < NicknamesHolder.userNames.Count; i++)
            {
                sb.Append(NicknamesHolder.userNames[i] + " ");
            }

            stream.SendNext(sb.ToString());
            stream.SendNext(StaticHolder.maxPlayers);
        }
        else
        {
            string nicks = (string)stream.ReceiveNext();
            string[] arrayNicks = nicks.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < NicknamesHolder.userNames.Count; i++)
            {
                NicknamesHolder.userNames[i] = arrayNicks[i];
            }

            StaticHolder.maxPlayers = (byte)stream.ReceiveNext();
        }
    }

    public void QUIT() => Application.Quit();

     private void OnApplicationQuit() => SaveNickName(_trimmedUserName);
    
     private void SaveNickName(string nickName) => PlayerPrefs.SetString("Nick", nickName);
    
     private string LoadNickName() => PlayerPrefs.GetString("Nick");
}
