using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;

[CreateAssetMenu(fileName = "Circle", menuName = "Scriptable Objects/Circle")]
public class Circle : ScriptableObject
{
    [SerializeField] TransformData _transformData;
    [SerializeField] float _radius = 0.5f;
    public Vector3 Center => _transformData.Center;
    public Vector3 Size => _transformData.Size;
    public Vector3 EulerAngles => _transformData.EulerAngles;
    public float Radius => _radius * Mathf.PI * 2;
}
