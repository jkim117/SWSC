using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public ShipController shipController;

    [SerializeField]
    private TextMeshProUGUI warningText = null;

    [SerializeField]
    private Slider throttleAmount = null;

    [SerializeField]
    private TextMeshProUGUI speedText = null;

    [SerializeField]
    private Slider overallPowerAmount = null;

    [SerializeField]
    private Slider enginePowerAmount = null;

    [SerializeField]
    private Slider laserPowerAmount = null;

    [SerializeField]
    private Slider shieldPowerAmount = null;

    [SerializeField]
    private TextMeshProUGUI targetText = null;

    [SerializeField]
    private Slider laserChargeAmount = null;

    [SerializeField]
    private Slider jammerAmount = null;

    [SerializeField]
    private Slider repulsorAmount = null;

    [SerializeField]
    private Slider rearShieldAmount = null;

    [SerializeField]
    private Slider rearOverShieldAmount = null;

    [SerializeField]
    private Slider frontShieldAmount = null;

    [SerializeField]
    private Slider frontOverShieldAmount = null;

    [SerializeField]
    private Slider shipHealthAmount = null;

    private string speedTextMessage = " KPH";
    private float sliderFillSpeed = 0.25f;

    private float currentThrottle = 1;
    public int currentOverallPower;
    private int currentEnginePower = 4;
    private int currentLaserPower = 2;
    private int currentShieldPower = 2;
    private float timeTillFill = 0;

    private float currentLaserCharge = 1f;
    private float currentRepulsor = 0;
    private float currentJammer = 0f;
    private float timeTillFillRepulsor = 0;

    private float frontShield = 20f;
    private float rearShield = 20f;
    private float frontOverShield = 10f;
    private float rearOverShield = 10f;
    private float shipHealth = 100f;
    private float timeTillFillFrontShield = 0f;
    private float timeTillFillFrontOverShield = 0f;
    private float timeTillFillRearShield = 0f;
    private float timeTillFillRearOverShield = 0f;

    private Transform frontShieldHighlight;
    private Transform rearShieldHighlight;

    public GameObject fighterSensorDotPrefab;
    public GameObject capShipSensorDotPrefab;

    private List<GameObject> enemyFighterDotList = new List<GameObject>();
    private List<GameObject> enemyCapShipDotList = new List<GameObject>();
    private List<GameObject> friendlyFighterDotList = new List<GameObject>();
    private List<GameObject> friendlyCapShipDotList = new List<GameObject>();
    private List<GameObject> objectiveList = new List<GameObject>();
    private List<GameObject> projectileList = new List<GameObject>();

    private List<List<GameObject>> targetDotList = new List<List<GameObject>>();
    private int prevTargetListIndex = 0;
    private int prevTargetSubListIndex = -1;
    private Color prevTargetColor;

    private Transform forwardSensor;
    private Transform rearSensor;

    // Start is called before the first frame update
    void Start()
    {
        
        
        frontShieldHighlight = transform.Find("FrontShieldParent").Find("FrontShieldHighlight");
        rearShieldHighlight = transform.Find("RearShieldParent").Find("RearShieldHighlight");
        frontShieldHighlight.gameObject.SetActive(false);
        rearShieldHighlight.gameObject.SetActive(false);
        forwardSensor = transform.Find("ForwardSensorParent");
        rearSensor = transform.Find("RearSensorParent");

        targetDotList.Add(enemyFighterDotList);
        targetDotList.Add(enemyCapShipDotList);
        targetDotList.Add(friendlyFighterDotList);
        targetDotList.Add(friendlyCapShipDotList);
        targetDotList.Add(objectiveList);
        targetDotList.Add(projectileList);
        updateSensorDots();
    }

    private void updateOverallPower()
    {
        int newOverallPower = shipController.overallPower;
        if (currentOverallPower != newOverallPower)
        {
            currentOverallPower = newOverallPower;
            overallPowerAmount.value = currentOverallPower;
        }
    }

    private void updateEnginePower()
    {
        int newEnginePower = shipController.enginePower;
        if (currentEnginePower != newEnginePower)
        {
            currentEnginePower = newEnginePower;
            enginePowerAmount.value = currentEnginePower;
        }
    }

    private void updateLaserPower()
    {
        int newLaserPower = shipController.laserPower;
        if (currentLaserPower != newLaserPower)
        {
            currentLaserPower = newLaserPower;
            laserPowerAmount.value = currentLaserPower;
        }
    }

    private void updateShieldPower()
    {
        int newShieldPower = shipController.shieldPower;
        if (currentShieldPower != newShieldPower)
        {
            currentShieldPower = newShieldPower;
            shieldPowerAmount.value = currentShieldPower;
        }
    }

    private void updateThrottle()
    {
        float newThrottle = shipController.throttleLevel;
        if (currentThrottle != newThrottle)
        {
            currentThrottle = Mathf.Lerp(currentThrottle, newThrottle, timeTillFill);
            timeTillFill += sliderFillSpeed * Time.deltaTime;
        }
        throttleAmount.value = currentThrottle;
    }

    private void updateLaserCharge()
    {
        float newLaserCharge = shipController.getLaserCharge();
        if (currentLaserCharge != newLaserCharge)
        {
            currentLaserCharge = newLaserCharge;
            laserChargeAmount.value = currentLaserCharge;
        }
    }
    private void updateJammer()
    {
        float newJammer = shipController.getJammer();
        if (currentJammer != newJammer)
        {
            currentJammer = newJammer;
            jammerAmount.value = currentJammer;
        }
    }

    private void updateFrontShield()
    {
        float newFrontShield = shipController.getFrontShield();
        if (frontShield != newFrontShield)
        {
            frontShield = Mathf.Lerp(frontShield, newFrontShield, timeTillFillFrontShield);
            timeTillFillFrontShield += sliderFillSpeed * Time.deltaTime;
            frontShieldAmount.value = frontShield;
        }
        
    }
    private void updateFrontOverShield()
    {
        float newFrontOverShield = shipController.getFrontOverShield();
        if (frontOverShield != newFrontOverShield)
        {
            frontOverShield = Mathf.Lerp(frontOverShield, newFrontOverShield, timeTillFillFrontOverShield);
            timeTillFillFrontOverShield += sliderFillSpeed * Time.deltaTime;
            frontOverShieldAmount.value = frontOverShield;
        }
        
    }
    private void updateRearShield()
    {
        float newRearShield = shipController.getRearShield();
        if (rearShield != newRearShield)
        {
            rearShield = Mathf.Lerp(rearShield, newRearShield, timeTillFillRearShield);
            timeTillFillRearShield += sliderFillSpeed * Time.deltaTime;
            rearShieldAmount.value = rearShield;
        }
        
    }
    private void updateRearOverShield()
    {
        float newRearOverShield = shipController.getRearOverShield();
        if (rearOverShield != newRearOverShield)
        {
            rearOverShield = Mathf.Lerp(rearOverShield, newRearOverShield, timeTillFillRearOverShield);
            timeTillFillRearOverShield += sliderFillSpeed * Time.deltaTime;
            rearOverShieldAmount.value = rearOverShield;
        }
        
    }
    private void updateShipHealth()
    {
        float newShipHealth = shipController.getHealthPercentage();
        if (shipHealth != newShipHealth)
        {
            shipHealth = newShipHealth;
            shipHealthAmount.value = shipHealth;
        }
    }

    private void updateRepulsor()
    {
        float newRepulsor = shipController.getRepulsorPercentage();
        if (currentRepulsor != newRepulsor)
        {
            currentRepulsor = Mathf.Lerp(currentRepulsor, newRepulsor, timeTillFillRepulsor);
            timeTillFillRepulsor += sliderFillSpeed * Time.deltaTime;
        }
        repulsorAmount.value = currentRepulsor;
    }

    private Vector2 convertRealSpaceToSensorLoc(Vector3 loc, out bool forwardSensor, out float distance)
    {
        // local X position of sensor dot is vertical with 0 being center, 100 being top
        // local Y position of sensor dot is horizontal with 0 being center, 100 being left
        Vector3 target_local_pos = shipController.transform.InverseTransformVector(loc - shipController.getPosition()); // target coordinates in terms of local coordinates
        distance = Vector3.Magnitude(loc - shipController.getPosition());


        Vector3 target_local_xz = new Vector3(target_local_pos.x, 0, target_local_pos.z);

        float targetAngleRad = Vector3.Angle(-Vector3.up, target_local_pos);
        float targetAngleTheta = Vector3.Angle(Vector3.right, target_local_xz);

        if (target_local_pos.y > 0) // target is in rear
        {
            forwardSensor = false;
            targetAngleRad = -(targetAngleRad - 180);
        }
        else // target is in front
        {
            forwardSensor = true;
        }
        float targetRad = Mathf.Lerp(0, 100, targetAngleRad / 90f);

        if (target_local_pos.z >= 0) // target is above
        {

        }
        else // target is below
        {
            targetAngleTheta = 360 - targetAngleTheta;
        }
        float x = targetRad * Mathf.Cos(Mathf.Deg2Rad * targetAngleTheta);
        float y = targetRad * Mathf.Sin(Mathf.Deg2Rad * targetAngleTheta);

        
        return new Vector2(y, -x);
    }
    private void updateSensorDots()
    {
        for (int j = 0; j < CrossSceneValues.targetList.Count; j++)
        {
            if ((j != 2 && targetDotList[j].Count < CrossSceneValues.targetList[j].Count) || (j == 2 && targetDotList[j].Count < CrossSceneValues.targetList[j].Count - 1))
            {
                int i = 0;
                if (j == 2)
                {
                    i = 1;
                }
                for (; i < CrossSceneValues.targetList[j].Count - targetDotList[j].Count; i++)
                {
                    GameObject fighterSensorDot;
                    if (j == 1 || j == 3)
                    {
                        fighterSensorDot = GameObject.Instantiate(capShipSensorDotPrefab, forwardSensor.position, forwardSensor.rotation);
                    }
                    else
                    {
                        fighterSensorDot = GameObject.Instantiate(fighterSensorDotPrefab, forwardSensor.position, forwardSensor.rotation);
                    }   
                    
                    fighterSensorDot.transform.SetParent(forwardSensor);
                    targetDotList[j].Add(fighterSensorDot);
                }
            }
            else if ((j != 2 && targetDotList[j].Count > CrossSceneValues.targetList[j].Count) || j == 2 && targetDotList[j].Count > CrossSceneValues.targetList[j].Count - 1)
            {
                int i = 0;
                if (j == 2)
                {
                    i = -1;
                }
                for (; i < targetDotList[j].Count - CrossSceneValues.targetList[j].Count; i++)
                {
                    GameObject targetDotToRemove = targetDotList[j][0];
                    targetDotList[j].RemoveAt(0);
                    Destroy(targetDotToRemove);
                }
            }
            //Debug.Log(targetDotList[j].Count);
            //Debug.Log(shipController.targetList[j].Count);
            int k = 0;
            for (int i = 0; i < CrossSceneValues.targetList[j].Count; i++)
            {
                if (CrossSceneValues.targetList[j][i] == shipController)
                {
                    continue;
                }
                bool usingForwardSensor;
                float distance;
                bool jammed;
                Vector2 localPositionOnSensor = convertRealSpaceToSensorLoc(CrossSceneValues.targetList[j][i].getJamPosition(true, out jammed), out usingForwardSensor, out distance);

                if (j == shipController.targetListIndex && CrossSceneValues.targetList[j][i] == shipController.getTarget())
                {
                    targetDotList[j][k].GetComponent<Image>().color = Color.white;
                }
                else if (j == 0 || j == 1)
                {
                    targetDotList[j][k].GetComponent<Image>().color = Color.red;
                }
                else if (j == 2 || j == 3)
                {
                    targetDotList[j][k].GetComponent<Image>().color = Color.green;
                }
                else
                {
                    targetDotList[j][k].GetComponent<Image>().color = Color.yellow;
                }

                if (distance > 1000)
                {
                    targetDotList[j][k].transform.localScale = new Vector3(0.05f, 0.05f, 1);
                }
                else
                {
                    targetDotList[j][k].transform.localScale = new Vector3(0.1f, 0.1f, 1);
                }
                if (usingForwardSensor)
                {
                    targetDotList[j][k].transform.SetParent(forwardSensor);
                }
                else
                {
                    targetDotList[j][k].transform.SetParent(rearSensor);
                }
                targetDotList[j][k].transform.localPosition = localPositionOnSensor;

                k++;
            }
        }

        /*if (shipController.targetListIndex != prevTargetListIndex || shipController.targetSubListIndex != prevTargetSubListIndex)
        {
            if (prevTargetSubListIndex != -1)
            {
                targetDotList[prevTargetListIndex][prevTargetSubListIndex].GetComponent<Image>().color = prevTargetColor;
            }

            prevTargetListIndex = shipController.targetListIndex;
            prevTargetSubListIndex = shipController.targetSubListIndex;
            if (shipController.targetSubListIndex != -1)
            {
                prevTargetColor = targetDotList[shipController.targetListIndex][shipController.targetSubListIndex].GetComponent<Image>().color;
                targetDotList[shipController.targetListIndex][shipController.targetSubListIndex].GetComponent<Image>().color = Color.white;
            }
        }*/
        
    }

    // Update is called once per frame
    void Update()
    {
        if (shipController.incomingWarning > 0)
        {
            warningText.text = "<<INCOMING MISSILE>>";
        }
        else if (shipController.lockedOnWarning > 0)
        {
            warningText.text = "<<ENEMY TARGET LOCK>>";
        }
        else if (shipController.lockingOnWarning > 0)
        {
            warningText.text = "<<ENEMY LOCKING ON>>";
        }
        else
        {
            warningText.text = "";
        }

        ShipController target = shipController.getTarget();
        
        string targetType;
        if (shipController.targetListIndex == 0)
        {
            targetType = "TARGETING: ENEMY FIGHTERS";
        }
        else if (shipController.targetListIndex == 1)
        {
            targetType = "TARGETING: ENEMY CAPITAL SHIPS";
        }
        else if (shipController.targetListIndex == 2)
        {
            targetType = "TARGETING: FRIENDLY FIGHTERS";
        }
        else if (shipController.targetListIndex == 3)
        {
            targetType = "TARGETING: FRIENDLY CAPITAL SHIPS";
        }
        else if (shipController.targetListIndex == 4)
        {
            targetType = "TARGETING: OBJECTIVES";

        }
        else
        {
            targetType = "TARGETING: PROJECTILES";
        }
        targetText.text = targetType + "\n";

        if (target != null)
        {
            bool jammed;
            double targetDistance = Math.Round(Vector3.Magnitude(shipController.getPosition() - target.getJamPosition(true, out jammed)));
            float targetHealth = target.getHealthPercentage();
            string targetName = target.getShipName();
            float shieldHealth = target.getShieldPercentage();
            if (shieldHealth < 0)
            {
                targetText.text = targetText.text + targetName + "\nDISTANCE: " + targetDistance + "\nHULL: " + targetHealth + "%\n\n";
            }
            else
            {
                targetText.text = targetText.text + targetName + "\nDISTANCE: " + targetDistance + "\nSHIELD: " + shieldHealth + "%\nHULL: " + targetHealth + "%\n\n";
            }
        }

        if (shipController.getCurrentWeaponSystem() == 0)
        {
            targetText.text = targetText.text + "LASER CANNONS: CONVERGENCE: " + shipController.getLaserConvergence();
        }
        else if (shipController.getCurrentWeaponSystem() == -1)
        {
            targetText.text = targetText.text + "ION CANNONS: CONVERGENCE: " + shipController.getLaserConvergence();
        }
        else if (shipController.getCurrentWeaponSystem() == 1)
        {
            targetText.text = targetText.text + "CONCUSSION MISSILES: CONVERGENCE: " + shipController.getMissileConvergence() + "\nAMMO COUNT: " + shipController.getCurrentWeaponAmmoCount();
        }
        else if (shipController.getCurrentWeaponSystem() == 2)
        {
            targetText.text = targetText.text + "PROTON TORPEDOES: CONVERGENCE: " + shipController.getMissileConvergence() + "\nAMMO COUNT: " + shipController.getCurrentWeaponAmmoCount();
        }
        else if (shipController.getCurrentWeaponSystem() == 3)
        {
            targetText.text = targetText.text + "ION MISSILES: CONVERGENCE: " + shipController.getMissileConvergence() + "\nAMMO COUNT: " + shipController.getCurrentWeaponAmmoCount();
        }
        else if (shipController.getCurrentWeaponSystem() == 4)
        {
            targetText.text = targetText.text + "ION TORPEDOES: CONVERGENCE: " + shipController.getMissileConvergence() + "\nAMMO COUNT: " + shipController.getCurrentWeaponAmmoCount();
        }
        else if (shipController.getCurrentWeaponSystem() == 5)
        {
            targetText.text = targetText.text + "PROTON BOMBS" + "\nAMMO COUNT: " + shipController.getCurrentWeaponAmmoCount();
        }
        else if (shipController.getCurrentWeaponSystem() == 6)
        {
            targetText.text = targetText.text + "ION BOMBS" + "\nAMMO COUNT: " + shipController.getCurrentWeaponAmmoCount();
        }

        speedText.text = Math.Round(shipController.activeSpeed) + speedTextMessage;
        updateThrottle();
        updateOverallPower();
        updateEnginePower();
        updateLaserPower();
        updateJammer();
        updateLaserCharge();
        updateRepulsor();
        updateShipHealth();
        updateSensorDots();
        updateShieldPower();
        updateFrontShield();
        updateFrontOverShield();
        updateRearShield();
        updateRearOverShield();

        if (shipController.getShieldFocusStatus() == -1)
        {
            frontShieldHighlight.gameObject.SetActive(false);
            rearShieldHighlight.gameObject.SetActive(true);
        }
        else if (shipController.getShieldFocusStatus() == 0)
        {
            frontShieldHighlight.gameObject.SetActive(false);
            rearShieldHighlight.gameObject.SetActive(false);
        }
        else if (shipController.getShieldFocusStatus() == 1)
        {
            frontShieldHighlight.gameObject.SetActive(true);
            rearShieldHighlight.gameObject.SetActive(false);
        }
    }
}
