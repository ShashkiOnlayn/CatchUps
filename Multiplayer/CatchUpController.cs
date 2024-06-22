using System.Threading.Tasks;
using TMPro;
using UnityEngine;

using IEnumerator = System.Collections.IEnumerator;

[RequireComponent(typeof(PhotonView), typeof(FirstPersonController))]
public class CatchUpController : Photon.MonoBehaviour, IPunObservable
{
    [HideInInspector] public PhotonView view;

    public const byte MaxScoreToEndGame = 100;
    public const byte CaughtBonus = 5;

    public TMP_Text taggerText;
    public TMP_Text nickname;
    public bool isTagger = false;
    public int score = 0;
    public CatchUpController[] players;

    [SerializeField] private float _rayDistance;
    [SerializeField] private LeaderBord _leaderBord;

    [SerializeField] private GameObject _crown;

    [SerializeField] private Animator _endGameAnim;

    private Ray _ray;
    private string _nickColorHEX;
    private FirstPersonController _firstPersonController;
    private Camera m_Cam;
    private Camera _mainCamera;
    private bool _gameEnded = false;
    private bool _gameEndedFlag;
    private bool _cantTagSomeone; // нужен чтобы в конце нельзя было запятнать других
    private GameObject _pedestal;
    private Camera _finishCamera;
    private TextMeshPro[] _nicks;
    private GameObject _thirdPlace;

    private void Start()
    {
        view = GetComponent<PhotonView>();
        _firstPersonController = GetComponent<FirstPersonController>();
        m_Cam = _firstPersonController.playerCamera;

        _mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        _mainCamera.enabled = false;

        _pedestal = GameObject.FindGameObjectWithTag("Pedestal");
        _finishCamera = _pedestal.GetComponentInChildren<Camera>(true);
        _nicks = _pedestal.GetComponentsInChildren<TextMeshPro>(true);
        _thirdPlace = _nicks[2].gameObject.transform.parent.parent.gameObject;

        if (view.isMine)
        {
            _crown.SetActive(false);

            players = FindObjectsOfType<CatchUpController>();

            view.owner.TagObject = gameObject;
            nickname.text = PhotonNetwork.playerName;

            _leaderBord.gameObject.SetActive(false);
        }
        else
        {
            nickname.text = view.owner.name;
        }
    }

