using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BWingController : ShieldedPlayerController
{
    // WEAPON VALUES CONFIGURABLE
    // default fire link is autoblaster with 0.075 fire rate and heavy blaster with 0.5 fire rate occurring simultaenously
    // link 1 is adding in ion cannons firing individually at 0.1f fire rate, link 2 fires ion cannons simultaenously with 0.18 fire rate
    private float autoFireRate = 0.1f;
    private float heavyFireRate = 0.5f;
    private float singleIonFireRate = 0.1f;
    private float dualIonFireRate = 0.18f;
    public GameObject blasterBoltPrefab; // prefab used for blaster bolts
    public GameObject autoBlasterBoltPrefab;
    public GameObject ionBoltPrefab;
    public GameObject missilePrefab;
    public GameObject ionmissilePrefab;
    public GameObject iontorpPrefab;
    public GameObject protontorpPrefab;
    public GameObject protonbombPrefab;
    public GameObject ionbombPrefab;
    public int cMissileCount = 3;
    public int protonTorpCount = 3;
    public int ionMissileCount = 2;
    public int ionTorpCount = 2;
    public int protonBombCount = 5;
    public int ionBombCount = 5;

    // GYRO VALUES VARIABLE
    private float gyroStatus = 0f;
    private float gyroRollStatus = 0f;
    private float activeGyroRoll = 0f;

    // WEAPON VALUES VARIABLE
    private float fireStatus = 0f;
    private float autoFireRateTimestamp;
    private float heavyFireRateTimestamp;
    private float ionFireRateTimestamp;
    private float ionFireRate;
    private int fireLinkStatus;
    private int fireLinkRotation;
    private int weaponSelection = 0; // 0 for lasers, 1 for missiles
    private int torpFireLinkStatus;

    // TARGETING VALUES CONFIGURABLE
    private float fullTimeTillLock = 7.0f;
    private float staticTimeTillNextTargetSound = 0.25f;
    // TARGETING VALUES VARIABLE
    private bool targetLockStatus = false;
    private bool targetLockReset = true; // set to false when during lock. Set to true once lock broken.
    private float timeTillLock;
    private float timeTillNextTargetSound;
    private float targetingConvergence = 300f;
    private float missileConvergence = 1000f;
    private Vector3 targetLockGuide;

    // EFFECT VALUES CONFIGURABLE
    private Light engineLightTR;
    private Light engineLightTL;
    private Light engineLightBR;
    private Light engineLightBL;
    private ParticleSystem.MainModule particleTR;
    private ParticleSystem.MainModule particleTL;
    private ParticleSystem.MainModule particleBR;
    private ParticleSystem.MainModule particleBL;
    public AudioClip autoBlasterClip; // audio clip used for blaster sound
    public AudioClip heavyBlasterClip; // audio clip used for heavy blaster sound
    public AudioClip ionClip; // audio clip used for ion blaster sound
    public AudioClip missileClip;
    public AudioClip protonTorpClip;
    public AudioClip ionmissileClip;
    public AudioClip iontorpClip;
    public AudioClip protonbombClip;
    public AudioClip ionbombClip;
    public AudioClip targetLockClip;
    public AudioClip targetingClip;
    public AudioClip sFoilCloseClip;
    public AudioClip sFoilOpenClip;

    // S FOIL VALUES CONFIGURABLE
    private float sFoilAngle = 90f;
    private float sFoilTransitionTime = 2.5f; // Time for SFoils to transition
    // S FOIL VALUES VARIABLE
    private bool sFoilToggle; // true is open, false is closed
    private bool sFoilTransitionStatus; // true is in transition, false is not in transition
    private float sFoilActiveTime = 0f; // current time in transition

    // Start is called before the first frame update
    void Start()
    {
        ShipControllerStart();
        PlayerControllerStart();

        // Initialize Ship Specific Constants:
        rollValue = 70f * 2.0f; // determines speed of rolls, derived from DPF
        rollAcceleration = 2.39f; // acceleration of rolls, derived from G
        lookRateSpeedPitch = 70f * 1.25f; // determines the speed of pitching/yawing, derived from DPF
        lookRateSpeedYaw = 70f * 0.75f; // derived from DPF
        forwardAcceleration = 0.239f * 2.0f; // determines main engine accleration value, derived from G
        maxSpeed = 263.889f; // determines max forward speed, derived from kph
        repulsorAcceleration = 0.9f; // repulsor acceleration. Decrements by one based on power level, derived from overall ship status. Interceptors will have a max of 1.1, bombers a max of 0.9
        pitchAcceleration = .7f * 2.39f * 1.25f; // derived from DPF and G
        yawAcceleration = .7f * 2.39f * 0.75f; // derived from DPF and G
        totalShipHealth = 125f;
        totalForwardShieldHealth = 26;
        totalRearShieldHealth = 26;
        totalShieldLimit = 78;
        maxRepulsorValue = 70.0f; // max repulsor speed, derived from DPF
        sensorRangeLimit = 5000f;
        sensorBurnLimit = 500f;

        // Initialize targeting values
        timeTillLock = fullTimeTillLock;
        timeTillNextTargetSound = staticTimeTillNextTargetSound;

        // Initialize Health values
        shipHealth = totalShipHealth;
        shipIonHealth = totalShipHealth;
        forwardShieldHealth = totalForwardShieldHealth;
        rearShieldHealth = totalRearShieldHealth;
        forwardOverShieldHealth = 13f;
        rearOverShieldHealth = 13f;

        // Initialize effects
        engineLightTL = transform.Find("GyroPivot").Find("EngineLightTL").gameObject.GetComponent<Light>();
        engineLightTR = transform.Find("GyroPivot").Find("EngineLightTR").gameObject.GetComponent<Light>();
        particleTR = transform.Find("GyroPivot").Find("EngineParticlesTR").gameObject.GetComponent<ParticleSystem>().main;
        particleTL = transform.Find("GyroPivot").Find("EngineParticlesTL").gameObject.GetComponent<ParticleSystem>().main;
        engineLightBL = transform.Find("GyroPivot").Find("EngineLightBL").gameObject.GetComponent<Light>();
        engineLightBR = transform.Find("GyroPivot").Find("EngineLightBR").gameObject.GetComponent<Light>();
        particleBR = transform.Find("GyroPivot").Find("EngineParticlesBR").gameObject.GetComponent<ParticleSystem>().main;
        particleBL = transform.Find("GyroPivot").Find("EngineParticlesBL").gameObject.GetComponent<ParticleSystem>().main;

        // Initialize movement values
        throttleLevel = 0;
        throttleStatus = 0;
        repulsorStatus = 0;

        // Initialize power values
        enginePower = 4;
        laserPower = 2;
        shieldPower = 2;
        overallPower = 8;

        // Initialize weapon values
        laserChargeLimit = 100f;
        laserCharge = 100f;
        ionFireRate = singleIonFireRate;
        fireLinkStatus = 0; // 0 for normal blasters, 1 for single ion, 2 for triple fire || 0 for single ion, 1 for all firing single, 2 for triple fire
        torpFireLinkStatus = 0; // 0 for single, 1 for dual
        chargeRate3 = 0.3f;
        chargeRate4 = 0.2f;

        //Debug.Log(transform.Find("GyroPivot").Find("EngineBlock").GetComponent<Renderer>().bounds.size);

        // Initialize S Foi values
        sFoilToggle = false;
        sFoilTransitionStatus = false;
    }

    //************************************************INPUT FUNCTIONS************************************************//
    // Gyroscope input function
    void OnGyro(InputValue value)
    {
        gyroStatus = value.Get<float>();
    }


    // S Foil input function
    void OnSFoils(InputValue value)
    {
        if (!sFoilTransitionStatus)
        {
            sFoilTransitionStatus = true;
            sFoilToggle = !sFoilToggle;

            if (sFoilToggle)
            {
                shipSounds.PlayOneShot(sFoilOpenClip);
            }
            else
            {
                shipSounds.PlayOneShot(sFoilCloseClip);
            }
        }
    }

    // Input function to adjust targeting convergence
    void OnAdjustTargeting(InputValue value)
    {
        if (value.Get<float>() < 0)
        {
            changeTargetingConvergence(-1f);
        }
        else if (value.Get<float>() > 0)
        {
            changeTargetingConvergence(1f);
        }
    }

    void OnResetTargeting()
    {
        if (weaponSelection == 0 || weaponSelection == -1) // lasers or ion
        {
            transform.Find("TargetingConvergence").position = transform.position + -300f * transform.up;
            targetingConvergence = 300f;
        }
        else if (weaponSelection < 5) // missiles
        {
            transform.Find("TargetingConvergence").position = transform.position + -1000f * transform.up;
            missileConvergence = 1000f;
        }
    }
    void targetLockCancel()
    {
        // target lock reset
        targetLockStatus = false;
        timeTillLock = fullTimeTillLock;
        timeTillNextTargetSound = 0.25f;
        staticTimeTillNextTargetSound = 0.25f;
        targetLockReset = true;
        targetCircle.gameObject.SetActive(false);
        crosshair.GetComponent<Image>().color = new Color32(255, 255, 255, 50);
        if (lockedOn)
        {
            currentTarget.lockedOnWarning--;
            lockedOn = false;
        }
        if (lockingOn)
        {
            currentTarget.lockingOnWarning--;
            lockingOn = false;
        }
    }

    // Weapon selection function
    void OnSwitchWeapon()
    {
        int oldWeaponSelection = weaponSelection;

        if (weaponSelection == 0)
        {
            weaponSelection = -1;
        }
        else if (weaponSelection == -1)
        {
            weaponSelection = 1;
        }
        else if (weaponSelection == 1)
        {
            weaponSelection = 2;
        }
        else if (weaponSelection == 2)
        {
            weaponSelection = 3;
        }
        else if (weaponSelection == 3)
        {
            weaponSelection = 4;
        }
        else if (weaponSelection == 4)
        {
            weaponSelection = 5;
        }
        else if (weaponSelection == 5)
        {
            weaponSelection = 6;
        }
        else if (weaponSelection == 6)
        {
            weaponSelection = 0;
        }


        if (weaponSelection == 1 && cMissileCount == 0) // switched to concussion missiles
        {
            weaponSelection = 2;
        }
        if (weaponSelection == 2 && protonTorpCount == 0) // switched to proton torpedoes
        {
            weaponSelection = 3;
        }
        if (weaponSelection == 3 && ionMissileCount == 0) // switched to ion missile
        {
            weaponSelection = 4;
        }
        if (weaponSelection == 4 && ionTorpCount == 0) // switched to ion torp
        {
            weaponSelection = 5;
        }
        if (weaponSelection == 5 && protonBombCount == 0)
        {
            weaponSelection = 6;
        }
        if (weaponSelection == 6 && ionBombCount == 0)
        {
            weaponSelection = 0;
        }
        // If switched from lasers to munitions or vice versa cancel target lock
        if ((oldWeaponSelection <= 0 && weaponSelection > 0) || (oldWeaponSelection > 0 && weaponSelection <= 0) || weaponSelection == 5 || weaponSelection == 6)
        {
            targetLockCancel();
        }

        if (weaponSelection == 0 || weaponSelection == -1) // lasers
        {
            transform.Find("TargetingConvergence").position = transform.position + -targetingConvergence * transform.up;
        }
        else // missiles
        {
            transform.Find("TargetingConvergence").position = transform.position + -missileConvergence * transform.up;
        }
    }

    // Input function for firing weapon system
    void OnFire(InputValue value)
    {
        fireStatus = value.Get<float>();
    }

    // Cannon link function
    void OnLinkCannons(InputValue value)
    {
        if (weaponSelection == 0)
        {
            fireLinkRotation = 0;
            if (fireLinkStatus == 0)
            {
                fireLinkStatus = 1;
                ionFireRate = singleIonFireRate;
            }
            else if (fireLinkStatus == 1)
            {
                fireLinkStatus = 2;
                ionFireRate = dualIonFireRate;
            }
            else if (fireLinkStatus == 2)
            {
                fireLinkStatus = 0;
                ionFireRate = singleIonFireRate;
            }
        }
        else if (weaponSelection == -1)
        {
            fireLinkRotation = 0;
            if (fireLinkStatus == 0)
            {
                fireLinkStatus = 1;
                ionFireRate = singleIonFireRate;
            }
            else if (fireLinkStatus == 1)
            {
                fireLinkStatus = 2;
                ionFireRate = dualIonFireRate;
            }
            else if (fireLinkStatus == 2)
            {
                fireLinkStatus = 0;
                ionFireRate = singleIonFireRate;
            }
        }
        else if (weaponSelection < 5)
        {
            if (torpFireLinkStatus == 0)
            {
                torpFireLinkStatus = 1;
            }
            else
            {
                torpFireLinkStatus = 0;
            }
        }
    }

    //************************************************WEAPON SYSTEM FUNCTIONS************************************************//
    void shootRayTop()
    {
        if (laserCharge <= 0)
        {
            return;
        }
        laserCharge-=0.5f;
        Transform muzzle = transform.Find("MuzzleTop");
        GameObject laser = GameObject.Instantiate(autoBlasterBoltPrefab, muzzle.position, muzzle.rotation) as GameObject;
        if (targetLockStatus)
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (targetLockGuide - muzzle.position).normalized, 2000, 7.5f, false);
        }
        else
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (transform.Find("TargetingConvergence").position - muzzle.position).normalized, 2000, 7.5f, false);
        }
    }

    void shootRayBottom()
    {
        if (laserCharge <= 0)
        {
            return;
        }
        laserCharge-=1.5f;
        Transform muzzle = transform.Find("GyroPivot").Find("MuzzleBottom");
        GameObject laser = GameObject.Instantiate(blasterBoltPrefab, muzzle.position, muzzle.rotation) as GameObject;
        if (targetLockStatus)
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (targetLockGuide - muzzle.position).normalized, 2000, 37f, false);
        }
        else
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (transform.Find("TargetingConvergence").position - muzzle.position).normalized, 2000, 37f, false);
        }
    }

    void shootIonRayL()
    {
        if (laserCharge <= 0)
        {
            return;
        }
        laserCharge--;
        Transform muzzleLeft = transform.Find("GyroPivot").Find("LeftWing").Find("IonMuzzleLeft");
        GameObject laser = GameObject.Instantiate(ionBoltPrefab, muzzleLeft.position, muzzleLeft.rotation) as GameObject;
        if (targetLockStatus)
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (targetLockGuide - muzzleLeft.position).normalized, 2000, 75f, true);
        }
        else
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (transform.Find("TargetingConvergence").position - muzzleLeft.position).normalized, 2000, 75f, true);
        }
    }

    void shootIonRayR()
    {
        if (laserCharge <= 0)
        {
            return;
        }
        laserCharge--;
        Transform muzzleRight = transform.Find("GyroPivot").Find("RightWing").Find("IonMuzzleRight");
        GameObject laser = GameObject.Instantiate(ionBoltPrefab, muzzleRight.position, muzzleRight.rotation) as GameObject;
        if (targetLockStatus)
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (targetLockGuide - muzzleRight.position).normalized, 2000, 75f, true);
        }
        else
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (transform.Find("TargetingConvergence").position - muzzleRight.position).normalized, 2000, 75f, true);
        }
    }
    void shootIonRayBottom()
    {
        if (laserCharge <= 0)
        {
            return;
        }
        laserCharge--;
        Transform muzzle = transform.Find("GyroPivot").Find("IonMuzzleBottom");
        GameObject laser = GameObject.Instantiate(ionBoltPrefab, muzzle.position, muzzle.rotation) as GameObject;
        if (targetLockStatus)
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (targetLockGuide - muzzle.position).normalized, 2000, 75f, true);
        }
        else
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (transform.Find("TargetingConvergence").position - muzzle.position).normalized, 2000, 75f, true);
        }
    }

    void shootTorpL()
    {
        Transform torpMuzzleL = transform.Find("GyroPivot").Find("TorpMuzzleL");
        GameObject currentPrefab = missilePrefab;
        if (weaponSelection == 2)
        {
            currentPrefab = protontorpPrefab;
        }
        else if (weaponSelection == 3)
        {
            currentPrefab = ionmissilePrefab;
        }
        else if (weaponSelection == 4)
        {
            currentPrefab = iontorpPrefab;
        }
        GameObject missile = GameObject.Instantiate(currentPrefab, torpMuzzleL.position, torpMuzzleL.rotation) as GameObject;
        if (currentTarget == null || !targetLockStatus)
        {
            missile.GetComponent<MissileBehavior>().dumbFire(totalVelocityVector, (transform.Find("TargetingConvergence").position - torpMuzzleL.position).normalized, weaponSelection);
        }
        else
        {
            missile.GetComponent<MissileBehavior>().setTarget(totalVelocityVector, (transform.Find("TargetingConvergence").position - torpMuzzleL.position).normalized, currentTarget, weaponSelection, true);
        }

    }

    void shootTorpR()
    {
        Transform torpMuzzleR = transform.Find("GyroPivot").Find("TorpMuzzleR");
        GameObject currentPrefab = missilePrefab;
        if (weaponSelection == 2)
        {
            currentPrefab = protontorpPrefab;
        }
        else if (weaponSelection == 3)
        {
            currentPrefab = ionmissilePrefab;
        }
        else if (weaponSelection == 4)
        {
            currentPrefab = iontorpPrefab;
        }
        GameObject missile = GameObject.Instantiate(currentPrefab, torpMuzzleR.position, torpMuzzleR.rotation) as GameObject;
        if (currentTarget == null || !targetLockStatus)
        {
            missile.GetComponent<MissileBehavior>().dumbFire(totalVelocityVector, (transform.Find("TargetingConvergence").position - torpMuzzleR.position).normalized, weaponSelection);
        }
        else
        {
            missile.GetComponent<MissileBehavior>().setTarget(totalVelocityVector, (transform.Find("TargetingConvergence").position - torpMuzzleR.position).normalized, currentTarget, weaponSelection, true);
        }
    }
    void shootBomb()
    {
        Transform bombMuzzle = transform.Find("GyroPivot").Find("BombMuzzle");

        GameObject bombPrefab = protonbombPrefab;
        if (weaponSelection == 6)
        {
            bombPrefab = ionbombPrefab;
        }
       
        GameObject bomb = GameObject.Instantiate(bombPrefab, bombMuzzle.position, bombMuzzle.rotation) as GameObject;
        bomb.GetComponent<MissileBehavior>().dumbFire(totalVelocityVector, bombMuzzle.forward, weaponSelection);
    }

    void fireCannonsUpdate()
    {
        if (fireStatus == 1f && (weaponSelection == 0 || weaponSelection == -1))
        {
            if (laserCharge > 0)
            {

                if (weaponSelection == 0 || fireLinkStatus > 0) // just auto and heavy blaster
                {
                    if (Time.time > autoFireRateTimestamp)
                    {
                        shipSounds.PlayOneShot(autoBlasterClip);
                        shootRayTop();
                        autoFireRateTimestamp = Time.time + autoFireRate;
                    }
                    if (Time.time > heavyFireRateTimestamp)
                    {
                        shipSounds.PlayOneShot(heavyBlasterClip);
                        shootRayBottom();
                        heavyFireRateTimestamp = Time.time + heavyFireRate;
                    } 
                    
                }
                if (((weaponSelection == -1 && fireLinkStatus < 2) || (weaponSelection == 0 && fireLinkStatus == 1)) && Time.time > ionFireRateTimestamp) // single ion fire
                {
                    if (fireLinkRotation == 0)
                    {
                        shipSounds.PlayOneShot(ionClip);
                        shootIonRayL();
                        fireLinkRotation++;
                    }
                    else if (fireLinkRotation == 1)
                    {
                        shipSounds.PlayOneShot(ionClip);
                        shootIonRayR();
                        fireLinkRotation++;
                    }
                    else
                    {
                        shipSounds.PlayOneShot(ionClip);
                        shootIonRayBottom();
                        fireLinkRotation = 0;
                    }
                    ionFireRateTimestamp = Time.time + ionFireRate;
                }
                if (fireLinkStatus == 2 && Time.time > ionFireRateTimestamp)
                {
                    shipSounds.PlayOneShot(ionClip);
                    shootIonRayR();
                    shootIonRayL();
                    shootIonRayBottom();
                    ionFireRateTimestamp = Time.time + ionFireRate;
                }
                    
            }

        }
        else if (fireStatus == 1f && weaponSelection > 0 && weaponSelection < 5)
        {
            if ((getCurrentWeaponAmmoCount() > 0 && torpFireLinkStatus == 0) || getCurrentWeaponAmmoCount() == 1)
            {
                if (weaponSelection == 1)
                {
                    shipSounds.PlayOneShot(missileClip);
                    cMissileCount--;
                }
                else if (weaponSelection == 2)
                {
                    shipSounds.PlayOneShot(protonTorpClip);
                    protonTorpCount--;
                }
                else if (weaponSelection == 3)
                {
                    shipSounds.PlayOneShot(ionmissileClip);
                    ionMissileCount--;
                }
                else if (weaponSelection == 4)
                {
                    shipSounds.PlayOneShot(iontorpClip);
                    ionTorpCount--;
                }
                shootTorpL();
            }
            else if (getCurrentWeaponAmmoCount() > 1 && torpFireLinkStatus == 1)
            {
                if (weaponSelection == 1)
                {
                    shipSounds.PlayOneShot(missileClip);
                    cMissileCount -= 2;
                }
                else if (weaponSelection == 2)
                {
                    shipSounds.PlayOneShot(protonTorpClip);
                    protonTorpCount -= 2;
                }
                else if (weaponSelection == 3)
                {
                    shipSounds.PlayOneShot(ionmissileClip);
                    ionMissileCount -= 2;
                }
                else if (weaponSelection == 4)
                {
                    shipSounds.PlayOneShot(iontorpClip);
                    ionTorpCount -= 2;
                }
                shootTorpL();
                shootTorpR();
            }
            fireStatus = 0f;
        }
        else if (fireStatus == 1f && weaponSelection >= 5)
        {
            if (getCurrentWeaponAmmoCount() > 0)
            {
                if (weaponSelection == 5)
                {
                    shipSounds.PlayOneShot(protonbombClip);
                    protonBombCount--;
                }
                else if (weaponSelection == 6)
                {
                    shipSounds.PlayOneShot(ionbombClip);
                    ionBombCount--;
                }
                shootBomb();
            }
            fireStatus = 0f;
        }
    }

    //************************************************TARGETING SYSTEM FUNCTIONS************************************************//
    // Change targeting convergence distance
    void changeTargetingConvergence(float changeValue)
    {

        if (weaponSelection == 0 || weaponSelection == -1) // lasers
        {
            if (targetingConvergence <= 100f && changeValue < 0f)
            {
                targetingConvergence = 100f;
                return;
            }
            else if (targetingConvergence >= 1000f && changeValue > 0f)
            {
                targetingConvergence = 1000f;
                return;
            }
            transform.Find("TargetingConvergence").position = transform.Find("TargetingConvergence").position - 50f * changeValue * transform.up;
            targetingConvergence = targetingConvergence + 50f * changeValue;
        }
        else if (weaponSelection < 5) // missiles
        {
            if (missileConvergence <= 500f && changeValue < 0f)
            {
                missileConvergence = 500f;
                return;
            }
            else if (missileConvergence >= 5000f && changeValue > 0f)
            {
                missileConvergence = 5000f;
                return;
            }
            transform.Find("TargetingConvergence").position = transform.Find("TargetingConvergence").position - 100f * changeValue * transform.up;
            missileConvergence = missileConvergence + 100f * changeValue;
        }
    }
    bool targetLockBreak(float targetAngle, float targetDistance)
    {
        if (targetAngle > 15f || !targetingComputerToggle)
        {
            return true;
        }

        if (weaponSelection <= 0)
        {
            if (targetDistance > 1000f || Mathf.Abs(targetingConvergence - targetDistance) > 300f)
            {
                return true;
            }
        }
        else
        {
            if (targetDistance > 5000f || targetDistance < 300f || Mathf.Abs(missileConvergence - targetDistance) > 500f)
            {
                return true;
            }
        }
        return false;
    }

    // Target Lock function
    void targetLock(bool onTarget)
    {
        if (weaponSelection >= 5)
        {
            return;
        }
        if (!onTarget || jammed)
        {
            targetCircle.gameObject.SetActive(false);
            return;
        }
        Vector3 shipToTarget = currentTarget.getJamPosition(true, out jammed) - transform.position;
        Vector3 shipForwardVector = -transform.up;
        float targetAngle = Vector3.Angle(shipForwardVector, shipToTarget);
        float targetDistance = Vector3.Magnitude(shipToTarget);

        if (targetLockBreak(targetAngle, targetDistance) || jammed)
        {
            targetLockCancel();
        }
        else
        {
            if (!targetLockStatus)
            {
                if (targetLockReset)
                {
                    targetCircle.gameObject.SetActive(true);
                    targetLockReset = false;
                }
                if (!lockingOn)
                {
                    lockingOn = true;
                    currentTarget.lockingOnWarning++;
                }

                float targetingConvergenceDifference = 10f;
                float targetingRateFactor = 1f;
                if (weaponSelection == 0 || weaponSelection == -1) // lasers
                {
                    targetingConvergenceDifference += Mathf.Abs(targetDistance - targetingConvergence);
                    // The closer the targeting convergence to actual distance and the closer to the center of sight, the faster the lock
                    targetingRateFactor = targetingConvergenceDifference * targetAngle / 300f;
                }
                else if (weaponSelection < 5) // missiles
                {
                    targetingConvergenceDifference += Mathf.Abs(targetDistance - missileConvergence);
                    // The closer the targeting convergence to actual distance and the closer to the center of sight, the faster the lock
                    targetingRateFactor = targetingConvergenceDifference * targetAngle / 200f;
                }

                timeTillLock = timeTillLock - Time.fixedDeltaTime / targetingRateFactor;
                if (timeTillLock <= 0f)
                {
                    targetLockStatus = true;
                    if (!lockedOn)
                    {
                        lockedOn = true;
                        currentTarget.lockedOnWarning++;
                    }
                    //targetGuide.gameObject.SetActive(true);
                    shipSounds.PlayOneShot(targetLockClip);
                    targetCircle.gameObject.SetActive(false);
                    crosshair.GetComponent<Image>().color = new Color32(255, 0, 0, 100);

                    return;
                }


                float targetCircleScale = Mathf.Lerp(7f, 5f, (fullTimeTillLock - timeTillLock) / fullTimeTillLock);
                targetCircle.localScale = new Vector3(targetCircleScale, targetCircleScale, 1);

                float targetSoundRateFactor = Mathf.Log(1 / (targetingConvergenceDifference * targetAngle / 100f), 2f);
                if (targetSoundRateFactor > 1)
                {

                    timeTillNextTargetSound = timeTillNextTargetSound - Time.fixedDeltaTime * targetSoundRateFactor; // targeting beep increases if targeting rate is high
                }
                else
                {
                    timeTillNextTargetSound = timeTillNextTargetSound - Time.fixedDeltaTime;
                }

                if (timeTillNextTargetSound <= 0f)
                {
                    shipSounds.PlayOneShot(targetingClip);
                    timeTillNextTargetSound = staticTimeTillNextTargetSound;
                }

            }
        }
    }

    // Calculate target guide position
    void calcTargetGuide(bool onTarget)
    {
        /*Vector3 cameraPosition = GameObject.Find("Main Camera").transform.position;

        if (!onTarget || !targetLockStatus)
        {
            targetGuide.position = cameraPosition;
            targetGuide.gameObject.SetActive(false);
            crosshair.GetComponent<Image>().color = new Color32(255, 255, 255, 50);
            return;
        }
        Vector3 targetLocation = currentTarget.getPosition();
        Vector3 targetVelocity = currentTarget.getVelocity();
        Vector3 laserVelocity = totalVelocityVector - 750f * transform.up;
        Vector3 targetPlaneVelocity = Vector3.ProjectOnPlane(targetVelocity, transform.up);

        Vector3 shipLocation = transform.position;
        Vector3 shipToTargetLocation = targetLocation - shipLocation;
        float k = shipToTargetLocation.magnitude;

        float d_t = 1.1f * Mathf.Sqrt(k * k / (Mathf.Pow(laserVelocity.magnitude, 2f) / Mathf.Pow(targetPlaneVelocity.magnitude, 2f) - 1));
        Vector3 targetGuideRealPosition = targetLocation + targetPlaneVelocity.normalized * d_t;
        //targetGuide.position = targetGuideRealPosition;
        targetGuide.position = (targetGuideRealPosition - cameraPosition).normalized * 300f + cameraPosition;*/

        /*float shipToTargetVelocityAngle = Vector3.Angle(shipToTargetLocation, shipVelocity);
        Vector3.ProjectOnPlane(targetVelocity, transform.up);
        float v_t = targetVelocity.magnitude;
        float v_s = shipVelocity.magnitude;
        float a = 1 + Mathf.Pow(v_t, 2f) / Mathf.Pow(v_s, 2f);
        float b = 2 * k * v_t * Mathf.Cos(shipToTargetVelocityAngle) / v_s;
        float c = -Mathf.Pow(k, 2);
        float x_s = (-b + Mathf.Sqrt(Mathf.Pow(b, 2f) - 4 * a * c)) / (2 * a);
        float x_t = x_s * v_t / v_s;
        targetGuide.position = targetLocation + targetVelocity.normalized * x_t;
        //targetGuide.position = targetLocation;*/
        if (!onTarget || !targetLockStatus)
        {
            targetGuide.gameObject.SetActive(false);
            targetLockStatus = false;
            crosshair.GetComponent<Image>().color = new Color32(255, 255, 255, 50);
            return;
        }
        Vector3 targetLocation = currentTarget.getPosition();
        Vector3 targetVelocity = currentTarget.getVelocity();
        Vector3 laserVelocity = totalVelocityVector - 750f * transform.up;

        float time_laser_travel = Vector3.Distance(targetLocation, transform.position) / laserVelocity.magnitude;
        Vector3 targetFutureLocation = targetLocation + targetVelocity * time_laser_travel;
        //targetLockGuide = transform.TransformPoint(transform.InverseTransformPoint(targetFutureLocation).normalized * targetingConvergence);
        targetLockGuide = targetFutureLocation;
    }

    //************************************************MISC FUNCTIONS************************************************//
    // S Foil Transition function
    void sFoilTransition()
    {
        if (sFoilTransitionStatus)
        {
            sFoilActiveTime += Time.fixedDeltaTime;

            float sFoilAngleSet;
            if (sFoilActiveTime >= sFoilTransitionTime)
            {
                if (sFoilToggle)
                {
                    sFoilAngleSet = 0;
                }
                else
                {
                    sFoilAngleSet = sFoilAngle;
                }
                sFoilActiveTime = 0f;
                sFoilTransitionStatus = false;

            }
            else
            {
                if (sFoilToggle)
                {
                    sFoilAngleSet = Mathf.Lerp(sFoilAngle, 0, sFoilActiveTime / sFoilTransitionTime);
                }
                else
                {
                    sFoilAngleSet = Mathf.Lerp(0, sFoilAngle, sFoilActiveTime / sFoilTransitionTime);
                }

            }
            //transform.Find("GyroPivot").Find("RightWing").RotateAroundLocal();
            transform.Find("GyroPivot").Find("RightWing").localRotation = Quaternion.Euler(0, sFoilAngleSet, 0);
            transform.Find("GyroPivot").Find("LeftWing").localRotation = Quaternion.Euler(0, -sFoilAngleSet, 0);
        }
    }
    
    void engineLightIntensity()
    {
        if (throttleLevel < 100)
        {
            particleTR.startColor = new Color(0, 0, 0, 0);
            particleTL.startColor = new Color(0, 0, 0, 0);
            particleBR.startColor = new Color(0, 0, 0, 0);
            particleBL.startColor = new Color(0, 0, 0, 0);
        }
        else
        {
            Color particleColor = new Color(238, 156, 84, Mathf.Lerp(0, 255, enginePower / (float)enginePowerLimit));
            particleTL.startColor = particleColor;
            particleTR.startColor = particleColor;
            particleBL.startColor = particleColor;
            particleBR.startColor = particleColor;
        }

        //Update Engine Light Intensity
        float engineLightIntensity = Mathf.Lerp(0.0f, 1.0f, realSpeedGoal / maxSpeed);
        engineLightTL.intensity = engineLightIntensity;
        engineLightTR.intensity = engineLightIntensity;
        engineLightBL.intensity = engineLightIntensity;
        engineLightBR.intensity = engineLightIntensity;
    }

    void gyroUpdate()
    {
        if (gyroStatus == 1f)
        {
            rollStatus = 0f;
            activeGyroRoll = Mathf.Lerp(activeGyroRoll, rollValue * gyroRollStatus, rollAcceleration * Time.fixedDeltaTime);
            transform.Find("GyroPivot").localRotation = transform.Find("GyroPivot").localRotation * Quaternion.Euler(0, activeGyroRoll * Time.fixedDeltaTime, 0);
        }
        else
        {
            activeGyroRoll = 0f;
        }
    }

    protected override void OnRoll(InputValue value)
    {
        if (ionStatus)
        {
            return;
        }
        gyroRollStatus = value.Get<float>();
        if (gyroStatus != 1f)
        {
            rollStatus = value.Get<float>();
        }
        else
        {
            rollStatus = 0f;
        }
    }

    //************************************************OVERRIDE FUNCTIONS************************************************//
    // Override function to update laser charge, taking into account S-Foils
    protected override void updateLaserCharge()
    {
        if (laserPower == 0) // loses two laser charge every second
        {
            if (laserCharge == 0)
            {
                return;
            }
            if (Time.time > laserChargeTimestamp)
            {
                laserCharge--;
                laserChargeTimestamp = Time.time + 0.5f;
            }
        }
        else if (laserPower == 1) // loses a laser charge every second
        {
            if (laserCharge == 0)
            {
                return;
            }
            if (Time.time > laserChargeTimestamp)
            {
                laserCharge--;
                laserChargeTimestamp = Time.time + 1f;
            }
        }
        else if (laserPower == 2) // static power generation
        {
            return;
        }
        else if (laserPower == 3) // gains a lser charge every second
        {
            if (laserCharge == laserChargeLimit || !sFoilToggle)
            {
                return;
            }
            if (Time.time > laserChargeTimestamp)
            {
                laserCharge++;
                laserChargeTimestamp = Time.time + 1f;
            }
        }
        else
        {
            if (laserCharge == laserChargeLimit || !sFoilToggle) // gains two laser charge every second
            {
                return;
            }
            if (Time.time > laserChargeTimestamp)
            {
                laserCharge++;
                laserChargeTimestamp = Time.time + 0.5f;
            }
        }
    }
    
    // Override function to get laser convergence distance
    public override float getLaserConvergence()
    {
        return targetingConvergence;
    }

    // Override function to get missile convergence distance
    public override float getMissileConvergence()
    {
        return missileConvergence;
    }

    // Override function to retrieve current weapon selection
    public override int getCurrentWeaponSystem()
    {
        return weaponSelection;
    }
    public override int getCurrentWeaponAmmoCount()
    {
        if (weaponSelection == 0)
        {
            return 0;
        }
        else if (weaponSelection == 1)
        {
            return cMissileCount;
        }
        else if (weaponSelection == 2)
        {
            return protonTorpCount;
        }
        else if (weaponSelection == 3)
        {
            return ionMissileCount;
        }
        else if (weaponSelection == 4)
        {
            return ionTorpCount;
        }
        else if (weaponSelection == 5)
        {
            return protonBombCount;
        }
        else if (weaponSelection == 6)
        {
            return ionBombCount;
        }
        else
        {
            return 0;
        }
    }

    void Update()
    {
        fireCannonsUpdate();
        PlayerControllerUpdate();
    }

    void FixedUpdate()
    {
        gyroUpdate();
        sFoilTransition();
        engineLightIntensity();
        calcTargetGuide(currentTarget != null);
        targetLock(currentTarget != null);
        PlayerControllerFixedUpdate();
        ShieldedPlayerFixedUpdate();
    }
}
