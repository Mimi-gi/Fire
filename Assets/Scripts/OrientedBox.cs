using UnityEngine;

[CreateAssetMenu(fileName = "OrientedBox", menuName = "Scriptable Objects/OrientedBox")]
public class TransformData : ScriptableObject
{
    [SerializeField] Vector3 _center = Vector3.zero;
    [SerializeField] Vector3 _size = Vector3.one;
    [SerializeField] Vector3 _eulerAngles = Vector3.zero;
    public Vector3 Center => _center;
    public Vector3 Size => _size;
    public Vector3 EulerAngles => _eulerAngles;
}
