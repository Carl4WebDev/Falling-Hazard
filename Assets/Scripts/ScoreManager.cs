using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public Text scoreDisplay;
    public Text finalScoreDisplay;

    private float score;
    private bool isRunning = true;

    void Update()
    {
        if (isRunning)
        {
            score += Time.deltaTime;
            scoreDisplay.text = Mathf.FloorToInt(score).ToString();
        }
    }

    public void StopScore()
    {
        isRunning = false;
        finalScoreDisplay.text = "Score: " + Mathf.FloorToInt(score).ToString();
    }
}
