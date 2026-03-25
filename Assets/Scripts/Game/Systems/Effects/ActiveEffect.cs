public class ActiveEffect
{
    public BaseUnitStats Target;
    public IEffect Effect;
    public float RemainingDuration;

    public EffectId Id => Effect.Id;

    public ActiveEffect(BaseUnitStats target, IEffect effect, float duration)
    {
        Target = target;
        Effect = effect;
        RemainingDuration = duration;
    }
}