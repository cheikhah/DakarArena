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
        timerText.text = Mathf.CeilToInt(timeLeft).ToString();

        if (timeLeft <= 0)
        {
            EndGame();
        }
    }

    public void DamagePlayer1(float amount)
    {
        healthBarP1.value -= amount;
        if (healthBarP1.value <= 0) EndGame();
    }

    public void DamagePlayer2(float amount)
    {
        healthBarP2.value -= amount;
        if (healthBarP2.value <= 0) EndGame();
    }

    void EndGame()
    {
        gameOver = true;
        SceneManager.LoadScene("Victory");
    }
}