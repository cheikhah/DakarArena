using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public Slider healthBarP1;
    public Slider healthBarP2;
    public TextMeshProUGUI timerText;

    private float timeLeft = 99f;
    private bool gameOver = false;

    void Update()
    {
        if (gameOver) return;

        timeLeft -= Time.deltaTime;
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(timeLeft).ToString();

        if (timeLeft <= 0)
            EndGame();
    }

    public void DamagePlayer1(float pourcentageDegats)
    {
        AppliquerDegats(healthBarP1, pourcentageDegats, "healthBarP1");
    }

    public void DamagePlayer2(float pourcentageDegats)
    {
        AppliquerDegats(healthBarP2, pourcentageDegats, "healthBarP2");
    }

    // pourcentageDegats est une fraction (0.1 = 10% de la vie max), pas une valeur absolue.
    // Comme ça, le résultat est correct peu importe si le Slider est configuré en 0-1, 0-100, etc.
    private void AppliquerDegats(Slider barre, float pourcentageDegats, string nomChamp)
    {
        if (barre == null)
        {
            Debug.LogWarning($"GameManager : le champ '{nomChamp}' n'est pas assigné dans l'Inspector. " +
                              "Aucun dégât ne peut être appliqué tant que ce n'est pas réglé.");
            return;
        }

        float degatsReels = pourcentageDegats * (barre.maxValue - barre.minValue);
        barre.value -= degatsReels;

        if (barre.value <= barre.minValue)
            EndGame();
    }

    void EndGame()
    {
        gameOver = true;
        SceneManager.LoadScene("Victory");
    }
}