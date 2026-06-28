using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class VictoryManager : MonoBehaviour
{
    public TextMeshProUGUI texteResultat;

    public void AfficherResultat(string texte)
    {
        if (texteResultat != null)
            texteResultat.text = texte;
    }

    public void Replay()
    {
        Scene sceneActuelle = SceneManager.GetActiveScene();
        SceneManager.LoadScene(sceneActuelle.name);
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}