public class AbilityContext
{
    public TargetRegistry TargetRegistry { get; private set; }
    public BaseUnitStats Caster { get; private set; }

    public AbilityContext(TargetRegistry targetRegistry,
        BaseUnitStats caster = null)
    {
        TargetRegistry = targetRegistry;
        Caster = caster;
    }
}
