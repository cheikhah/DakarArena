using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryManager : MonoBehaviour
{
    public void Replay()
    {
        SceneManager.LoadScene("Arena_Sandaga");
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}