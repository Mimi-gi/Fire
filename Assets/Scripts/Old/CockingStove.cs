using UnityEngine;
using R3;
using R3.Triggers;
using System;
using UnityEngine.Rendering.Universal;

public class CockingStove : MonoBehaviour, IActable, IResetable
{
    [SerializeField] LevelInformation _levelInformation;
    public LevelInformation LevelInformation { get => _levelInformation; }
    [SerializeField] GameObject _playerPrefab;
    [SerializeField] SpriteRenderer _sr;

    [SerializeField] bool _initiallyActive = false;
    ReactiveProperty<bool> _isActive;
    //
    [SerializeField] Sprite _on;
    [SerializeField] Sprite _off;
    [SerializeField] Light2D _light;

    void Start()
    {
        Register();
        _isActive = new(_initiallyActive);
        _light.enabled = _initiallyActive;
        _sr.sprite = _initiallyActive ? _on : _off;

        // 初期状態がアクティブなら登録
        if (_initiallyActive)
        {
            PlayerManager.Instance.RegisterActiveStove(this);
        }

        _isActive
        .Subscribe(isActive =>
        {
            if (isActive)
            {
                Debug.Log("Stove is active");
                _sr.sprite = _on;
                _light.enabled = true;
                PlayerManager.Instance.RegisterActiveStove(this);
            }
            else
            {
                _sr.sprite = _off;
                _light.enabled = false;
                PlayerManager.Instance.UnregisterActiveStove(this);
            }
        });
    }

    public void SpawnPlayer()
    {
        var player = Instantiate(_playerPrefab, transform.position, Quaternion.identity);
        PlayerManager.Instance.RegisterPlayer(player.GetComponent<Player>());
    }

    public void OnAct(Player player = null)
    {
        _isActive.Value = true;
    }

    public void Reset()
    {
        _isActive.Value = _initiallyActive;
    }

    public void Register()
    {
        LevelInformation.RegisterResetable(this);
    }
}
