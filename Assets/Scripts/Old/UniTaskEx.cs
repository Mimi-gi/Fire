using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Linq;
using System.Threading;

namespace UniTaskExtensions
{
    public static class UniTaskEx
    {
        /// <summary>
        /// 指定した名前のアニメーションステートを再生し、再生終了まで待機します。
        /// </summary>
        /// <param name="animator">対象のAnimator</param>
        /// <param name="stateName">再生するステート名</param>
        /// <param name="layer">レイヤー（デフォルト: 0）</param>
        /// <param name="ct">キャンセレーショントークン</param>
        public static async UniTask PlayAndAwait(this Animator animator, string stateName, int layer = 0, CancellationToken ct = default)
        {
            // ステートが存在するか確認
            int stateHash = Animator.StringToHash(stateName);
            if (!animator.HasState(layer, stateHash))
            {
                Debug.LogError($"Animator does not have state '{stateName}' on layer {layer}");
                return;
            }

            // クリップの長さを取得
            float clipLength = GetClipLength(animator, stateName);
            if (clipLength <= 0f)
            {
                Debug.LogError($"Could not find clip length for state '{stateName}'");
                return;
            }

            // アニメーションを再生
            animator.Play(stateName, layer, 0f);

            // クリップの長さだけ待機（キャンセル可能）
            await UniTask.Delay((int)(clipLength * 1000), cancellationToken: ct);
        }

        /// <summary>
        /// AnimatorControllerからクリップの長さを取得します。
        /// </summary>
        private static float GetClipLength(Animator animator, string stateName)
        {
            if (animator.runtimeAnimatorController == null)
            {
                return -1f;
            }

            // AnimatorControllerからクリップを検索
            var clips = animator.runtimeAnimatorController.animationClips;
            var clip = clips.FirstOrDefault(c => c.name == stateName);

            if (clip != null)
            {
                return clip.length;
            }

            // クリップ名がステート名と異なる場合は見つからない
            return -1f;
        }
    }
}