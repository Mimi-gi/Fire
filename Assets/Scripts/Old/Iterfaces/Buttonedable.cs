using UnityEngine;

public abstract class Buttonedable : MonoBehaviour
{
    private int _pendingPressCount = 0;
    private bool _isCurrentlyActive = false;

    void LateUpdate()
    {
        if (_pendingPressCount > 0)
        {
            // 奇数回 = 状態反転, 偶数回 = 変化なし
            if (_pendingPressCount % 2 == 1)
            {
                _isCurrentlyActive = !_isCurrentlyActive;
                OnStateChanged(_isCurrentlyActive);
            }
            _pendingPressCount = 0;
        }
    }

    /// <summary>
    /// ボタンが押された時に呼ぶ（トグル）
    /// </summary>
    public void Press()
    {
        _pendingPressCount++;
    }

    /// <summary>
    /// 互換性のため残す（即時反映が必要な場合用）
    /// </summary>
    public void Set(bool isPressed)
    {
        _isCurrentlyActive = isPressed;
        OnStateChanged(_isCurrentlyActive);
    }

    /// <summary>
    /// 初期状態を設定する（Start等で呼び出す）
    /// </summary>
    protected void InitializeState(bool initialActive)
    {
        _isCurrentlyActive = initialActive;
        OnStateChanged(_isCurrentlyActive);
    }

    /// <summary>
    /// 状態が変化した時に呼ばれる
    /// </summary>
    protected abstract void OnStateChanged(bool isActive);
}