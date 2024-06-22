using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class IsMine : Photon.MonoBehaviour
{
    [SerializeField] private FirstPersonController _fpc;
    [SerializeField] private GameObject _camera;
    [SerializeField] private LeaderBord _leaderBord;

    private PhotonView _view;

    private void Start()
    {
        _view = GetComponent<PhotonView>();

        if (!_view.isMine)
        {
            _fpc.enabled = false;
            _camera.SetActive(false);
            _leaderBord.gameObject.SetActive(false);
        }
    }
}
