using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject pausePanel;
    public ScoreManager scoreManager;

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
        if (scoreManager != null) scoreManager.PauseScore();
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (scoreManager != null) scoreManager.ResumeScore();
    }

    public void PlayGame () {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }

    public void RestartGame () {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }

    public void MainMenu () {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

}
