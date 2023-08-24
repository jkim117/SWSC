using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TieDController : ShieldedPlayerController
{
    // WEAPON VALUES CONFIGURABLE
    private float singleFireRate = 0.1f; // single weapons fire rate
    private float dualFireRate = 0.19f; // dual weapons fire rate
    private float hexFireRate = 0.36f; // quad fire rate
    public GameObject blasterBoltPrefab; // prefab used for blaster bolts
    public GameObject missilePrefab;
    public GameObject ionmissilePrefab;
    public GameObject iontorpPrefab;
    public GameObject protontorpPrefab;
    public int cMissileCount = 3;
    public int protonTorpCount = 3;
    public int ionMissileCount = 0;
    public int ionTorpCount = 0;
    public int launch1Status = 1;
    public int launch2Status = 1;
    public int launch3Status = 1;
    public int launch4Status = 2;
    public int launch5Status = 2;
    public int launch6Status = 2;

    // WEAPON VALUES VARIABLE
    private float fireStatus = 0f;
    private float fireRateTimestamp;
    private float fireRate;
    private int fireLinkStatus;
    private int fireLinkRotation;
    private int weaponSelection = 0; // 0 for lasers, 1 for missiles
    private int torpFireLinkStatus;
    private int torpLinkRotation;

    // TARGETING VALUES CONFIGURABLE
    private float fullTimeTillLock = 5.0f;
    private float staticTimeTillNextTargetSound = 0.08f;
    // TARGETING VALUES VARIABLE
    private bool targetLockStatus = false;
    private bool targetLockReset = true; // set to false when during lock. Set to true once lock broken.
    private float timeTillLock;
    private float timeTillNextTargetSound;
    private float targetingConvergence = 300f;
    private float missileConvergence = 1000f;
    private Vector3 targetLockGuide;

    // EFFECT VALUES CONFIGURABLE
    private Light engineLightR;
    private Light engineLightL;
    private ParticleSystem.MainModule particleR;
    private ParticleSystem.MainModule particleL;
    public AudioClip blasterClip; // audio clip used for blaster sound
    public AudioClip missileClip;
    public AudioClip protonTorpClip;
    public AudioClip ionmissileClip;
    public AudioClip iontorpClip;
    public AudioClip targetLockClip;
    public AudioClip targetingClip;

    // Start is called before the first frame update
    void Start()
    {
        ShipControllerStart();
        PlayerControllerStart();

        // Initialize Ship Specific Constants:
        rollValue = 100f * 2.0f; // determines speed of rolls, derived from DPF
        rollAcceleration = 4.3f; // acceleration of rolls, derived from G
        lookRateSpeedPitch = 100f * 1.25f; // determines the speed of pitching/yawing, derived from DPF
        lookRateSpeedYaw = 100f * 0.75f; // derived from DPF
        forwardAcceleration = 0.43f * 2.0f; // determines main engine accleration value, derived from G
        maxSpeed = 500f; // determines max forward speed, derived from kph
        repulsorAcceleration = 1.1f; // repulsor acceleration. Decrements by one based on power level, derived from overall ship status. Interceptors will have a max of 1.1, bombers a max of 0.9
        maxRepulsorValue = 100.0f; // max repulsor speed, derived from DPF
        pitchAcceleration = 4.3f * 1.25f; // derived from DPF and G
        yawAcceleration = 4.3f * 0.75f; // derived from DPF and G
        totalShipHealth = 50f;
        totalForwardShieldHealth = 40;
        totalRearShieldHealth = 40;
        totalShieldLimit = 120;
        sensorRangeLimit = 5000f;
        sensorBurnLimit = 500f;

        // Initialize targeting values
        timeTillLock = fullTimeTillLock;
        timeTillNextTargetSound = staticTimeTillNextTargetSound;

        // Initialize Health values
        shipHealth = 50f;
        shipIonHealth = totalShipHealth;
        forwardShieldHealth = 40f;
        rearShieldHealth = 40f;
        forwardOverShieldHealth = 20f;
        rearOverShieldHealth = 20f;

        shieldTransfer0 = 0.05f;
        shieldTransfer1 = 0.075f;
        shieldTransfer2 = 0.1f;
        shieldTransfer3 = 0.125f;
        shieldTransfer4 = 0.15f;
        shieldChange0 = -0.025f;
        shieldChange1 = -0.0125f;
        shieldChange3 = 0.125f;
        shieldChange4 = 0.25f;
        shieldRegenDelayFull = 3f;

        // Initialize effects
        engineLightL = transform.Find("EngineLightL").gameObject.GetComponent<Light>();
        engineLightR = transform.Find("EngineLightR").gameObject.GetComponent<Light>();
        particleR = transform.Find("EngineParticlesR").gameObject.GetComponent<ParticleSystem>().main;
        particleL = transform.Find("EngineParticlesL").gameObject.GetComponent<ParticleSystem>().main;

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
        fireRate = singleFireRate;
        fireLinkStatus = 0; // 0 for single fire, 1 for dual fire, 2 for hex fire
        torpFireLinkStatus = 0; // 0 for single, 1 for dual, 2 for hex fire

        //Debug.Log(GetComponent<Renderer>().bounds.size);
    }

    //************************************************INPUT FUNCTIONS************************************************//
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
        if (weaponSelection == 0) // lasers
        {
            transform.Find("TargetingConvergence").position = transform.position + -300f * transform.up;
            targetingConvergence = 300f;
        }
        else // missiles
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
        timeTillNextTargetSound = staticTimeTillNextTargetSound;
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

        weaponSelection = (weaponSelection + 1) % 5;


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
            weaponSelection = 0;
        }

        // If switched from lasers to munitions or vice versa cancel target lock
        if ((oldWeaponSelection == 0 && weaponSelection != 0) || (oldWeaponSelection != 0 && weaponSelection == 0))
        {
            targetLockCancel();
        }

        if (weaponSelection == 0) // lasers
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
                fireRate = dualFireRate;
            }
            else if (fireLinkStatus == 1)
            {
                fireLinkStatus = 2;
                fireRate = hexFireRate;
            }
            else
            {
                fireLinkStatus = 0;
                fireRate = singleFireRate;
            }
        }
        else
        {
            torpLinkRotation = 0;
            if (torpFireLinkStatus == 0)
            {
                torpFireLinkStatus = 1;
            }
            else if (torpFireLinkStatus == 1)
            {
                torpFireLinkStatus = 2;
            }
            else
            {
                torpFireLinkStatus = 0;
            }
        }

    }

    //************************************************WEAPON SYSTEM FUNCTIONS************************************************//
    void shootRay1()
    {
        if (laserCharge <= 0)
        {
            return;
        }
        laserCharge--;
        Transform muzzle1 = transform.Find("Muzzle1");
        GameObject laser = GameObject.Instantiate(blasterBoltPrefab, muzzle1.position, muzzle1.rotation) as GameObject;
        if (targetLockStatus)
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (targetLockGuide - muzzle1.position).normalized, 2000, 10, false);
        }
        else
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (transform.Find("TargetingConvergence").position - muzzle1.position).normalized, 2000, 10, false);
        }
    }
    void shootRay2()
    {
        if (laserCharge <= 0)
        {
            return;
        }
        laserCharge--;
        Transform muzzle2 = transform.Find("Muzzle2");
        GameObject laser = GameObject.Instantiate(blasterBoltPrefab, muzzle2.position, muzzle2.rotation) as GameObject;
        if (targetLockStatus)
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (targetLockGuide - muzzle2.position).normalized, 2000, 10, false);
        }
        else
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (transform.Find("TargetingConvergence").position - muzzle2.position).normalized, 2000, 10, false);
        }
    }
    void shootRay3()
    {
        if (laserCharge <= 0)
        {
            return;
        }
        laserCharge--;
        Transform muzzle3 = transform.Find("Muzzle3");
        GameObject laser = GameObject.Instantiate(blasterBoltPrefab, muzzle3.position, muzzle3.rotation) as GameObject;
        if (targetLockStatus)
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (targetLockGuide - muzzle3.position).normalized, 2000, 10, false);
        }
        else
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (transform.Find("TargetingConvergence").position - muzzle3.position).normalized, 2000, 10, false);
        }
    }
    void shootRay4()
    {
        if (laserCharge <= 0)
        {
            return;
        }
        laserCharge--;
        Transform muzzle4 = transform.Find("Muzzle4");
        GameObject laser = GameObject.Instantiate(blasterBoltPrefab, muzzle4.position, muzzle4.rotation) as GameObject;
        if (targetLockStatus)
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (targetLockGuide - muzzle4.position).normalized, 2000, 10, false);
        }
        else
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (transform.Find("TargetingConvergence").position - muzzle4.position).normalized, 2000, 10, false);
        }
    }
    void shootRay5()
    {
        if (laserCharge <= 0)
        {
            return;
        }
        laserCharge--;
        Transform muzzle5 = transform.Find("Muzzle5");
        GameObject laser = GameObject.Instantiate(blasterBoltPrefab, muzzle5.position, muzzle5.rotation) as GameObject;
        if (targetLockStatus)
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (targetLockGuide - muzzle5.position).normalized, 2000, 10, false);
        }
        else
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (transform.Find("TargetingConvergence").position - muzzle5.position).normalized, 2000, 10, false);
        }
    }
    void shootRay6()
    {
        if (laserCharge <= 0)
        {
            return;
        }
        laserCharge--;
        Transform muzzle6 = transform.Find("Muzzle6");
        GameObject laser = GameObject.Instantiate(blasterBoltPrefab, muzzle6.position, muzzle6.rotation) as GameObject;
        if (targetLockStatus)
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (targetLockGuide - muzzle6.position).normalized, 2000, 10, false);
        }
        else
        {
            laser.GetComponent<ShotBehavior>().setVelocity(totalVelocityVector, (transform.Find("TargetingConvergence").position - muzzle6.position).normalized, 2000, 10, false);
        }
    }

    void shootTorp1()
    {
        Transform torpMuzzle = transform.Find("TorpMuzzle1");
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
        GameObject missile = GameObject.Instantiate(currentPrefab, torpMuzzle.position, torpMuzzle.rotation) as GameObject;
        if (currentTarget == null || !targetLockStatus)
        {
            missile.GetComponent<MissileBehavior>().dumbFire(totalVelocityVector, (transform.Find("TargetingConvergence").position - torpMuzzle.position).normalized, weaponSelection);
        }
        else
        {
            missile.GetComponent<MissileBehavior>().setTarget(totalVelocityVector, (transform.Find("TargetingConvergence").position - torpMuzzle.position).normalized, currentTarget, weaponSelection, true);
        }

    }
    void shootTorp2()
    {
        Transform torpMuzzle = transform.Find("TorpMuzzle2");
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
        GameObject missile = GameObject.Instantiate(currentPrefab, torpMuzzle.position, torpMuzzle.rotation) as GameObject;
        if (currentTarget == null || !targetLockStatus)
        {
            missile.GetComponent<MissileBehavior>().dumbFire(totalVelocityVector, (transform.Find("TargetingConvergence").position - torpMuzzle.position).normalized, weaponSelection);
        }
        else
        {
            missile.GetComponent<MissileBehavior>().setTarget(totalVelocityVector, (transform.Find("TargetingConvergence").position - torpMuzzle.position).normalized, currentTarget, weaponSelection, true);
        }

    }
    void shootTorp3()
    {
        Transform torpMuzzle = transform.Find("TorpMuzzle3");
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
        GameObject missile = GameObject.Instantiate(currentPrefab, torpMuzzle.position, torpMuzzle.rotation) as GameObject;
        if (currentTarget == null || !targetLockStatus)
        {
            missile.GetComponent<MissileBehavior>().dumbFire(totalVelocityVector, (transform.Find("TargetingConvergence").position - torpMuzzle.position).normalized, weaponSelection);
        }
        else
        {
            missile.GetComponent<MissileBehavior>().setTarget(totalVelocityVector, (transform.Find("TargetingConvergence").position - torpMuzzle.position).normalized, currentTarget, weaponSelection, true);
        }

    }
    void shootTorp4()
    {
        Transform torpMuzzle = transform.Find("TorpMuzzle4");
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
        GameObject missile = GameObject.Instantiate(currentPrefab, torpMuzzle.position, torpMuzzle.rotation) as GameObject;
        if (currentTarget == null || !targetLockStatus)
        {
            missile.GetComponent<MissileBehavior>().dumbFire(totalVelocityVector, (transform.Find("TargetingConvergence").position - torpMuzzle.position).normalized, weaponSelection);
        }
        else
        {
            missile.GetComponent<MissileBehavior>().setTarget(totalVelocityVector, (transform.Find("TargetingConvergence").position - torpMuzzle.position).normalized, currentTarget, weaponSelection, true);
        }

    }
    void shootTorp5()
    {
        Transform torpMuzzle = transform.Find("TorpMuzzle5");
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
        GameObject missile = GameObject.Instantiate(currentPrefab, torpMuzzle.position, torpMuzzle.rotation) as GameObject;
        if (currentTarget == null || !targetLockStatus)
        {
            missile.GetComponent<MissileBehavior>().dumbFire(totalVelocityVector, (transform.Find("TargetingConvergence").position - torpMuzzle.position).normalized, weaponSelection);
        }
        else
        {
            missile.GetComponent<MissileBehavior>().setTarget(totalVelocityVector, (transform.Find("TargetingConvergence").position - torpMuzzle.position).normalized, currentTarget, weaponSelection, true);
        }

    }
    void shootTorp6()
    {
        Transform torpMuzzle = transform.Find("TorpMuzzle6");
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
        GameObject missile = GameObject.Instantiate(currentPrefab, torpMuzzle.position, torpMuzzle.rotation) as GameObject;
        if (currentTarget == null || !targetLockStatus)
        {
            missile.GetComponent<MissileBehavior>().dumbFire(totalVelocityVector, (transform.Find("TargetingConvergence").position - torpMuzzle.position).normalized, weaponSelection);
        }
        else
        {
            missile.GetComponent<MissileBehavior>().setTarget(totalVelocityVector, (transform.Find("TargetingConvergence").position - torpMuzzle.position).normalized, currentTarget, weaponSelection, true);
        }

    }

    // Function called in Update() to fire
    void fireCannonsUpdate()
    {
        if (fireStatus == 1f && Time.time > fireRateTimestamp && weaponSelection == 0)
        {
            if (laserCharge > 0)
            {
                shipSounds.PlayOneShot(blasterClip);
                if (fireLinkStatus == 2)
                {
                    shootRay1();
                    shootRay2();
                    shootRay3();
                    shootRay4();
                    shootRay5();
                    shootRay6();
                }
                else if (fireLinkStatus == 1)
                {
                    if (fireLinkRotation == 0)
                    {
                        shootRay1();
                        shootRay2();
                        fireLinkRotation++;
                    }
                    else if (fireLinkRotation == 1)
                    {
                        shootRay3();
                        shootRay4();
                        fireLinkRotation++;
                    }
                    else
                    {
                        shootRay5();
                        shootRay6();
                        fireLinkRotation = 0;
                    }
                }
                else
                {
                    if (fireLinkRotation == 0)
                    {
                        shootRay1();
                        fireLinkRotation++;
                    }
                    else if (fireLinkRotation == 1)
                    {
                        shootRay2();
                        fireLinkRotation++;
                    }
                    else if (fireLinkRotation == 2)
                    {
                        shootRay3();
                        fireLinkRotation++;
                    }
                    else if (fireLinkRotation == 3)
                    {
                        shootRay4();
                        fireLinkRotation++;
                    }
                    else if (fireLinkRotation == 4)
                    {
                        shootRay5();
                        fireLinkRotation++;
                    }
                    else
                    {
                        shootRay6();
                        fireLinkRotation = 0;
                    }
                }

                fireRateTimestamp = Time.time + fireRate;
            }
            
        }
        else if (fireStatus == 1f && weaponSelection != 0)
        {
            int torpsToFire;
            int torpFired = 0;
            if (torpFireLinkStatus == 2)
            {
                torpsToFire = 6;
            }
            else if (torpFireLinkStatus == 1)
            {
                torpsToFire = 2;
            }
            else
            {
                torpsToFire = 1;
            }

            if (launch1Status == weaponSelection && torpsToFire > 0)
            {
                shootTorp1();
                launch1Status = 0;
                torpsToFire--;
                torpFired++;
            }
            if (launch2Status == weaponSelection && torpsToFire > 0)
            {
                shootTorp2();
                launch2Status = 0;
                torpsToFire--;
                torpFired++;
            }
            if (launch3Status == weaponSelection && torpsToFire > 0)
            {
                shootTorp3();
                launch3Status = 0;
                torpsToFire--;
                torpFired++;
            }
            if (launch4Status == weaponSelection && torpsToFire > 0)
            {
                shootTorp4();
                launch4Status = 0;
                torpsToFire--;
                torpFired++;
            }
            if (launch5Status == weaponSelection && torpsToFire > 0)
            {
                shootTorp5();
                launch5Status = 0;
                torpsToFire--;
                torpFired++;
            }
            if (launch6Status == weaponSelection && torpsToFire > 0)
            {
                shootTorp6();
                launch6Status = 0;
                torpsToFire--;
                torpFired++;
            }

            if (weaponSelection == 1 && torpFired > 0)
            {
                shipSounds.PlayOneShot(missileClip);
                cMissileCount -= torpFired;
            }
            else if (weaponSelection == 2 && torpFired > 0)
            {
                shipSounds.PlayOneShot(protonTorpClip);
                protonTorpCount -= torpFired;
            }
            else if (weaponSelection == 3 && torpFired > 0)
            {
                shipSounds.PlayOneShot(ionmissileClip);
                ionMissileCount -= torpFired;
            }
            else if (weaponSelection == 4 && torpFired > 0)
            {
                shipSounds.PlayOneShot(iontorpClip);
                ionTorpCount -= torpFired;
            }


            fireStatus = 0f;
        }
    }

    //************************************************TARGETING SYSTEM FUNCTIONS************************************************//
    // Change targeting convergence distance
    void changeTargetingConvergence(float changeValue)
    {

        if (weaponSelection == 0) // lasers
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
        else // missiles
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
                if (weaponSelection == 0) // lasers
                {
                    targetingConvergenceDifference += Mathf.Abs(targetDistance - targetingConvergence);
                    // The closer the targeting convergence to actual distance and the closer to the center of sight, the faster the lock
                    targetingRateFactor = targetingConvergenceDifference * targetAngle / 300f;
                }
                else
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
            crosshair.GetComponent<Image>().color = new Color32(255, 255, 255, 50);
            targetLockStatus = false;
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
    void engineLightIntensity()
    {
        if (throttleLevel < 100)
        {
            particleR.startColor = new Color(0, 0, 0, 0);
            particleL.startColor = new Color(0, 0, 0, 0);
        }
        else
        {
            Color particleColor = new Color(255, 0, 0, Mathf.Lerp(0, 20, enginePower / (float)enginePowerLimit));
            particleL.startColor = particleColor;
            particleR.startColor = particleColor;
        }

        //Update Engine Light Intensity
        float engineLightIntensity = Mathf.Lerp(0.0f, 0.1f, realSpeedGoal / maxSpeed);
        engineLightL.intensity = engineLightIntensity;
        engineLightR.intensity = engineLightIntensity;
    }

    //************************************************OVERRIDE FUNCTIONS************************************************//
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
        else
        {
            return 0;
        }
    }

    // Override function to retrieve current weapon selection
    public override int getCurrentWeaponSystem()
    {
        return weaponSelection;
    }

    void Update()
    {
        fireCannonsUpdate();
        PlayerControllerUpdate();
    }

    void FixedUpdate()
    {
        engineLightIntensity();
        calcTargetGuide(currentTarget != null);
        targetLock(currentTarget != null);
        PlayerControllerFixedUpdate();
        ShieldedPlayerFixedUpdate();
    }
}