    private void Update()
    {
        if (_cantTagSomeone == false)
        {
            if (view.isMine)
            {
                _nickColorHEX = ConvertColorToHtml(nickname.color);

                _ray.origin = m_Cam.transform.position;
                _ray.direction = m_Cam.ScreenPointToRay(Input.mousePosition).direction;
                Debug.DrawRay(_ray.origin, _ray.direction * _rayDistance, Color.cyan);

                if (Input.GetMouseButtonDown(0))
                {
                    if (isTagger)
                    {
                        try
                        {
                            if (Physics.Raycast(_ray, out RaycastHit hit, _rayDistance))
                            {
                                var touchedPlayer = hit.collider.gameObject;

                                if (touchedPlayer.CompareTag("Player"))
                                {
                                    view.RPC(nameof(UpdateNickColor), view.owner, view.owner, 1, 1, 1);
                                    view.RPC(nameof(SetTagger), view.owner, view.owner, false);

                                    AddBonus(BonusType.Caught);
                                    CheckScore();

                                    var touchedPlayerCatchUp = touchedPlayer.GetComponent<CatchUpController>();
                                    var touchedPlayerView = touchedPlayer.GetComponent<PhotonView>();

                                    touchedPlayerView.owner.TagObject = touchedPlayer.gameObject;
                                    touchedPlayerView.RPC(nameof(touchedPlayerCatchUp.SetTagger), touchedPlayerView.owner, touchedPlayerView.owner, true);
                                    touchedPlayerView.RPC(nameof(touchedPlayerCatchUp.UpdateNickColor), touchedPlayerView.owner, touchedPlayerView.owner, 1, 0, 0);
                                }
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.Log(e);
                        }
                    }
                }
            }
            else
            {
                nickname.color = ConvertHtmlToColor(_nickColorHEX);
            }
        }
    }

    private void LateUpdate()
    {
        if (players.Length != PhotonNetwork.room.PlayerCount)
            players = FindObjectsOfType<CatchUpController>();
    }

    [PunRPC]
    public void GetRandomPlayer()
    {
        StaticHolder.randomPlayerIndex = Random.Range(0, PhotonNetwork.room.PlayerCount);
        StaticHolder.currentTagger = PhotonNetwork.playerList[StaticHolder.randomPlayerIndex];
        StaticHolder.players.ForEach(p =>
        {
            if (p.GetComponent<PhotonView>().owner.ID == StaticHolder.currentTagger.ID)
            {
                StaticHolder.currentTagger.TagObject = p;
            }
        });
    }

    [PunRPC]
    public void SetTagger(PhotonPlayer photonPlayer, bool enable)
    {
        if ((photonPlayer.TagObject as GameObject).TryGetComponent(out CatchUpController catchUpController))
        {
            catchUpController.taggerText.enabled = enable;
            catchUpController.isTagger = enable;
        }

        if (players.Length != PhotonNetwork.room.PlayerCount)
            players = FindObjectsOfType<CatchUpController>();

        _leaderBord.gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(AddScore());
    }

    [PunRPC]
    public void UpdateNickColor(PhotonPlayer player, int r, int g, int b)
    {
        if (view.isMine)
        {
            Color nickColor = new(r, g, b);

            if ((player.TagObject as GameObject).TryGetComponent(out CatchUpController catchUpController))
            {
                catchUpController.nickname.color = nickColor;
            }
        }
    }

    private IEnumerator AddScore()
    {
        while (_gameEnded == false)
        {
            if (isTagger == false)
                score++;

            _leaderBord.SetLeaderBord(players, _nicks);
            CheckScore();

            yield return new WaitForSeconds(2f);
        }
    }

    private void AddBonus(BonusType type)
    {
        switch (type)
        {
            case BonusType.Caught:
                if (_gameEnded == false)
                {
                    score += CaughtBonus;

                    _leaderBord.SetLeaderBord(players, _nicks);
                }
                break;
        }
    }

    public void SetCrownEnable(bool enable) => _crown.SetActive(enable);

    private void CheckScore()
    {
        if (score >= MaxScoreToEndGame || _gameEnded)
        {
            _gameEnded = true;
            _cantTagSomeone = true;
            photonView.RPC(nameof(EndGame), PhotonTargets.All);
        }
    }

    [PunRPC]
    private async void EndGame()
    {
        _gameEnded = true;
        _cantTagSomeone = true;

        var leaderBord = GetComponentInChildren<LeaderBord>(true);

        _firstPersonController.SetMovementEnable(false);
        leaderBord.gameObject.SetActive(false);
        taggerText.gameObject.SetActive(false);

        _endGameAnim.SetTrigger("gameEnded_show");

        await Task.Delay(600);
        _firstPersonController.SetMovementEnable(false);
        leaderBord.gameObject.SetActive(false);
        taggerText.gameObject.SetActive(false);
        m_Cam.gameObject.SetActive(false);
        _mainCamera.enabled = true;

        _endGameAnim.SetBool("gameEnded_hide", true);

        await Task.Delay(2000);
        leaderBord.gameObject.SetActive(false);
        taggerText.gameObject.SetActive(false);

        _endGameAnim.SetBool("gameEnded_hide", false);

        _endGameAnim.SetTrigger("gameEnded_show");

        await Task.Delay(600);
        _firstPersonController.SetMovementEnable(false);
        if (StaticHolder.maxPlayers == 2)
        {
            _thirdPlace.gameObject.SetActive(false);
        }
        _leaderBord.SetLeaderBord(players, _nicks);
        leaderBord.gameObject.SetActive(false);
        taggerText.gameObject.SetActive(false);
        _finishCamera.enabled = true;
        _mainCamera.enabled = false;

        _endGameAnim.SetBool("gameEnded_hide", true);

        _firstPersonController.SetMovementEnable(false);
        await Task.Delay(10 * 1000);
        _firstPersonController.SetMovementEnable(false);

        _endGameAnim.SetTrigger("gameEnded_show");

        await Task.Delay(600);
        leaderBord.gameObject.SetActive(false);
        taggerText.gameObject.SetActive(false);

        _firstPersonController.SetMovementEnable(false);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("Menu");
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(_nickColorHEX);
            stream.SendNext(score);
            stream.SendNext(_gameEnded);
            stream.SendNext(_thirdPlace.active);
        }
        else
        {
            _nickColorHEX = (string)stream.ReceiveNext();
            score = (int)stream.ReceiveNext();
            _gameEnded = (bool)stream.ReceiveNext();
            _thirdPlace.active = (bool)stream.ReceiveNext();
        }
    }

    private string ConvertColorToHtml(Color color)
    {
        // Преобразуем значения компонент цвета в диапазон от 0 до 255 и форматируем их как шестнадцатеричные числа
        string rHex = Mathf.RoundToInt(color.r * 255).ToString("X2");
        string gHex = Mathf.RoundToInt(color.g * 255).ToString("X2");
        string bHex = Mathf.RoundToInt(color.b * 255).ToString("X2");
        string aHex = Mathf.RoundToInt(color.a * 255).ToString("X2");

        // Собираем строку в формате HTML
        string htmlColor = "#" + rHex + gHex + bHex + aHex;

        return htmlColor;
    }

    public Color ConvertHtmlToColor(string htmlColor)
    {
        // Если цвет в формате "#RRGGBB" или "#RRGGBBAA", удалите начальный '#'
        if (htmlColor != null)
        {
            if (htmlColor.StartsWith("#"))
            {
                htmlColor = htmlColor.Substring(1);
            }

            float r = 0f, g = 0f, b = 0f, a = 1f; // Установите альфа по умолчанию в 1

            if (htmlColor.Length == 6) // Формат RRGGBB
            {
                r = int.Parse(htmlColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                g = int.Parse(htmlColor.Substring(2, 4), System.Globalization.NumberStyles.HexNumber) / 255f;
                b = int.Parse(htmlColor.Substring(4, 6), System.Globalization.NumberStyles.HexNumber) / 255f;
            }
            else if (htmlColor.Length == 8) // Формат RRGGBBAA
            {
                r = int.Parse(htmlColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                g = int.Parse(htmlColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                b = int.Parse(htmlColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
                a = int.Parse(htmlColor.Substring(6, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
            }
            else
            {
                Debug.LogWarning("Invalid HTML color string length. Expected 6 or 8 characters.");
            }

            return new Color(r, g, b, a);

        }
        else
        {
            return new Color(0, 0, 0, 0);
        }

    }
}

enum BonusType
{
    Caught, // догнал
}