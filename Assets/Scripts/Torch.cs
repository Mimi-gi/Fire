using System;
using System.Collections.Generic;
using R3;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Torch : MonoBehaviour, IButton, IActable
{
    ReactiveProperty<bool> _isPressed = new ReactiveProperty<bool>(false);
    public ReadOnlyReactiveProperty<bool> IsPressed { get { return _isPressed; } }
    [SerializeField] List<Buttonedable> buttonedable;
    public IDisposable _spriteDispose; //炎の移動


    //仮実装
    [SerializeField] Sprite _idle;
    [SerializeField] Sprite _pressed;
    [SerializeField] Sprite _pressedMove; //デフォルトは左(ややこい)

    [SerializeField] Light2D _light;
    SpriteRenderer _spriteRenderer;
    void Start()
    {
        _light.enabled = false;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _isPressed.Subscribe(isPressed =>
        {
            Debug.Log(isPressed);
            if (buttonedable != null)
            {
                foreach (var item in buttonedable)
                {
                    item.Set(isPressed);
                }
            }
            if (isPressed)
            {
                _light.enabled = true;
                _spriteRenderer.sprite = _pressed;
            }
            else
            {
                _light.enabled = false;
                _spriteRenderer.sprite = _idle;
                _spriteDispose?.Dispose();
            }
        });
        _spriteRenderer.sprite = _idle;
    }

    public void OnAct(Player player = null)
    {
        _isPressed.Value = !_isPressed.Value;
        if (_isPressed.Value)
        {
            _spriteDispose = InputProcessor.Instance.Move
                .Subscribe(x =>
                {
                    if (x > 0)
                    {
                        _spriteRenderer.sprite = _pressedMove;
                        _spriteRenderer.flipX = true;
                    }
                    else if (x < 0)
                    {
                        _spriteRenderer.sprite = _pressedMove;
                        _spriteRenderer.flipX = false;
                    }
                    else
                    {
                        _spriteRenderer.sprite = _pressed;
                    }
                });
        }
        else
        {
            GetComponent<SpriteRenderer>().sprite = _idle;
        }
    }


}
