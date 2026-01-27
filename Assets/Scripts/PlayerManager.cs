using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    public bool CanBirth { get; private set; }

    List<Player> _players = new();
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {

    }
    void BirthCheck()
    {
        if (_players.Count < 1)
        {
            CanBirth = true;
        }
        else
        {
            CanBirth = false;
        }
        Debug.Log("CanBirth: " + CanBirth);
    }

    public void RegisterPlayer(Player player)
    {
        if (_players.Contains(player)) return;
        _players.Add(player);
        BirthCheck();
    }

    public void UnregisterPlayer(Player player)
    {
        if (!_players.Contains(player)) return;
        _players.Remove(player);
        BirthCheck();
    }

    public void OffPlayer()
    {
        foreach (var player in _players)
        {
            player.Off();
        }
    }
    public void OnPlayer()
    {
        foreach (var player in _players)
        {
            player.On();
        }
    }
}