using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameMenuScript : MonoBehaviour
{
    public Canvas menuCanvas;
    public Canvas backGroundCanvas;
    public PlayerInput playerInput;
    private bool menuActive = false;
    private string selectedPlanet;

    public Button changeShipButton;

    public void ChangeShip()
    {
        GameObject.Find("SceneInitializer").GetComponent<SceneInitializer>().SwapShip();

        if (!CrossSceneValues.tutorialMode)
        {
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
        
        /*AudioListener.pause = false;
        Time.timeScale = 1;
        CrossSceneValues.inGameMenu = false;
        playerInput.ActivateInput();
        CrossSceneValues.currentPlanet = selectedPlanet;
        SceneManager.LoadScene(selectedPlanet);*/
    }

    void OnInGameMenu()
    {
        if (Vector3.Distance(SpawnGameAssets.calculateAirbaseUnityCoord(CrossSceneValues.currentAirbase), CrossSceneValues.player.transform.position) > 400)
        {
            return;
        }
        if (CrossSceneValues.tutorialActive)
        {
            return;
        }
        if(!menuActive)
        {
            AudioListener.pause = true;
            CrossSceneValues.inGameMenu = true;
            Cursor.visible = true;
            menuActive = true;
            playerInput.DeactivateInput();
            menuCanvas.gameObject.SetActive(true);
            backGroundCanvas.gameObject.SetActive(true);
            Time.timeScale = 0;
        }
        else
        {
            AudioListener.pause = false;
            CrossSceneValues.inGameMenu = false;
            Cursor.visible = false;
            menuActive = false;
            playerInput.ActivateInput();
            menuCanvas.gameObject.SetActive(false);
            backGroundCanvas.gameObject.SetActive(false);
            Time.timeScale = 1;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //changeShipButton.interactable = false;
        selectedPlanet = CrossSceneValues.currentPlanet;
        menuCanvas.gameObject.SetActive(false);
        backGroundCanvas.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
