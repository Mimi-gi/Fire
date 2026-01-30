using System.Collections.Generic;
using UnityEngine;
using R3;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    public bool CanBirth { get; private set; }

    List<Player> _players = new();
    List<CockingStove> _activeStoves = new();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        BirthCheck();

        // Birth入力を一括管理
        InputProcessor.Instance.Birth
            .Where(x => x == InputType.Press && CanBirth)
            .Subscribe(x => SpawnFromAllActiveStoves());
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

    void SpawnFromAllActiveStoves()
    {
        var currentLevel = CameraController.Instance.CurrentLevelInformation;
        foreach (var stove in _activeStoves)
        {
            if (stove.LevelInformation == currentLevel)
            {
                stove.SpawnPlayer();
            }
        }
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

    public void RegisterActiveStove(CockingStove stove)
    {
        if (_activeStoves.Contains(stove)) return;
        _activeStoves.Add(stove);
    }

    public void UnregisterActiveStove(CockingStove stove)
    {
        if (!_activeStoves.Contains(stove)) return;
        _activeStoves.Remove(stove);
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

    public void DeathOthers(Player excludePlayer)
    {
        // リストをコピーしてイテレーション（Deathでリストが変更される可能性があるため）
        var playersCopy = new List<Player>(_players);
        foreach (var player in playersCopy)
        {
            if (player != excludePlayer)
            {
                player.Death().Forget();
            }
        }
    }
}
