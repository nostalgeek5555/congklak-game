using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HomePanel : MonoBehaviour
{
    [Header("Buttons")]
    public Button startButton;
    public Button exitButton;

    // Start is called before the first frame update
    void Start()
    {
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(() =>
        {
            HandleStartButton();
        });

        exitButton.onClick.RemoveAllListeners();
        exitButton.onClick.AddListener(() =>
        {
            HandleExitButton();
        });
    }

    public void HandleStartButton()
    {
        SceneManager.LoadScene(1);
    }

    public void HandleExitButton()
    {
        Application.Quit(0);
    }
}
