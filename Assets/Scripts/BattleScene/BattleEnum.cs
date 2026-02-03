namespace BattleScene
{
    public enum SkillType
    {
        Dodge,
        NormalAttack,
        AttackSkill,
        DefenseSkill,
        FunctionalSkill,
    }

    public enum BattleState
    {
        None,
        Preparing,
        Rhythm,
        Decision,
        Resolve,
        Finished 
    }

    public enum RhythmState
    {
        Idle,
        Beating,
        Decision
    }

    public enum ActionRejectReason
    {
        None,
        NotInDecision,
        AlreadySelected,
        InvalidSkill,
        SpiritNotEnough,
        DodgeNotEnough
    }

    public enum RoundPhase
    {
        None,
        Beat,
        Decision,
        Result,
        LoopDelay
    }

}