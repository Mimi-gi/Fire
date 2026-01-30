using UnityEngine;
using Cysharp.Threading.Tasks;

public class CameraMoveKey : MonoBehaviour
{
    [SerializeField] LevelInformation cameraInformation;
    public Movetype Movetype;
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.TryGetComponent<Player>(out Player player))
        {

            CameraController.Instance.MoveCamera(cameraInformation, player).Forget();

        }
    }
}

public enum Movetype
{
    Reset,
    None
}