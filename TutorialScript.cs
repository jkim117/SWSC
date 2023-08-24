using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class TutorialScript : MonoBehaviour
{
    public Canvas menuCanvas;
    public PlayerInput playerInput;
    private bool menuActive = false;
    private float timer;
    private bool firstTutorialActive = false;
    public AudioSource tutorialAudio;
    public AudioClip[] tutorialCips;

    [SerializeField]
    private TextMeshProUGUI tutorialText = null;

    private int tutorialCounter = 0;

    private string tut0 = "WELCOME TO THE FLIGHT TUTORIAL. PRESS [ENTER] TO RESUME THE GAME AND PRESS [ENTER] AGAIN WHEN YOU ARE READY FOR THE NEXT BLOCK OF INSTRUCTION.";
    private string tut1 = "AIRCRAFT LIFT IS CONTROLLED REPULSORLIFTS IN STAR WARS - A SCI-FI ANTI-GRAVITY TECHNOLOGY. THE REPULSORLIFT LEVEL IS THE BLUE BAR IN THE BOTTOM LEFT OF THE SCREEN. BY DEFAULT THE REPULSORLIFT IS SET TO EXACTLY COUNTERACT THE FORCE OF GRAVITY OF THE PLANET YOU ARE ON IF THE SHIP IS EXACTLY LEVEL WITH THE GROUND. YOU CAN MANUALLY ADJUST THE STRENGTH OF THE REPULSORS WITH THE LEFT BRACKET KEY [ TO LOWER AND THE RIGHT BRACKET KEY ] TO RISE. PRESSING [LEFT CTRL] WILL RESET THE REPULSORS TO THEIR DEFAULT LEVEL. TO TAKE OFF, YOU WILL NEED TO INCREASE YOUR REPULSOR STRENGTH WITH THE RIGHT BRACKET KEY. PRESS [ENTER] TO RESUME THE GAME AND PRESS [ENTER] AGAIN WHEN YOU ARE READY FOR THE NEXT BLOCK OF INSTRUCTION.";
    private string tut2 = "PRESS [W] TO THROTTLE UP AND [D] TO THROTTLE DOWN. YOU CAN SEE YOUR CURRENT THROTTLE ON THE LEFT HAND SIDE OF THE SCREEN. SETTING FULL THROTTLE WILL ACTIVATE AFTERBURNERS WHICH WILL GIVE AN EXTRA BOOST OF SPEED BUT AT THE COST OF HIGH FUEL CONSUMPTION. PRESS [A] TO ROLL LEFT AND [D] TO ROLL RIGHT. THE MOUSE CONTROLS PITCH AND YAW. PRESS [ENTER] TO RESUME THE GAME AND PRESS [ENTER] AGAIN WHEN YOU ARE READY FOR THE NEXT BLOCK OF INSTRUCTION.";
    private string tut3 = "PRESSING [V] TOGGLES YOUR S-FOILS. HAVING YOUR S FOILS CLOSED GIVES A SLIGHT BOOST TO MANEUVERABILITY BUT ALSO PREVENTS YOU FROM INCREASING THE CHARGE TO YOUR LASERS.";
    private string tut4 = "THE TARGETING COMPUTER ALLOWS YOU TO EASILY IDENTIFY OBJECTS OF INTEREST AROUND THE BATTLEFIELD. PRESS [T] TO CYCLE THROUGH DIFFERENT TARGETING OPTIONS. YOU CAN TARGET ENEMY FIGHERS, ENEMY CAPITAL SHIPS, FRIENDLY FIGHTERS, FRIENDLY CAPITAL SHIPS, AND OBJECTIVES. PRESS [F] TO CYCLE THROUGH TARGETS OF A CERTAIN TYPE. PRESS [ENTER] TO RESUME THE GAME AND PRESS [ENTER] AGAIN WHEN YOU ARE READY FOR THE NEXT BLOCK OF INSTRUCTION.";
    private string tut5 = "SELECT A PARTICULAR TARGET. THE CIRCLES TO THE TOP LEFT AND RIGHT OF THE SCREEN ARE YOUR SCANNERS. THE SCANNER TO THE LEFT IS THE FORWARD SENSOR, SHOWING OBJECTS TO THE FRONT OF YOUR SHIP. THE SCANNER TO THE RIGHT IS THE REAR SENSOR, SHOWING TARGETS BEHIND YOU. FIGHTERS AND OBJECTIVES ARE MARKED BY DOTS. CAPTIAL SHIPS ARE MARKED BY ARROWS. ENEMY OBJECTS ARE RED AND FRIENDLY OBJECTS ARE GREEN. OBJECTIVES ARE YELLOW. THE OBJECT YOU ARE CURRENTLY TARGETING IS WHITE. WHEN YOUR CURRENT TARGET IS WITHIN YOUR VIEW, IT IS SURROUNDED BY A WHITE SQUARE. PRESS [ENTER] TO RESUME THE GAME AND PRESS [ENTER] AGAIN WHEN YOU ARE READY FOR THE NEXT BLOCK OF INSTRUCTION.";
    private string tut6 = "PLACING YOUR CURRENT TARGET WITHIN THE VICINITY OF YOUR CORSSHAIR WILL CAUSE YOUR TARGETING COMPUTER TO START LOCKING-ON. YOU CAN TOGGLE YOUR TARGETING COMPUTER ON/OFF BY PRESSING THE MIDDLE MOUSE BUTTON. THE CLOSER YOUR TARGET TO THE CENTER OF YOUR CROSSHAIR, THE FASTER YOU WILL LOCK ON. YOUR MOUSE SCROLL-WHEEL ALSO CONTROLS THE TARGETING CONVERGENCE POINT FOR YOUR LASERS/CURRENTLY SELECTED WEAPON AND ALSO AFFECTS THE SPEED THAT YOUR TARGETING COMPUTER LOCKS-ON. PRESS [ENTER] TO RESUME THE GAME AND PRESS [ENTER] AGAIN WHEN YOU ARE READY FOR THE NEXT BLOCK OF INSTRUCTION.";
    private string tut7 = "TO FIRE YOUR WEAPONS, CLICK THE LEFT-MOUSE BUTTON. IF YOU DO NOT HAVE A TARGET-LOCK, YOUR LASERS WILL CONVERGE AT THE DISTANCE YOUR CURRENT TARGETING CONVERGENCE IS SET TO (IN METERS). IF YOU DO HAVE A TARGET LOCK, YOUR TARGETING COMPUTER ADJUSTS THE TRAJECTORY OF YOUR LASERS BASED ON THE VELOCITY VECTOR OF YOUR TARGET AT THE TIME THE LASER IS FIRED. PRESS [Q] TO SWITCH YOUR WEAPONS. MISSILES/TORPEDOES WILL DUMBFIRE IF YOU DO NOT HAVE A LOCK. THEY WILL TRACK THEIR TARGETS IF YOU HAVE A LOCK. MISSILES HAVE BETTER TRACKING AND MOVE FASTER THAN TORPEDOES BUT ARE LESS POWERFUL. PRESSING [X] CHANGES THE LINK-STATUS OF YOUR LASERS OR TORPEDOES (WHETHER THEY FIRE TOGETHER OR SEPARATLEY). PRESS [ENTER] TO RESUME THE GAME AND PRESS [ENTER] AGAIN WHEN YOU ARE READY FOR THE NEXT BLOCK OF INSTRUCTION.";
    private string tut8 = "YOUR CURRENT LASER AMMO CAPACITY IS INDICATED BY THE RED BAR IN THE BOTTOM RIGHT OF THE SCREEN. AS YOU FIRE, YOU DEPLETE YOUR LASER CHARGE. TO INCREASE YOUR LASER CHARGE, YOU NEED TO HAVE ABOVE HALF POWER IN YOUR LASERS. TO ADJUST POWER IN YOUR LASERS, PRESS [2]. IF YOUR POWER IS LESS THAN HALF IN YOUR LASERS, YOU WILL SLOWLY LOSE LASER CHARGE. YOU CAN ADJUST POWER TO YOUR ENGINES AS WELL BY PRESSING [1]. YOU CAN INCREASE OR DECREASE OVERALL POWER USAGE BY USING THE [UP] AND [DOWN] DIRECTIONAL ARROWS RESPECTIVELY. HOLDING [2] AND CLICKING [1] WILL TRANSFER POWER FROM LASERS TO ENGINES AND VICE VERSA. THE MORE POWER IN YOUR ENGINES, THE FASTER AND MORE MANEUVERABLE YOUR FIGHTER. THE RED BAR IN THE BOTTOM LEFT OF THE SCREEN REPRESENTS YOUR CURRENT POWER LEVEL TO YOUR LASERS. THE BLUE BAR IN THE BOTTOM LEFT REPRESENTS YOUR CURRENT ENGINE POWER LEVEL. THE PURPLE BAR REPRESENTS YOUR OVERALL POWER USEAGE LEVEL. PRESS [ENTER] TO RESUME THE GAME AND PRESS [ENTER] AGAIN WHEN YOU ARE READY FOR THE NEXT BLOCK OF INSTRUCTION.";
    private string tut9 = "THE SAME POWER MANAGMENT RULES APPLY TO YOUR SHIELDS AS WELL. CONTROL YOUR SHIELD POWER LEVEL (BOTTOM LEFT GREEN BAR) BY PRESSING [3]. POWER ABOVE HALF WILL INCREASE YOUR SHIELD CHARGE. THE GREEN BARS SURROUNDING YOUR HEALTH IN THE BOTTOM RIGHT OF THE SCREEN SHOW YOUR SHIELDS. PRESSING [SPACEBAR] WILL TOGGLE WHETHER YOU WANT TO FOCUS YOUR SHIELDS FORWARD OR BACK. YOU CAN ALSO SHUNT RAW POWER FROM YOUR LASER CHARGE METER TO YOUR SHIELDS AND VICE VERSA BY PRESSING [F2] AND [F3] BUT THIS IS INEFFICIENT AND CAUSES A SLIGHT LOSS OF POWER EACH TIME YOU DO IT. PRESS [ENTER] TO RESUME THE GAME AND PRESS [ENTER] AGAIN WHEN YOU ARE READY FOR THE NEXT BLOCK OF INSTRUCTION.";
    private string tut10 = "IF YOU ARE BEING PURSUED BY AN ENEMY FIGHTER OR AN ENEMY FIGHTER HAS LAUNCHED A MISSILE AT YOU, YOU CAN SHUNT LASER POWER TO YOUR ELECTRONIC COUNTERMEASURES BY CLICKING [F4] OR SHUNT SHIELD POWER TO COUNTERMEASURES USING [F5]. THESE COUNTERMEASURES WILL TEMPORARILY JAM SENSORS AND MAKE YOU IMPOSSIBLE TO TARGET OR TRACK FOR A SHORT PERIOD. PRESS [ENTER] TO RESUME THE GAME AND PRESS [ENTER] AGAIN WHEN YOU ARE READY FOR THE NEXT BLOCK OF INSTRUCTION.";
    private string tut11 = "CLICKING [LEFT SHIFT] ALLOWS YOU TO LOOK BEHIND YOUR FIGHTER WHICH IS USEFUL WHEN BEING PURSUED BY ENEMY FIGHTERS. CLICKING [ESC] ALLOWS YOU TO TOGGLE THE PAUSE MENU. PRESS [ENTER] TO RESUME THE GAME AND PRESS [ENTER] AGAIN WHEN YOU ARE READY FOR THE NEXT BLOCK OF INSTRUCTION.";
    private string tut12 = "LANDING OR GETTING CLOSE TO YOUR AIRBASE WILL ALLOW YOU TO SWITCH FIGHTERS USING THE [TAB] KEY. IF YOU SWITCH TO A B-WING, HOLDING THE RIGHT MOUSE BUTTON WHILE ROLLING YOUR FIGHTER ([A] OR [D]) ACTIVATES YOUR B-WING'S GYRO COCKPIT CONTROL, ALLOWING YOU TO SPIN YOUR FIGHTER WITHOUT SPINNING THE COCKPIT. PRESS [ENTER] TO RESUME THE GAME AND PRESS [ENTER] AGAIN WHEN YOU ARE READY FOR THE NEXT BLOCK OF INSTRUCTION.";
    private string tut13 = "IF YOU TARGET THE IMPERIAL STAR DESTROYER, YOU WILL NOTICE THAT IT IS A SHIELDED TARGET. CAPTIAL SHIPS HAVE POWERFUL SHIELDS THAT NEED TO BE BROUGHT DOWN BEFORE ANY LASTING DAMAGE CAN BE DONE. PRESSING [Y], YOU CAN CYCLE THROUGH THE STAR DESTROYER'S SUBCOMPONENTS. DESTROYING THE ENGINES WILL IMPACT HOW FAST THE STAR DESTROYER MOVES AND MANEUVERS. DESTROYING THE BRIDGE IMPACTS THE FREQUENCY AND ACCURACY OF THE TURBOLASERS. DESTROYING THE SHIELD GENERATORS PREVENTS THE SHIELDS FROM RECHARGING. DESTROYING THE POWER GENERATOR CAUSES SUBSEQUENT DAMAGE TO THE STAR DESTROYER TO GET A 50% DAMAGE BUFF. DESTROYING THE HANGAR PREVENTS TIE FIGHTERS FROM CONTINUING TO SPAWN FROM THE STAR DESTROYER.";
    private bool tutorialdone = false;

    void OnTutorialNext()
    {
        if (CrossSceneValues.tutorialMode)
        {
            if (!menuActive && !tutorialdone)
            {
                //AudioListener.pause = true;
                Cursor.visible = true;
                menuActive = true;
                playerInput.DeactivateInput();
                menuCanvas.gameObject.SetActive(true);
                Time.timeScale = 0;
                CrossSceneValues.tutorialActive = true;

                if (tutorialCounter == 0)
                {
                    tutorialText.text = tut0;
                    tutorialCounter++;
                    tutorialAudio.PlayOneShot(tutorialCips[0]);
                }
                else if (tutorialCounter == 1)
                {
                    tutorialText.text = tut1;
                    tutorialCounter++;
                    tutorialAudio.PlayOneShot(tutorialCips[1]);
                }
                else if (tutorialCounter == 2)
                {
                    tutorialText.text = tut2;
                    tutorialCounter++;
                    tutorialAudio.PlayOneShot(tutorialCips[2]);
                }
                else if (tutorialCounter == 3)
                {
                    tutorialText.text = tut3;
                    tutorialCounter++;
                    tutorialAudio.PlayOneShot(tutorialCips[3]);
                }
                else if (tutorialCounter == 4)
                {
                    tutorialText.text = tut4;
                    tutorialCounter++;
                    tutorialAudio.PlayOneShot(tutorialCips[4]);
                }
                else if (tutorialCounter == 5)
                {
                    tutorialText.text = tut5;
                    tutorialCounter++;
                    tutorialAudio.PlayOneShot(tutorialCips[5]);
                }
                else if (tutorialCounter == 6)
                {
                    tutorialText.text = tut6;
                    tutorialCounter++;
                    tutorialAudio.PlayOneShot(tutorialCips[6]);
                }
                else if (tutorialCounter == 7)
                {
                    tutorialText.text = tut7;
                    tutorialCounter++;
                    tutorialAudio.PlayOneShot(tutorialCips[7]);
                }
                else if (tutorialCounter == 8)
                {
                    tutorialText.text = tut8;
                    tutorialCounter++;
                    tutorialAudio.PlayOneShot(tutorialCips[8]);
                }
                else if (tutorialCounter == 9)
                {
                    tutorialText.text = tut9;
                    tutorialCounter++;
                    tutorialAudio.PlayOneShot(tutorialCips[9]);
                }
                else if (tutorialCounter == 10)
                {
                    tutorialText.text = tut10;
                    tutorialCounter++;
                    tutorialAudio.PlayOneShot(tutorialCips[10]);
                }
                else if (tutorialCounter == 11)
                {
                    tutorialText.text = tut11;
                    tutorialCounter++;
                    tutorialAudio.PlayOneShot(tutorialCips[11]);
                }
                else if (tutorialCounter == 12)
                {
                    tutorialText.text = tut12;
                    tutorialCounter++;
                    tutorialAudio.PlayOneShot(tutorialCips[12]);
                }
                else if (tutorialCounter == 13)
                {
                    tutorialText.text = tut13;
                    tutorialCounter++;
                    tutorialAudio.PlayOneShot(tutorialCips[13]);
                    tutorialdone = true;
                }
                else
                {
                    tutorialText.text = "TEST TEXT";
                }

                
            }
            else
            {
                //AudioListener.pause = false;
                Cursor.visible = false;
                menuActive = false;
                playerInput.ActivateInput();
                menuCanvas.gameObject.SetActive(false);
                Time.timeScale = 1;
                CrossSceneValues.tutorialActive = false;
            }
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        menuCanvas.gameObject.SetActive(false);
        if (!CrossSceneValues.tutorialMode)
        {
            Destroy(this.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (CrossSceneValues.tutorialMode)
        {
            timer += Time.deltaTime;
            if (timer > 1.0 && !firstTutorialActive)
            {
                firstTutorialActive = true;
                OnTutorialNext();
            }
        }
        
    }
}