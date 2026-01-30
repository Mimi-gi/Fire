using UnityEngine;
using UnityEngine.InputSystem;
using R3;
using R3.Triggers;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using System.Collections.Generic;
using UnityEngine.VFX;
using System;
using System.Threading;
using UniTaskExtensions;
using ObservableExtensions;


public class Player : MonoBehaviour
{
    [Header("コンポーネント群")]
    [SerializeField] Transform _tr;
    [SerializeField] SpriteRenderer _sr;
    [SerializeField] Collider2D _col;
    [SerializeField] Sprite _idle;
    [SerializeField] Sprite _stretch;
    [SerializeField] VisualEffect _vfx;
    [SerializeField] Animator _animator;
    TransformVFX _transformVFX;

    [Header("横移動関連")]
    [SerializeField] float _moveDuration = 0.2f;
    [SerializeField] int _resolution = 32;
    [SerializeField] Ease _moveEase = Ease.Linear;
    [SerializeField] Collider2D _leftCollider;
    [SerializeField] Collider2D _rightCollider;
    [HideInInspector] public bool Movability = true;
    [HideInInspector] public bool _isMoving = false; // Single press only logic
    [Header("伸縮関連")]
    [SerializeField] Collider2D _upCollider;
    [HideInInspector] public bool Stretchability = true; // TODO: Remove 'readonly' if this needs to change at runtime
    [HideInInspector] public bool _isStretching = false;
    [Header("エフェクト関連")]
    [SerializeField] TransformData _idleBox;
    [SerializeField] TransformData _stretchBox;
    [Header("行動関連")]
    [SerializeField] VisualEffect _actVFX;
    CircleVFX _actVFxTransform;
    public bool Actability = true;
    [SerializeField] Circle _idleFxCircle;
    [SerializeField] Circle _stretchFxCircle;
    [SerializeField] BoxCollider2D _actCollider;
    [SerializeField] TransformData _actIdleBox;
    [SerializeField] TransformData _actStretchBox;
    [Header("消失関連")]
    [SerializeField] AnimationClip _death;
    bool _isDeathing = false;

    ReactiveProperty<bool> _inputable = new(true);
    CancellationTokenSource _animationCts;

    HashSet<Collider2D> _leftWalls = new();
    HashSet<Collider2D> _rightWalls = new();
    HashSet<Collider2D> _upWalls = new();
    List<IDisposable> _disposables = new();

    void Start()
    {
        _inputable
        .Subscribe(x =>
        {
            Debug.Log("Inputable: " + x);
        })
        .AddTo(this);
        PlayerManager.Instance.RegisterPlayer(this);
        _transformVFX = new TransformVFX(_vfx);
        _transformVFX.SetTransform(_idleBox);
        _actVFxTransform = new CircleVFX(_actVFX);
        // Reactive Wall Detection
        _disposables.Add(_leftCollider.gameObject.OnTriggerEnter2DAsObservable()
            .Where(c => c.GetComponent<Wall>() != null)
            .Subscribe(c => _leftWalls.Add(c))
            .AddTo(this));

        _disposables.Add(_leftCollider.gameObject.OnTriggerExit2DAsObservable()
            .Where(c => c.GetComponent<Wall>() != null)
            .Subscribe(c => _leftWalls.Remove(c))
            .AddTo(this));

        _disposables.Add(_rightCollider.gameObject.OnTriggerEnter2DAsObservable()
            .Where(c => c.GetComponent<Wall>() != null)
            .Subscribe(c => _rightWalls.Add(c))
            .AddTo(this));

        _disposables.Add(_rightCollider.gameObject.OnTriggerExit2DAsObservable()
            .Where(c => c.GetComponent<Wall>() != null)
            .Subscribe(c => _rightWalls.Remove(c))
            .AddTo(this));
        _disposables.Add(_upCollider.gameObject.OnTriggerEnter2DAsObservable()
            .Where(c => c.GetComponent<Wall>() != null)
            .Subscribe(c => _upWalls.Add(c))
            .AddTo(this));
        _disposables.Add(_upCollider.gameObject.OnTriggerExit2DAsObservable()
            .Where(c => c.GetComponent<Wall>() != null)
            .Subscribe(c => _upWalls.Remove(c))
            .AddTo(this));

        _actCollider.enabled = false;

        _disposables.Add(_actCollider.gameObject.OnTriggerEnter2DAsObservable()
            .Where(c => c.GetComponent<IActable>() != null)
            .Subscribe(c =>
            {
                var actable = c.GetComponent<IActable>();
                actable.OnAct(this);
            })
            .AddTo(this));

        _disposables.Add(InputProcessor.Instance.Move
            .Where(x => x != 0 && _inputable.Value)
            .SubscribeAwait(async (x, ct) =>
            {
                while (ct.IsCancellationRequested == false)
                {
                    float currentInput = InputProcessor.Instance.Move.CurrentValue;

                    if (currentInput == 0 || !_inputable.Value) break;

                    if (_isStretching)
                    {
                        await UniTask.Yield(ct);
                        continue;
                    }

                    int sign = currentInput > 0 ? 1 : -1;
                    bool isBlocked = sign > 0 ? _rightWalls.Count > 0 : _leftWalls.Count > 0;

                    if (isBlocked)
                    {
                        await UniTask.Yield(ct);
                        continue;
                    }

                    _isMoving = true;
                    Vector3 startPos = _tr.position;
                    Vector3 targetPos = startPos + new Vector3(sign, 0, 0);
                    _col.enabled = false;
                    await LMotion.Create(startPos, targetPos, _moveDuration)
                        .WithEase(_moveEase)
                        .Bind(v =>
                        {
                            var x = Mathf.Round(v.x * _resolution) / _resolution;
                            var y = Mathf.Round(v.y * _resolution) / _resolution;
                            _tr.position = new Vector3(x, y, v.z);
                        })
                        .ToUniTask(ct);
                    _col.enabled = true;

                    _isMoving = false;
                    await UniTask.Delay(50);
                }
            }, AwaitOperation.Drop));

        _disposables.Add(InputProcessor.Instance.Up
            .Where(_ => _inputable.Value && !_isMoving && !_isDeathing)
            .SubscribeAwait(async (x, ct) =>
            {
                Debug.Log("State :" + x);
                switch (x)
                {
                    case InputType.Press:
                        Debug.Log("Press");
                        if (!Stretchability) return;
                        await ChangeAnimation(State.Stretch);
                        _isStretching = true;
                        break;
                    case InputType.Release:
                        Debug.Log("Release");
                        await ChangeAnimation(State.Idle);
                        _isStretching = false;
                        break;
                }
                if (_isStretching)
                {
                    _transformVFX.SetTransform(_stretchBox);
                    _actCollider.offset = _stretchBox.Center;
                    _actCollider.size = _stretchBox.Size;
                }
                else
                {
                    _transformVFX.SetTransform(_idleBox);
                    _actCollider.offset = _idleBox.Center;
                    _actCollider.size = _idleBox.Size;
                }
                _transformVFX.Play();
            }));

        _disposables.Add(InputProcessor.Instance.Act
            .Where(x => x == InputType.Press && _inputable.Value)
            .SubscribeAwait(async (x, ct) =>
            {
                Movability = false;
                _inputable.Value = false;
                if (_isStretching)
                {
                    _actVFxTransform.SetCircle(_stretchFxCircle);
                    _actCollider.offset = _actStretchBox.Center;
                    _actCollider.size = _actStretchBox.Size;
                }
                else
                {
                    _actVFxTransform.SetCircle(_idleFxCircle);
                    _actCollider.offset = _actIdleBox.Center;
                    _actCollider.size = _actIdleBox.Size;
                }
                _actVFxTransform.Play();
                switch (x)
                {
                    case InputType.Press:
                        _actCollider.enabled = true;
                        break;
                    case InputType.Release:
                        _actCollider.enabled = false;
                        break;
                }
                await UniTask.Delay(50);
                Movability = true;
                _actCollider.enabled = false;
                _inputable.Value = true;
            }));
    }

