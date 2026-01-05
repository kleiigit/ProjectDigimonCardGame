namespace ProjectScript.Enums
{
    public enum PlayerSide
    {
        PlayerBlue,
        PlayerRed
    }
    public enum Phase
    {
        UpPhase,
        DrawPhase,
        CostPhase,
        EvolutionPhase,
        MainPhase,
        BattlePhase,
        PreparationPhase,
        AttackPhase,
        EndPhase
    }
    public enum  FieldPlace
    {
        MainDeck,
        PartnerPile,
        SecurityPile,
        TrashPile,
        BattleZone,
        LeaderZone,
        DataPile,
        CheckZone,
        Hand,
    }
    public enum Keyword
    {
        Draw,
        Target,
        Condition,
        Destroy,
        Discard,
        Down,
        Freeze,
        Cache,
    }
}