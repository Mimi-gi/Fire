using UnityEngine;
using UnityEngine.InputSystem;
using R3;
using ObservableExtensions;
using static ObservableExtensions.ObservableEx;

public class InputProcessor : MonoBehaviour
{
    [SerializeField] InputActionAsset _asset;
    InputAction _move;
    InputAction _up;
    InputAction _act;
    InputAction _birth;
    public ReadOnlyReactiveProperty<float> Move { get; private set; }
    public Observable<InputType> Up { get; private set; }
    public Observable<InputType> Act { get; private set; }
    public Observable<InputType> Birth { get; private set; }
    public static InputProcessor Instance { get; private set; }
    void Awake()
    {
        _move = _asset.actionMaps[0]["Move"];
        _up = _asset.actionMaps[0]["Up"];
        _act = _asset.actionMaps[0]["Act"];
        _birth = _asset.actionMaps[0]["Birth"];
        if (Instance != null)
        {
            Debug.LogError("InputProcessor is already initialized");
            return;
        }
        Instance = this;
        Move = InputAxisToReactiveProperty(_move);
        Up = InputToObservable(_up);
        Act = InputToObservable(_act);
        Birth = InputToObservable(_birth);
    }


}