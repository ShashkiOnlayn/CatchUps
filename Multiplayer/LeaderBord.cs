using System.Linq;
using TMPro;
using UnityEngine;

public class LeaderBord : Photon.MonoBehaviour
{
	[SerializeField] private TMP_Text[] _nicksPlaces;

	private CatchUpController[] _topPlayers = new CatchUpController[3];

	public void SetLeaderBord(CatchUpController[] players, TextMeshPro[] nicksPlaces)
	{
		_topPlayers = new CatchUpController[StaticHolder.maxPlayers == 2 ? 2 : 3];

        _topPlayers = players.OrderByDescending(x => x.score).Take(StaticHolder.maxPlayers == 2 ? 2 : 3).ToArray();

		if(players.Length == 2 )
		{
			_nicksPlaces[2].transform.parent.gameObject.SetActive(false);
		}

		for (int i = 0; i < _topPlayers.Length; i++)
		{
			_topPlayers[i].SetCrownEnable(i is 0);

			_nicksPlaces[i].text = _topPlayers[i].photonView.owner.NickName + " -- " + _topPlayers[i].score;
            nicksPlaces[i].text = _topPlayers[i].nickname.text;
        }
    }
}
