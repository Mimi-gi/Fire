using UnityEngine;
using R3;
using UnityEngine.VFX;
using R3.Triggers;

public class Shower : Buttonedable
{
    [SerializeField] bool _initiallyActive = true;
    ReactiveProperty<bool> _isOn;
    public ReadOnlyReactiveProperty<bool> IsOn => _isOn;
    Collider2D _collider;
    [SerializeField] VisualEffect showerEffect;
    void Start()
    {
        _initiallyActive = !_initiallyActive;
        _isOn = new ReactiveProperty<bool>(_initiallyActive);
        _collider = this.GetComponent<Collider2D>();
        _isOn.Subscribe(isOn =>
        {
            _collider.enabled = isOn;
            if (isOn)
            {
                showerEffect.Play();
            }
            else
            {
                showerEffect.Stop();
            }
        });
        _collider.OnTriggerEnter2DAsObservable()
            .Subscribe(c =>
            {
                Debug.Log("Trigger Enter: " + c.gameObject.name);
                if (c.TryGetComponent(out Player player))
                {
                    player.Death();
                }
            });

        // 初期状態を設定（ボタン未押下=false）
        InitializeState(false);
    }

    protected override void OnStateChanged(bool isActive)
    {
        // isActive=true(ボタン押下中): 初期状態を反転
        // isActive=false(ボタン未押下): 初期状態のまま
        _isOn.Value = _initiallyActive != isActive;
        Debug.Log("Shower OnStateChanged: " + _isOn.Value);
    }
}