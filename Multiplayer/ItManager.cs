using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItManager : MonoBehaviour // менеджер вод
{
    public static List<GameObject> _players = new();

    private static bool _notChanged = true;

    private void Start()
    {
        print("Started");
        StartCoroutine(CheckPlayers());
    }

    private IEnumerator CheckPlayers()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (_players.Count > 1 && _notChanged)
            {
                yield return StartCoroutine(StartTimer());
                ChangeIt(_players[Random.Range(0, NicknamesHolder.userNames.Count)]);
                _notChanged = false;
                break;
            }
        }
    }

    private IEnumerator StartTimer()
    {
        for (int i = 5; i > 0; i--)
        {
            yield return new WaitForSeconds(1);
            Debug.Log(i);
        }
    }

    private void ChangeIt(GameObject newIt)
    {
        _notChanged = false;
        print("Присвоил");
        //newIt.GetComponent<CatchUpController>().SetIt(true);
    }
}
