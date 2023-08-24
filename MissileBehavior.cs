using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MissileBehavior : ShipController
{
    public GameObject collisionExplosion;

    private Rigidbody missileRB;
    private ShipController target;
    private bool dumbfire;

    private float currentSpeed;
    private float terminalSpeed = 800;
    private float missileAcceleration = 0.5f;
    private int typeOfMissile;

    private float minDistancePredict = 500f;
    private float maxDistancePredict = 2000f;
    private float maxTimePrediction = 5f;
    private float rotateSpeed = 40;
    private float maxDistancetraveled = 3000;
    private float distanceTraveled;

    private float ionMissileTerminalSpeed = 500;
    private float ionMissileMissileAcceleration = 0.75f;
    private float ionMissileMinDistancePredict = 0f;
    private float ionMissileMaxDistancePredict = 2000f;
    private float ionMissileRotateSpeed = 200;
    private float ionMissileMaxDistance = 5000;

    private float cMissileTerminalSpeed = 500;
    private float cMissileMissileAcceleration = 0.75f;
    private float cMissileMinDistancePredict = 0f;
    private float cMissileMaxDistancePredict = 2000f;
    private float cMissileRotateSpeed = 200;
    private float cMissileMaxDistance = 5000;

    private float ionTorpTerminalSpeed = 350;
    private float ionTorpMissileAcceleration = 0.5f;
    private float ionTorpMinDistancePredict = 100f;
    private float ionTorpMaxDistancePredict = 2000f;
    private float ionTorpRotateSpeed = 100;
    private float ionTorpMaxDistance = 5000;

    private float protonTorpTerminalSpeed = 350;
    private float protonTorpMissileAcceleration = 0.5f;
    private float protonTorpMinDistancePredict = 100f;
    private float protonTorpMaxDistancePredict = 2000f;
    private float protonTorpRotateSpeed = 100;
    private float protonTorpMaxDistance = 5000;

    private float ionBombTerminalSpeed = 100;
    private float ionBombMissileAcceleration = 0.1f;
    private float ionBombMaxDistance = 1000;

    private float protonBombTerminalSpeed = 100;
    private float protonBombMissileAcceleration = 0.1f;
    private float protonBombMaxDistance = 1000;

    private Transform targetMarker;
    private bool friendly_missile;

    // Update is called once per frame
    /*void Update()
    {
        // transform.position += transform.forward * Time.deltaTime * 300f;// The step size is equal to speed times frame time.
        float step = speed * Time.deltaTime;

        if (m_target != null)
        {
            if (transform.position == m_target)
            {
                explode();
                return;
            }
            transform.position = Vector3.MoveTowards(transform.position, m_target, step);
        }

    }*/
    private void Start()
    {
        CrossSceneValues.projectilesList.Add(this);
        targetMarker = transform.Find("Canvas");
        targetMarker.gameObject.SetActive(false);
    }

    public override void toggleTargetMarker(bool setActive)
    {
        targetMarker.gameObject.SetActive(setActive);
    }

    void FixedUpdate()
    {
        if (dumbfire)
        {
            dumbFire();
            //missileRB.MovePosition(missileRB.position + laser_velocity * Time.fixedDeltaTime);
        }
        else
        {
            homingMissileAction();
        }

        if (distanceTraveled >= maxDistancetraveled)
        {
            explode();
            return;
        }
        
    }

    private void Update()
    {
        Transform aiCanvas = transform.Find("Canvas");
        aiCanvas.rotation = Camera.main.transform.rotation;

        float cameraDist = Vector3.Magnitude((aiCanvas.position - Camera.main.transform.position));
        aiCanvas.Find("TargetFrame").localScale = new Vector3(cameraDist, cameraDist, cameraDist) / 300;
    }

    void dumbFire()
    {
        currentSpeed = Mathf.Lerp(currentSpeed, terminalSpeed, Time.fixedDeltaTime * missileAcceleration);
        missileRB.position = missileRB.position + transform.forward * currentSpeed * Time.fixedDeltaTime;
        distanceTraveled += currentSpeed * Time.fixedDeltaTime;
    }

    void homingMissileAction()
    {
        currentSpeed = Mathf.Lerp(currentSpeed, terminalSpeed, Time.fixedDeltaTime * missileAcceleration);
        //missileRB.velocity = transform.forward * currentSpeed;
        missileRB.position = missileRB.position + transform.forward * currentSpeed * Time.fixedDeltaTime;

        // Predict future position
        bool jammed;
        float leadTimePercentage = Mathf.InverseLerp(minDistancePredict, maxDistancePredict, Vector3.Distance(transform.position, target.getJamPosition(friendly_missile, out jammed)));
        float predictionTime = Mathf.Lerp(0, maxTimePrediction, leadTimePercentage);
        Vector3 standardPrediction = target.getJamPosition(friendly_missile, out jammed) + target.getVelocity() * predictionTime;

        Vector3 heading = standardPrediction - transform.position;
        Quaternion rotation = Quaternion.LookRotation(heading);
        distanceTraveled += Mathf.Lerp(1f, 2f, (Quaternion.Angle(transform.rotation, rotation) % 180f) / 180f) * currentSpeed * Time.fixedDeltaTime;
        missileRB.MoveRotation(Quaternion.RotateTowards(transform.rotation, rotation, rotateSpeed * Time.fixedDeltaTime));

    }

    void OnCollisionEnter(Collision collision)
    {
        // IF missile collision
        if ("concussion_missile_prefab(Clone) (UnityEngine.GameObject)" == collision.gameObject.ToString() && typeOfMissile == 1)
        {
            return;
        }
        // IF proton torp collision
        if ("proton_torp_prefab(Clone) (UnityEngine.GameObject)" == collision.gameObject.ToString() && typeOfMissile == 2)
        {
            return;
        }
        // IF missile collision
        if ("ion_missile_prefab(Clone) (UnityEngine.GameObject)" == collision.gameObject.ToString() && typeOfMissile == 3)
        {
            return;
        }
        // IF proton torp collision
        if ("ion_torp_prefab(Clone) (UnityEngine.GameObject)" == collision.gameObject.ToString() && typeOfMissile == 4)
        {
            return;
        }
        explode();
    }

    void setMissileParameters(int type)
    {
        if (type == 1) // cmissile
        {
            terminalSpeed = cMissileTerminalSpeed;
            missileAcceleration = cMissileMissileAcceleration;
            minDistancePredict = cMissileMinDistancePredict;
            maxDistancePredict = cMissileMaxDistancePredict;
            rotateSpeed = cMissileRotateSpeed;
            maxDistancetraveled = cMissileMaxDistance;
        }
        else if (type == 2) // proton torp
        {
            terminalSpeed = protonTorpTerminalSpeed;
            missileAcceleration = protonTorpMissileAcceleration;
            minDistancePredict = protonTorpMinDistancePredict;
            maxDistancePredict = protonTorpMaxDistancePredict;
            rotateSpeed = protonTorpRotateSpeed;
            maxDistancetraveled = protonTorpMaxDistance;
        }
        else if (type == 3) // ion missile
        {
            terminalSpeed = ionMissileTerminalSpeed;
            missileAcceleration = ionMissileMissileAcceleration;
            minDistancePredict = ionMissileMinDistancePredict;
            maxDistancePredict = ionMissileMaxDistancePredict;
            rotateSpeed = ionMissileRotateSpeed;
            maxDistancetraveled = ionMissileMaxDistance;
        }
        else if (type == 4) // ion torp
        {
            terminalSpeed = ionTorpTerminalSpeed;
            missileAcceleration = ionTorpMissileAcceleration;
            minDistancePredict = ionTorpMinDistancePredict;
            maxDistancePredict = ionTorpMaxDistancePredict;
            rotateSpeed = ionTorpRotateSpeed;
            maxDistancetraveled = ionTorpMaxDistance;
        }
        else if (type == 5) // proton bomb
        {
            terminalSpeed = protonBombTerminalSpeed;
            missileAcceleration = protonBombMissileAcceleration;
            maxDistancetraveled = protonBombMaxDistance;
        }
        else if (type == 6) // ion bomb
        {
            terminalSpeed = ionBombTerminalSpeed;
            missileAcceleration = ionBombMissileAcceleration;
            maxDistancetraveled = ionBombMaxDistance;
        }
    }

    public void setTarget(Vector3 shipVelocity, Vector3 toZero, ShipController locked_target, int type, bool friendly) // if fired by player's side, friendly is true
    {
        missileRB = GetComponent<Rigidbody>();
        currentSpeed = shipVelocity.magnitude * 1.2f;
        target = locked_target;
        dumbfire = false;
        setMissileParameters(type);
        typeOfMissile = type;
        target.incomingWarning++;
        friendly_missile = friendly;
    }

    public void dumbFire(Vector3 shipVelocity, Vector3 toZero, int type)
    {
        missileRB = GetComponent<Rigidbody>();
        
        if (type >= 5)
        {
            currentSpeed = shipVelocity.magnitude;
        }
        else
        {
            currentSpeed = shipVelocity.magnitude * 1.2f;
        }
        dumbfire = true;
        setMissileParameters(type);
        typeOfMissile = type;
    }

    void explode()
    {
        if (!dumbfire)
        {
            target.incomingWarning--;
        }
        if (collisionExplosion != null)
        {
            GameObject explosion = (GameObject)Instantiate(
                collisionExplosion, transform.position, transform.rotation);
            Destroy(gameObject);
            Destroy(explosion, 1f);
        }
        CrossSceneValues.projectilesList.Remove(this);
        Destroy(gameObject);

    }
}