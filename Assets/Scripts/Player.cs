using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] InputActionMap _map;
    InputAction _moveInput;
    InputAction _upInput;
    InputAction _actInput;
    InputProcessor _inputProcessor;

    void Start()
    {
        _inputProcessor = new InputProcessor(_moveInput, _upInput, _actInput);
    }
}
