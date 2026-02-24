using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // Function for your "Start" button
    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    // Function for your "Controls" button
    public void OpenControls()
    {
        SceneManager.LoadScene("Control");
    }

    // Pro-tip: A "Back" button to return to the menu
    public void BackToMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
}