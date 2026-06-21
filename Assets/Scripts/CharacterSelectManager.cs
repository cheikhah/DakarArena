using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectManager : MonoBehaviour
{
    public void SelectDjibril()
    {
        PlayerPrefs.SetString("SelectedCharacter", "Djibril");
        SceneManager.LoadScene("Arena_Sandaga");
    }

    public void SelectMariama()
    {
        PlayerPrefs.SetString("SelectedCharacter", "Mariama");
        SceneManager.LoadScene("Arena_Sandaga");
    }

    public void GoBack()
    {
        SceneManager.LoadScene("MainMenu");
    }
}