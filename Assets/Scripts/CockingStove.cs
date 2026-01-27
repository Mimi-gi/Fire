using UnityEngine;
using R3;
using R3.Triggers;
using System;
using UnityEngine.Rendering.Universal;

public class CockingStove : MonoBehaviour, IActable
{
    [SerializeField] GameObject _playerPrefab;
    [SerializeField] SpriteRenderer _sr;

    ReactiveProperty<bool> _isActive = new(false);
    //
    [SerializeField] Sprite _on;
    [SerializeField] Sprite _off;
    [SerializeField] Light2D _light;
    IDisposable _disposable;
    void Start()
    {
        _light.enabled = false;
        _sr.sprite = _off;
        _isActive
        .Where(b => b)
        .Subscribe(b =>
        {
            _sr.sprite = _on;
            _light.enabled = true;
            _disposable = InputProcessor.Instance.Birth
            .Where(x => x == InputType.Press && PlayerManager.Instance.CanBirth)
            .Subscribe(x =>
            {
                var player = Instantiate(_playerPrefab, transform.position, Quaternion.identity);
                PlayerManager.Instance.RegisterPlayer(player.GetComponent<Player>());
            });
        });


    }

    public void OnAct(Player player=null)
    {
        _isActive.Value = true;
    }
}