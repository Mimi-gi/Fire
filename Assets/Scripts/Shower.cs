using UnityEngine;
using R3;
using UnityEngine.VFX;
using R3.Triggers;

public class Shower : Buttonedable
{
    ReactiveProperty<bool> _isOn = new ReactiveProperty<bool>(true);
    public ReadOnlyReactiveProperty<bool> IsOn => _isOn;
    Collider2D _collider;
    [SerializeField] VisualEffect showerEffect;
    void Start()
    {
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
    }

    public override void Set(bool isPressed)
    {
        _isOn.Value = !isPressed;
        Debug.Log("Shower Set: " + _isOn.Value);
    }
}