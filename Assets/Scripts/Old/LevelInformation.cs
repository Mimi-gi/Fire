using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelInformation", menuName = "Scriptable Objects/LevelInformation")]
public class LevelInformation : ScriptableObject
{
    public CameraInformation CameraInformation;
    HashSet<IResetable> resetables = new HashSet<IResetable>();
    public IReadOnlyCollection<IResetable> Resetables => resetables;
    public int Level;
    public void RegisterResetable(IResetable resetable)
    {
        resetables.Add(resetable);
    }
    public void Reset()
    {
        foreach (var resetable in resetables)
        {
            Debug.Log("リセット");
            resetable.Reset();
        }
    }
}

