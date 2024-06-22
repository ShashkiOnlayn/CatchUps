using System.Collections.Generic;
using UnityEngine;

public static class StaticHolder
{
    [Header("Muliplayer")]
    public static bool playersPresent = false; // игроки присутвуют?
    public static byte skyboxIndex;
    public static byte maxPlayers;
    public static int randomPlayerIndex;
    public static PhotonPlayer currentTagger;
    public static List<GameObject> players = new();
}
