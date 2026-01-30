using UnityEngine;
using UnityEngine.VFX;
using R3;
using R3.Triggers;
using LitMotion;
using Cysharp.Threading.Tasks;

public class Rope : MonoBehaviour, IActable
{
    [SerializeField] float _h;
    [SerializeField] SpriteRenderer _back;
    [SerializeField] SpriteRenderer _spriteRenderer;
    [SerializeField] VisualEffect _vfx;
    [SerializeField] Collider2D _col;

    [SerializeField] float _speed = 5f;
    bool _IsFired = false;
    float t = 5;
    //スプライトのheightは5->0
    //VFXのheightは5->0
    void Start()
    {
        _vfx.Stop();
        _back.size = new Vector2(_back.size.x, _h);
        _spriteRenderer.size = new Vector2(_spriteRenderer.size.x, _h);
        _col.offset = new Vector2(_col.offset.x, _h);

    }
    public void OnAct(Player player)
    {
        Debug.Log("Rope Act");
        if (_IsFired) return;
        PlayerManager.Instance.OffPlayer();
        player.gameObject.SetActive(false);
        UniTask.Create(async () =>
        {
            _IsFired = true;
            t = _h;
            _vfx.Play();
            await LMotion.Create(0f, _h, _h / _speed)
            .Bind(x =>
            {
                t = _h - x;
                _spriteRenderer.size = new Vector2(_spriteRenderer.size.x, t);
                _vfx.transform.localPosition = new Vector3(_vfx.transform.localPosition.x, t, _vfx.transform.localPosition.z);
            });
            _vfx.Stop();
            var newPos = transform.position - Vector3.up * 0.5f;
            player.transform.position = newPos;
            player.gameObject.SetActive(true);
            PlayerManager.Instance.OnPlayer();
        }).Forget();
    }
}
