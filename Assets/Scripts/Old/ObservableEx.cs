using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using System;

namespace ObservableExtensions
{
    public static class ObservableEx
    {
        public static Observable<InputAction.CallbackContext> StartedAsObservable(this InputAction action)
        {
            return Observable.Create<InputAction.CallbackContext>(observer =>
            {
                Action<InputAction.CallbackContext> handler = ctx => observer.OnNext(ctx);
                action.started += handler;
                return Disposable.Create(() => action.started -= handler);
            });
        }

        public static Observable<InputAction.CallbackContext> PerformedAsObservable(this InputAction action)
        {
            return Observable.Create<InputAction.CallbackContext>(observer =>
            {
                Action<InputAction.CallbackContext> handler = ctx => observer.OnNext(ctx);
                action.performed += handler;
                return Disposable.Create(() => action.performed -= handler);
            });
        }

        public static Observable<InputAction.CallbackContext> CanceledAsObservable(this InputAction action)
        {
            return Observable.Create<InputAction.CallbackContext>(observer =>
            {
                Action<InputAction.CallbackContext> handler = ctx => observer.OnNext(ctx);
                action.canceled += handler;
                return Disposable.Create(() => action.canceled -= handler);
            });
        }

        // Keep the original for backward compatibility if needed, or remove if unused.
        public static Observable<InputType> InputToObservable(InputAction action)
        {
            return Observable.Merge(
                action.StartedAsObservable().Select(_ => InputType.Press),
                action.CanceledAsObservable().Select(_ => InputType.Release)
            );
        }

        public static async UniTask AwaitInvoke(Observable<InputType> observable, Func<UniTask> onTimeout, Func<UniTask> onEvent, float timeout = -1f)
        {
            UniTask task_event = observable.FirstAsync().AsUniTask();
            UniTask task_timeout = UniTask.Delay((int)(timeout * 1000));
            var index = await UniTask.WhenAny(task_event, task_timeout);
            if (index == 0)
            {
                await onEvent();
            }
            else
            {
                await onTimeout();
            }
        }

        public static async UniTask SeriesAwaitInvoke(
            Observable<InputType> observable,
            Func<UniTask> preMotion,
            Func<UniTask> onTimeout,
            Func<UniTask> onEvent,
            float startInput,
            float endInput)
        {
            // preMotionを開始（await せずに走らせておく）
            var preMotionTask = preMotion();

            // startInput秒待ってからインプット受付開始
            await UniTask.Delay((int)(startInput * 1000));

            // インプット受付開始
            bool inputReceived = false;
            var inputTask = observable.FirstAsync().AsUniTask();
            var timeoutTask = UniTask.Delay((int)(endInput * 1000));

            // どちらが先に来るか待つ（inputTaskが先なら hasResultLeft = true）
            var (hasInput, _) = await UniTask.WhenAny(inputTask, timeoutTask);

            if (hasInput)
            {
                // インプットを受け取った
                inputReceived = true;
            }

            // preMotionが終わるまで待つ
            await preMotionTask;

            if (inputReceived)
            {
                // インプットがあった場合はonEventを実行
                await onEvent();
            }
            else
            {
                // タイムアウトした場合はonTimeoutを実行
                await onTimeout();
            }
        }

        public static ReadOnlyReactiveProperty<float> InputAxisToReactiveProperty(InputAction action)
        {
            return Observable.Create<float>(observer =>
            {
                Action<InputAction.CallbackContext> handler = ctx => observer.OnNext(ctx.ReadValue<float>());
                action.performed += handler;
                action.canceled += handler;
                return Disposable.Create(() =>
                {
                    action.performed -= handler;
                    action.canceled -= handler;
                });
            }).ToReadOnlyReactiveProperty(action.ReadValue<float>());
        }

        /// <summary>
        /// Subject<Unit>が発火するまで待機します。
        /// </summary>
        public static async UniTask WaitAsync(this Subject<Unit> subject)
        {
            await subject.FirstAsync().AsUniTask();
        }

        /// <summary>
        /// Observable<Unit>が発火するまで待機します。
        /// </summary>
        public static async UniTask WaitAsync(this Observable<Unit> observable)
        {
            await observable.FirstAsync().AsUniTask();
        }
    }
}

public enum InputType
{
    Press,
    Release
}