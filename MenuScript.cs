using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuScript : MonoBehaviour
{

    public Canvas menuCanvas;
    public Canvas loadGameCanvas;
    public Canvas newGameCanvas;

    //private bool menuActive = false;
    private int shipToLoad = 0;
    private string[] availableShips = { "X", "A", "Y", "B", "LN", "IN", "SA", "D" };

    public Button newRebelGameButton;
    public Button newEmpireGameButton;

    public Button loadGame1;
    public Button deleteGame1;
    public TextMeshProUGUI game1Text;

    public Button loadGame2;
    public Button deleteGame2;
    public TextMeshProUGUI game2Text;

    public Button loadGame3;
    public Button deleteGame3;
    public TextMeshProUGUI game3Text;

    public Button loadGame4;
    public Button deleteGame4;
    public TextMeshProUGUI game4Text;

    public TMP_Dropdown difficultyDropdown;

    // Start is called before the first frame update
    void Start()
    {
        PlayerController.totalxShift = -30000;
        PlayerController.totalzShift = -30000;
        SaveSystem.loadAllGameSlots();
        CrossSceneValues.tutorialMode = false;
    }

    public void OnDifficultyChoice()
    {
        CrossSceneValues.difficulty = difficultyDropdown.value;
    }

    public void LoadGameTutorial()
    {
        CrossSceneValues.tutorialMode = true;
        CrossSceneValues.difficulty = 0;
        CrossSceneValues.shipToLoad = availableShips[0];
        CrossSceneValues.worldSeed = 0;
        CrossSceneValues.currentAirbase = 0;
        CrossSceneValues.airbaseValues[0] = true;

        SceneManager.LoadScene("C2");
    }

    public void NewGameMenu()
    {
        menuCanvas.gameObject.SetActive(false);
        newGameCanvas.gameObject.SetActive(true);

        if (SaveSystem.universalData.numGaveSaves >= 4)
        {
            newRebelGameButton.interactable = false;
            newEmpireGameButton.interactable = false;
        }
        else
        {
            newRebelGameButton.interactable = true;
            newEmpireGameButton.interactable = true;
        }
    }

    public void LoadGameMenu()
    {
        
        menuCanvas.gameObject.SetActive(false);
        loadGameCanvas.gameObject.SetActive(true);

        loadGame1.interactable = false;
        deleteGame1.interactable = false;
        game1Text.text = "";
        loadGame2.interactable = false;
        deleteGame2.interactable = false;
        game2Text.text = "";
        loadGame3.interactable = false;
        deleteGame3.interactable = false;
        game3Text.text = "";
        loadGame4.interactable = false;
        deleteGame4.interactable = false;
        game4Text.text = "";


        if (SaveSystem.universalData.numGaveSaves > 0)
        {
            loadGame1.interactable = true;
            deleteGame1.interactable = true;
            if (SaveSystem.data0.rebelPlayer)
            {
                game1Text.text = "New Republic";
            }
            else
            {
                game1Text.text = "Empire";
            }

            if (SaveSystem.data0.difficulty == 0)
            {
                game1Text.text += " - Easy";
            }
            else if (SaveSystem.data0.difficulty == 1)
            {
                game1Text.text += " - Normal";
            }
            else
            {
                game1Text.text += " - Hard";
            }

        }
        if (SaveSystem.universalData.numGaveSaves > 1)
        {
            loadGame2.interactable = true;
            deleteGame2.interactable = true;
            if (SaveSystem.data1.rebelPlayer)
            {
                game2Text.text = "New Republic";
            }
            else
            {
                game2Text.text = "Empire";
            }

            if (SaveSystem.data1.difficulty == 0)
            {
                game2Text.text += " - Easy";
            }
            else if (SaveSystem.data1.difficulty == 1)
            {
                game2Text.text += " - Normal";
            }
            else
            {
                game2Text.text += " - Hard";
            }
        }
        if (SaveSystem.universalData.numGaveSaves > 2)
        {
            loadGame3.interactable = true;
            deleteGame3.interactable = true;
            if (SaveSystem.data2.rebelPlayer)
            {
                game3Text.text = "New Republic";
            }
            else
            {
                game3Text.text = "Empire";
            }

            if (SaveSystem.data2.difficulty == 0)
            {
                game3Text.text += " - Easy";
            }
            else if (SaveSystem.data2.difficulty == 1)
            {
                game3Text.text += " - Normal";
            }
            else
            {
                game3Text.text += " - Hard";
            }
        }
        if (SaveSystem.universalData.numGaveSaves > 3)
        {
            loadGame4.interactable = true;
            deleteGame4.interactable = true;
            if (SaveSystem.data3.rebelPlayer)
            {
                game4Text.text = "New Republic";
            }
            else
            {
                game4Text.text = "Empire";
            }

            if (SaveSystem.data3.difficulty == 0)
            {
                game4Text.text += " - Easy";
            }
            else if (SaveSystem.data3.difficulty == 1)
            {
                game4Text.text += " - Normal";
            }
            else
            {
                game4Text.text += " - Hard";
            }
        }


    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void startNewGame(bool rebelPlayer)
    {
        if (rebelPlayer)
        {
            CrossSceneValues.shipToLoad = availableShips[0];
        }
        else
        {
            CrossSceneValues.shipToLoad = availableShips[4];
        }
        Random.InitState(System.DateTime.Now.Millisecond);
        CrossSceneValues.worldSeed = Random.Range(0, 10000);
        CrossSceneValues.currentAirbase = 0;
        CrossSceneValues.airbaseValues[0] = true;

        if (SaveSystem.universalData.numGaveSaves > 4)
        {
            Debug.LogError("No more game slots");
        }
        SaveSystem.universalData.numGaveSaves++;
        switch (SaveSystem.universalData.numGaveSaves)
        {
            case 1:
                SaveSystem.data0 = new GameData();
                SaveSystem.save(0);
                SaveSystem.currentLoadedGame = 0;
                break;
            case 2:
                SaveSystem.data1 = new GameData();
                SaveSystem.save(1);
                SaveSystem.currentLoadedGame = 1;
                break;
            case 3:
                SaveSystem.data2 = new GameData();
                SaveSystem.save(2);
                SaveSystem.currentLoadedGame = 2;
                break;
            case 4:
                SaveSystem.data3 = new GameData();
                SaveSystem.save(3);
                SaveSystem.currentLoadedGame = 3;
                break;
            default:
                SaveSystem.data0 = new GameData();
                SaveSystem.save(0);
                SaveSystem.currentLoadedGame = 0;
                break;
        }
        
        // world map generation, generate airbase locations with their initial affiliation

        //CrossSceneValues.loadFighters = true;
        SaveSystem.saveUniversalData();
        SceneManager.LoadScene("C2");
    }

    public void LoadGame(int gameLoad)
    {
        switch (gameLoad)
        {
            case 0:
                CrossSceneValues.shipToLoad = SaveSystem.data0.shipToLoad;
                CrossSceneValues.worldSeed = SaveSystem.data0.worldSeed;
                CrossSceneValues.difficulty = SaveSystem.data0.difficulty;
                CrossSceneValues.airbaseValues = SaveSystem.data0.airbaseValues;
                CrossSceneValues.currentAirbase = SaveSystem.data0.currentAirbase;
                break;
            case 1:
                CrossSceneValues.shipToLoad = SaveSystem.data1.shipToLoad;
                CrossSceneValues.worldSeed = SaveSystem.data1.worldSeed;
                CrossSceneValues.difficulty = SaveSystem.data1.difficulty;
                CrossSceneValues.airbaseValues = SaveSystem.data1.airbaseValues;
                CrossSceneValues.currentAirbase = SaveSystem.data1.currentAirbase;
                break;
            case 2:
                CrossSceneValues.shipToLoad = SaveSystem.data2.shipToLoad;
                CrossSceneValues.worldSeed = SaveSystem.data2.worldSeed;
                CrossSceneValues.difficulty = SaveSystem.data2.difficulty;
                CrossSceneValues.airbaseValues = SaveSystem.data2.airbaseValues;
                CrossSceneValues.currentAirbase = SaveSystem.data2.currentAirbase;
                break;
            case 3:
                CrossSceneValues.shipToLoad = SaveSystem.data3.shipToLoad;
                CrossSceneValues.worldSeed = SaveSystem.data3.worldSeed;
                CrossSceneValues.difficulty = SaveSystem.data3.difficulty;
                CrossSceneValues.airbaseValues = SaveSystem.data3.airbaseValues;
                CrossSceneValues.currentAirbase = SaveSystem.data3.currentAirbase;
                break;
            default:
                CrossSceneValues.shipToLoad = SaveSystem.data0.shipToLoad;
                CrossSceneValues.worldSeed = SaveSystem.data0.worldSeed;
                CrossSceneValues.difficulty = SaveSystem.data0.difficulty;
                CrossSceneValues.airbaseValues = SaveSystem.data0.airbaseValues;
                CrossSceneValues.currentAirbase = SaveSystem.data0.currentAirbase;
                break;
        }
        SaveSystem.currentLoadedGame = gameLoad;
        //CrossSceneValues.loadFighters = true;
        SceneManager.LoadScene("C2");
    }

    public void DeleteGame(int gameToDelete)
    {
        switch (SaveSystem.universalData.numGaveSaves)
        {
            case 1:
                break;
            case 2:
                if (gameToDelete == 0)
                {
                    SaveSystem.data0 = SaveSystem.data1;
                } 
                break;
            case 3:
                if (gameToDelete == 0)
                {
                    SaveSystem.data0 = SaveSystem.data1;
                    SaveSystem.data1 = SaveSystem.data2;
                }
                else if (gameToDelete == 1)
                {
                    SaveSystem.data1 = SaveSystem.data2;
                }
                break;
            case 4:
                if (gameToDelete == 0)
                {
                    SaveSystem.data0 = SaveSystem.data1;
                    SaveSystem.data1 = SaveSystem.data2;
                    SaveSystem.data2 = SaveSystem.data3;
                }
                else if (gameToDelete == 1)
                {
                    SaveSystem.data1 = SaveSystem.data2;
                    SaveSystem.data2 = SaveSystem.data3;
                }
                else if (gameToDelete == 2)
                {
                    SaveSystem.data2 = SaveSystem.data3;
                }
                break;
            default:
                break;
        }

        SaveSystem.universalData.numGaveSaves--;
        SaveSystem.saveUniversalData();

        LoadGameMenu();
    }

    public void LoadToMainMenu()
    {
        loadGameCanvas.gameObject.SetActive(false);
        menuCanvas.gameObject.SetActive(true);
    }
    
    public void NewToMainMenu()
    {
        newGameCanvas.gameObject.SetActive(false);
        menuCanvas.gameObject.SetActive(true);
    }

    /*public void SwapShip()
    {
        shipToLoad = (shipToLoad + 1) % availableShips.Length;
        Destroy(displayShipObject);
        displayShipObject = GameObject.Instantiate(shipObjects[shipToLoad]);
        displayShipObject.transform.position = new Vector3(1000, 2000, 0);
        displayShipObject.transform.Rotate(new Vector3(0, 0, 135));
    }*/

}
