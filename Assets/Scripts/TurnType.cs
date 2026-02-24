namespace BossFight
{
    public enum TurnType
    {
        HeroTurn,
        MonsterTurn,
        GenieChoice,
        SecondMonsterTurn
    }

    public enum GameState
    {
        Playing,
        Success,
        Failed,
        TimeOut,
        Rewinding
    }
}
