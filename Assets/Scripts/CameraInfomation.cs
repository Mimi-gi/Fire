using UnityEngine;

[CreateAssetMenu(fileName = "CameraInfomation", menuName = "Scriptable Objects/CameraInfomation")]
public class CameraInfomation : ScriptableObject
{
    [SerializeField] Vector3 _centerPos;
    [SerializeField] float _size;
    public Vector3 CenterPos => _centerPos;
    public float Size => _size;
}
