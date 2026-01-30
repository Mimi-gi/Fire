using UnityEngine;

[CreateAssetMenu(fileName = "CameraInformation", menuName = "Scriptable Objects/CameraInformation")]
public class CameraInformation : ScriptableObject
{
    [SerializeField] Vector3 _centerPos;
    [SerializeField] float _size;
    public Vector3 CenterPos => _centerPos;
    public float Size => _size;
}
