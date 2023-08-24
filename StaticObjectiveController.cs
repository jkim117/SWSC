using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticObjectiveController : ShipController
{
    // SHIP HEALTH VALUES CONFIGURABLE
    public float totalShipHealth;
    // SHIP HEALTH VALUES VARIABLE
    public float shipHealth;

    // EFFECTS VALUES CONFIGURABLE
    public GameObject explosionPrefab;
    public AudioClip explosionClip;
    public AudioSource shipSounds; // audio source used for ship sounds

    protected Transform targetMarker;
    public string shipName;

    // Start is called before the first frame update
    protected void StaticControllerStart()
    {
        targetMarker = transform.Find("Canvas");
        targetMarker.gameObject.SetActive(false);
        sensorBurnLimit = 1000;
        sensorRangeLimit = 10000f;
        offOpposingList = false;

        if (friendly)
        {
            CrossSceneValues.objectiveList.Add(this);
            CrossSceneValues.objectiveList_enemy.Add(this);
        }
        else
        {
            CrossSceneValues.objectiveList_enemy.Add(this);
            CrossSceneValues.objectiveList.Add(this);
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

    public override string getShipName()
    {
        return shipName;
    }

    public override void destroyShipController()
    {
        CrossSceneValues.objectiveList.Remove(this);
        CrossSceneValues.objectiveList_enemy.Remove(this);
        Destroy(gameObject);
    }

    protected virtual void takeDamage(float damage, bool frontOrRear)
    {
        /*shipHealth -= damage;

        if (shipHealth <= 0)
        {
            CrossSceneValues.objectiveList.Remove(this);
            GameObject explosion = GameObject.Instantiate(explosionPrefab, transform.position, transform.rotation);
            explosion.GetComponent<ParticleSystem>().Play();
            AudioSource.PlayClipAtPoint(explosionClip, transform.position, 1f);
            Destroy(gameObject);
        }*/
    }
    protected virtual void takeIonDamage(float damage, bool frontOrRear)
    {
    }

    void OnCollisionEnter(Collision collision)
    {
        /*foreach (ContactPoint contact in collision.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal, Color.white);
        }
        if (collision.relativeVelocity.magnitude > 2)
            audioSource.Play();*/
        //Debug.Log(collision.relativeVelocity.magnitude);


        /*ContactPoint contact = collision.GetContact(0);
        Vector3 contactPointFromCenter = (contact.point - transform.position);
        bool frontDamage = collision.relativeVelocity.x > 0;

        if (collision.gameObject.ToString().Contains("shot_prefab"))
        {
            ShotBehavior sb = collision.gameObject.GetComponent<ShotBehavior>();
            if (sb.isIonDamage())
            {
                takeIonDamage(sb.getDamage(), frontDamage);
            }
            else
            {
                takeDamage(sb.getDamage(), frontDamage);
            }
            return;
        }

        // IF missile collision
        if ("concussion_missile_prefab(Clone) (UnityEngine.GameObject)" == collision.gameObject.ToString())
        {
            takeDamage(100, frontDamage);
            return;
        }
        // IF proton torp collision
        if ("proton_torp_prefab(Clone) (UnityEngine.GameObject)" == collision.gameObject.ToString())
        {
            takeDamage(500, frontDamage);
            return;
        }
        // IF missile collision
        if ("ion_missile_prefab(Clone) (UnityEngine.GameObject)" == collision.gameObject.ToString())
        {
            takeIonDamage(500, frontDamage);
            return;
        }
        // IF ion torp collision
        if ("ion_torp_prefab(Clone) (UnityEngine.GameObject)" == collision.gameObject.ToString())
        {
            takeIonDamage(5000, frontDamage);
            return;
        }
        // IF proton bomb collision
        if ("proton_bomb_prefab(Clone) (UnityEngine.GameObject)" == collision.gameObject.ToString())
        {
            takeDamage(1000, frontDamage);
            return;
        }
        // IF ion bomb collision
        if ("ion_bomb_prefab(Clone) (UnityEngine.GameObject)" == collision.gameObject.ToString())
        {
            takeIonDamage(6000, frontDamage);
            return;
        }*/
    }

    
}
