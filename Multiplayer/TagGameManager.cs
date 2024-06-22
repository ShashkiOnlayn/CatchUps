using Photon;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class TagGameManager : PunBehaviour, IPunObservable
{
    public static int randomPlayerIndex;

    [SerializeField] private TMP_Text _countdownText;

    private PhotonView _view;
    private bool _alreadyStarted;
    private GameObject _thirdPlace;

    private void Start()
    {
        _view = GetComponent<PhotonView>();

        var pedestal = GameObject.FindGameObjectWithTag("Pedestal");
        var nicks = pedestal.GetComponentsInChildren<TextMeshPro>(true);
        _thirdPlace = nicks[2].gameObject.transform.parent.parent.gameObject;
        Debug.LogError("-----------" + _thirdPlace.name);

        Debug.LogError("Player connected");
        CheckPlayerCount();
    }

    private void Update()
    {
        if (_alreadyStarted == false)
        {
            if (PhotonNetwork.isMasterClient)
            {
                if (Input.GetKeyDown(KeyCode.P))
                {
                    _alreadyStarted = true;
                    StartCoroutine(StartCountdown());
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (PhotonNetwork.room.PlayerCount == 0)
        {
            Debug.LogError(nameof(TagGameManager) + " reseted");
        }
        print("Destroyed");
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            player.GetComponent<CatchUpController>().players = FindObjectsOfType<CatchUpController>();
        }

        CheckPlayerCount();
        Debug.LogError("Player connected and checked");
        Debug.LogError(newPlayer.ID);
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            player.GetComponent<CatchUpController>().players = FindObjectsOfType<CatchUpController>();
        }

        if (GameObject.FindGameObjectsWithTag("Player").Length == 1)
        {
            _alreadyStarted = false;
            StopCoroutine(StartCountdown());
        }
    }

    private void CheckPlayerCount()
    {
        if (PhotonNetwork.room.PlayerCount == StaticHolder.maxPlayers)
        {
            if (PhotonNetwork.isMasterClient)
                PhotonNetwork.room.IsOpen = false;

            StartCoroutine(StartCountdown());
        }
    }

    private IEnumerator StartCountdown()
    {
        photonView.RPC(nameof(SetPlayers), PhotonTargets.All);

        int countdown = 10;
        while (countdown > 0)
        {
            _countdownText.text = countdown.ToString();
            yield return new WaitForSeconds(1);
            countdown--;
        }

        _countdownText.text = "GO!";

        if (PhotonNetwork.isMasterClient)
        {
            if (PhotonNetwork.room.IsOpen)
                PhotonNetwork.room.IsOpen = false;

            if (PhotonNetwork.room.PlayerCount == 2)
            {
                _thirdPlace.SetActive(false);
            }

            var catchUpController = (PhotonNetwork.masterClient.TagObject as GameObject).GetComponent<CatchUpController>();
            var masterClientView = catchUpController.view;

            masterClientView.RPC(nameof(catchUpController.GetRandomPlayer), PhotonNetwork.masterClient);
            photonView.RPC(nameof(SetCurrentTagger), PhotonTargets.All, StaticHolder.currentTagger.ID);
        }

        photonView.RPC(nameof(SetValues), PhotonTargets.All);
    }

    [PunRPC]
    private void SetValues()
    {
        StaticHolder.players.ForEach(p =>
        {
            var pView = p.GetComponent<PhotonView>();
            var pCatchUp = p.GetComponent<CatchUpController>();

            if (pView.owner.ID == StaticHolder.currentTagger.ID)
            {
                pView.RPC(nameof(pCatchUp.SetTagger), pView.owner, pView.owner, true);
                pView.RPC(nameof(pCatchUp.UpdateNickColor), PhotonTargets.All, pView.owner, 1, 0, 0); // красный
            }
            else
            {
                pView.RPC(nameof(pCatchUp.SetTagger), pView.owner, pView.owner, false);
                pView.RPC(nameof(pCatchUp.UpdateNickColor), pView.owner, pView.owner, 1, 1, 1); // белый
            }
        });
    }

    [PunRPC] private void SetPlayers() => StaticHolder.players = GameObject.FindGameObjectsWithTag("Player").ToList();

    [PunRPC]
    public void SetCurrentTagger(int taggerID)
    {
        foreach (var p in StaticHolder.players)
        {
            if (p.GetComponent<PhotonView>().owner.ID == taggerID)
            {
                StaticHolder.currentTagger = p.GetComponent<PhotonView>().owner;
                StaticHolder.currentTagger.TagObject = p.GetComponent<PhotonView>().owner.TagObject;
                break;
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(_countdownText.text);
            stream.SendNext(_thirdPlace.active);
        }
        else
        {
            _countdownText.text = (string)stream.ReceiveNext();
            _thirdPlace.active = (bool)stream.ReceiveNext();
        }
    }
}