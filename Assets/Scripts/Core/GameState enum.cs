namespace GeniesGambit.Core
{
    public enum GameState
    {
        MainMenu,
        HeroTurn,        // Player controls hero → path is RECORDED
        GenieWishScreen, // Mid-game pause: pick wishes
        MonsterTurn,     // Player controls monster → hero ghost plays back
        LevelComplete,
        GameOver,
        BossScene        // Triggered after Iteration 7 completes
    }
}