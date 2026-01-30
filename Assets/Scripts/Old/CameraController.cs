using UnityEngine;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    public Camera mainCamera;
    [SerializeField] float _duration = 0.5f;
    [SerializeField] Ease _ease = Ease.Linear;
    public int CurrentLevel;
    [HideInInspector] public LevelInformation CurrentLevelInformation;
    [SerializeField] List<LevelInformation> levelInformations;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
        CurrentLevelInformation = levelInformations[CurrentLevel];
        mainCamera.transform.position = CurrentLevelInformation.CameraInformation.CenterPos;
        mainCamera.orthographicSize = CurrentLevelInformation.CameraInformation.Size;
    }

    public async UniTask MoveCamera(LevelInformation levelInformation, Player player)
    {
        if (CurrentLevel == levelInformation.Level)
        {
            return;
        }
        CurrentLevelInformation.Reset();
        CurrentLevel = levelInformation.Level;
        CurrentLevelInformation = levelInformation;
        Vector3 startPos = mainCamera.transform.position;
        Vector3 targetPos = levelInformation.CameraInformation.CenterPos;
        float startSize = mainCamera.orthographicSize;
        float targetSize = levelInformation.CameraInformation.Size;

        // 位置とサイズを同時に線形補間
        PlayerManager.Instance.OffPlayer();
        await LMotion.Create(0f, 1f, _duration)
            .WithEase(_ease)
            .Bind(t =>
            {
                mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
                mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            })
            .ToUniTask();
        PlayerManager.Instance.OnPlayer();
        PlayerManager.Instance.DeathOthers(player);
    }
}