    void Update()
    {
        Movability = !_isMoving && !_isStretching;
        Stretchability = !_isMoving && !_isStretching;
    }

    async UniTask ChangeAnimation(State target)
    {
        // 既存のアニメーションをキャンセル
        _animationCts?.Cancel();
        _animationCts?.Dispose();
        _animationCts = new CancellationTokenSource();
        var ct = _animationCts.Token;

        Movability = false;
        Actability = false;

        try
        {
            switch (target)
            {
                case State.Idle:
                    Debug.Log("Idle");
                    await _animator.PlayAndAwait(AnimationNames.Fire_ToIdle, 0, ct);
                    ct.ThrowIfCancellationRequested();
                    _animator.Play(AnimationNames.Fire_idle);
                    break;
                case State.Move:
                    Debug.Log("Move");
                    _animator.Play(AnimationNames.Fire_idle);
                    break;
                case State.Stretch:
                    Debug.Log("Stretch");
                    await _animator.PlayAndAwait(AnimationNames.Fire_ToStretch, 0, ct);
                    ct.ThrowIfCancellationRequested();
                    _animator.Play(AnimationNames.Fire_stretch);
                    break;
            }
            Actability = true;
            Movability = true;
        }
        catch (OperationCanceledException)
        {
            // キャンセルされた場合は新しいアニメーションに処理を委譲
            Debug.Log("Animation cancelled - switching to new animation");
        }
    }

    public async UniTaskVoid Death()
    {
        Debug.Log("Call Death");
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
        _disposables.Clear();
        _isDeathing = true;
        PlayerManager.Instance.UnregisterPlayer(this);
        _animator.Play(AnimationNames.Extinguish2); //モードで変更する予定
        await UniTask.Delay(415);
        _isDeathing = false;
        Destroy(this.gameObject);
    }

    public void Off()
    {
        _inputable.Value = false;
    }

    public void On()
    {
        _inputable.Value = true;
    }




}

public enum State
{
    Idle,
    Move,
    Stretch
}

public static class AnimationNames
{
    public const string Extinguish1 = "Extinguish1";
    public const string Extinguish2 = "Extinguish2";
    public const string Extinguish3 = "Extinguish3";
    public const string Fire_idle = "Fire_idle";
    public const string Fire_stretch = "Fire_stretch";
    public const string Fire_ToStretch = "Fire_ToStretch";
    public const string Fire_ToIdle = "Fire_ToIdle";
}