public interface IEffect
{
    EffectId Id { get; }

    void OnApply(BaseUnitStats target);
    void OnExpire(BaseUnitStats target);
    void Tick(BaseUnitStats target, float deltaTime);
}