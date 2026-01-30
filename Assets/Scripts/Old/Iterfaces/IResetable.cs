public interface IResetable
{
    public LevelInformation LevelInformation { get; }
    public void Reset();
    public void Register();

    //初期状態に戻す
}