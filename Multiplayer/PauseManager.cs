using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject _pausePannel;

    private void Start()
    {
        _pausePannel.SetActive(false);
    }

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.Escape))
        {
            _pausePannel.SetActive(!_pausePannel.activeInHierarchy);
            EventBus.escapeClicked?.Invoke(!_pausePannel.activeInHierarchy);
        }
    }

    public void _Return()
    {
        _pausePannel.SetActive(false);
        EventBus.escapeClicked?.Invoke(!_pausePannel.activeInHierarchy);
    }

    public void _Exit()
    {
        NicknamesHolder.userNames.Remove(PhotonNetwork.playerName);
        CheckPlayers();
        PhotonNetwork.LoadLevel("Menu");
        PhotonNetwork.LeaveRoom();
    }

    private void CheckPlayers()
    {
        if(PhotonNetwork.countOfPlayersInRooms < 1)
        {
            RenderSettings.skybox = default;
            StaticHolder.playersPresent = false;
        }
    }
}
