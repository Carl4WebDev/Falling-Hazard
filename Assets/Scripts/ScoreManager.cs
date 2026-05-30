using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public Text scoreDisplay;
    public Text finalScoreDisplay;

    [Header("Combo")]
    public float multiplierInterval = 10f;
    public int maxMultiplier = 5;

    private float score;
    private bool isRunning = true;
    private int multiplier = 1;
    private float timeSinceLastHit = 0f;
    private int highScore;

    [Header("Kill Counter")]
    public Text killDisplay;
    private int killCount = 0;

    void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    void Update()
    {
        if (isRunning)
        {
            timeSinceLastHit += Time.deltaTime;
            if (timeSinceLastHit >= multiplierInterval * multiplier && multiplier < maxMultiplier)
            {
                multiplier++;
            }

            score += Time.deltaTime * multiplier;
            scoreDisplay.text = Mathf.FloorToInt(score).ToString();
        }
    }

    public void OnDamageTaken()
    {
        multiplier = 1;
        timeSinceLastHit = 0f;
    }

    public int GetMultiplier()
    {
        return multiplier;
    }

    public void AddKill()
    {
        killCount++;
        if (killDisplay != null)
            killDisplay.text = killCount.ToString();
    }

    public int GetKills()
    {
        return killCount;
    }

    public void PauseScore()
    {
        isRunning = false;
    }

    public void ResumeScore()
    {
        isRunning = true;
    }

    public void StopScore()
    {
        isRunning = false;
        int finalScore = Mathf.FloorToInt(score);
        if (finalScore > highScore)
        {
            highScore = finalScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }
        finalScoreDisplay.text = "Score: " + finalScore + "\nBest: " + highScore + "\nKills: " + killCount;
    }
}
