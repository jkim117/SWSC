using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class PauseMenuScript : MonoBehaviour
{
    public Canvas menuCanvas;
    public Canvas backGroundCanvas;
    public PlayerInput playerInput;
    public PlayerInput inGameMenuPlayerInput;
    private bool menuActive = false;

    public TMP_Dropdown difficutlyDropdown;
    public Slider sensitivitySlider;
    public Toggle yToggle;
    public Toggle xToggle;

    public void OnYInvert()
    {
        CrossSceneValues.invertY = yToggle.isOn;
        SaveSystem.universalData.yInvert = yToggle.isOn;
        SaveSystem.saveUniversalData();
    }
    public void OnXInvert()
    {
        CrossSceneValues.invertX = xToggle.isOn;
        SaveSystem.universalData.xInvert = xToggle.isOn;
        SaveSystem.saveUniversalData();
    }
    public void OnSensitivitySlider()
    {
        CrossSceneValues.sensitivity = sensitivitySlider.value;
        SaveSystem.universalData.sensitivity = sensitivitySlider.value;
        SaveSystem.saveUniversalData();
    }

    public void OnDifficultyChoice()
    {
        CrossSceneValues.difficulty = difficutlyDropdown.value;
        GameData newSave = new GameData();
        switch (SaveSystem.currentLoadedGame)
        {
            case 0:
                SaveSystem.data0 = newSave;
                break;
            case 1:
                SaveSystem.data1 = newSave;
                break;
            case 2:
                SaveSystem.data2 = newSave;
                break;
            case 3:
                SaveSystem.data3 = newSave;
                break;
            default:
                SaveSystem.data0 = newSave;
                break;
        }
        SaveSystem.save(SaveSystem.currentLoadedGame);
    }

    public void Resume()
    {
        if (!CrossSceneValues.inGameMenu)
        {
            AudioListener.pause = false;
            Cursor.visible = false;
            playerInput.ActivateInput();
            Time.timeScale = 1;
        }
        menuActive = false;
        inGameMenuPlayerInput.ActivateInput();
        menuCanvas.gameObject.SetActive(false);
        backGroundCanvas.gameObject.SetActive(false);
    }
    public void MainMenu()
    {
        AudioListener.pause = false;
        Time.timeScale = 1;
        playerInput.ActivateInput();
        inGameMenuPlayerInput.ActivateInput();
        SceneManager.LoadScene("C1_Menu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    void OnMenu()
    {
        if (CrossSceneValues.tutorialActive)
        {
            return;
        }
        if(!menuActive)
        {
            AudioListener.pause = true;
            Cursor.visible = true;
            menuActive = true;
            playerInput.DeactivateInput();
            inGameMenuPlayerInput.DeactivateInput();
            menuCanvas.gameObject.SetActive(true);
            backGroundCanvas.gameObject.SetActive(true);
            Time.timeScale = 0;
        }
        else
        {
            if (!CrossSceneValues.inGameMenu)
            {
                AudioListener.pause = false;
                Cursor.visible = false;
                playerInput.ActivateInput();
                Time.timeScale = 1;
            }
            menuActive = false;
            inGameMenuPlayerInput.ActivateInput();
            menuCanvas.gameObject.SetActive(false);
            backGroundCanvas.gameObject.SetActive(false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        menuCanvas.gameObject.SetActive(false);
        backGroundCanvas.gameObject.SetActive(false);
        difficutlyDropdown.value = CrossSceneValues.difficulty;
        yToggle.isOn = CrossSceneValues.invertY;
        xToggle.isOn = CrossSceneValues.invertX;
        sensitivitySlider.value = CrossSceneValues.sensitivity;

        if (CrossSceneValues.tutorialMode)
        {
            difficutlyDropdown.interactable = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
