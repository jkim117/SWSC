using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubComponentController : ShipController
{
    // SHIP HEALTH VALUES CONFIGURABLE
    public float totalShipHealth;
    // SHIP HEALTH VALUES VARIABLE
    public float shipHealth;
    public float totalShipShield;
    public float shipShield;

    // EFFECTS VALUES CONFIGURABLE
    public GameObject explosionPrefab;
    public AudioClip explosionClip;
    public AudioSource shipSounds; // audio source used for ship sounds

    private Transform targetMarker;
    public string shipName;
    public ShipController parent;
    private bool destroyed;

    // Start is called before the first frame update
    void Start()
    {
        targetMarker = transform.Find("Canvas");
        targetMarker.gameObject.SetActive(false);
        sensorBurnLimit = 0;
        sensorRangeLimit = 0;
        offOpposingList = false;
        explosionPrefab.SetActive(false);
        destroyed = false;
    }

    // Update is called once per frame
    void Update()
    {
        StaticControllerUpdate();
    }

    public void startFire()
    {
        if (!destroyed)
        {
            explosionPrefab.SetActive(true);
            destroyed = true;
        }
        
    }

    protected void StaticControllerUpdate()
    {
        Transform aiCanvas = transform.Find("Canvas");
        aiCanvas.rotation = Camera.main.transform.rotation;

        float cameraDist = Vector3.Magnitude((aiCanvas.position - Camera.main.transform.position));
        aiCanvas.Find("TargetFrame").localScale = new Vector3(cameraDist, cameraDist, cameraDist) / 300;
    }

    public override void toggleTargetMarker(bool setActive)
    {
        targetMarker.gameObject.SetActive(setActive);
    }

    public override float getHealthPercentage()
    {
        return (float)Mathf.Round((float)shipHealth / (float)totalShipHealth * 100f);
    }
    public override float getShieldPercentage()
    {
        return (float)Mathf.Round((float)(shipShield) / (float)(totalShipShield) * 100f);
    }

    public override string getShipName()
    {
        return shipName;
    }
    
}
