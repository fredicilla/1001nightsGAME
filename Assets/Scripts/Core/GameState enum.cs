namespace GeniesGambit.Core
{
    public enum GameState
    {
        MainMenu,
        HeroTurn,        // Player controls hero → path is RECORDED
        GenieWishScreen, // Mid-game pause: pick 3 wishes
        MonsterTurn,     // Player controls monster → hero ghost plays back
        LevelComplete,
        GameOver
    }
}