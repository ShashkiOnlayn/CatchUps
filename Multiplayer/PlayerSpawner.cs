using UnityEngine;

public class PlayerSpawner : Photon.MonoBehaviour
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform[] _spawnPoints;

    private void Start()
    {
        Vector3 dot = _spawnPoints[Random.Range(0, _spawnPoints.Length)].position;

        PhotonNetwork.Instantiate(_playerPrefab.name, dot, Quaternion.identity, 0);

       // await Task.Delay(500);
       // player.name = player.GetComponent<CatchUpController>().nickname.text;
    }
}
