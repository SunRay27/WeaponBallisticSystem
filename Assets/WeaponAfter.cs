using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BulletParameters2
{
    [SerializeField]
    [Header("Configuration")]
    float mass;
    [SerializeField]
    float caliber = 9.27f;
    [SerializeField]
    float powder = 1.4f;//in gramms

    [SerializeField]
    [Header("Environment resistance")]
    float derivationMultiplier = 1f;//Remove in future
    [SerializeField]
    float empericalMultiplier = 1;


    //Convert to SI 
    public float Mass
    {
        get { return mass / 1000; }
    }
    public float Radius
    {
        get { return caliber / 2000; }
    }
    public float SqrRadius
    {
        get { return caliber * caliber / 2000 / 2000; }
    }
    public float Powder
    {
        get { return powder / 1000; }
    }
    public float Derrivation
    {
        get { return derivationMultiplier; }
    }
    public float EmpMultiplier
    {
        get { return empericalMultiplier; }
    }
}

public class Weapon2 : MonoBehaviour
{

    public BulletParameters2 bullet;
    //Weapon parameters
    [Header("Weapon Setup")]
    public float length = 415;
    public float startSpeed,
             airToughness = 1.29f;

    public float shotsPerMinute = 300;
    public float angleError = 0;
    public bool auto = false;
    public float temperature = 0;

    //Visual
    [Header("Visual")]
    public Transform firePoint;
    public Light flicker;
    public AudioClip fireSound;



    float t;
    float linearAirResistance = 0;
    float delay = 0.1f;
    float square = 1;
    Quaternion oldRotation;
    private void Awake()
    {
        oldRotation = firePoint.localRotation;
        delay = 60 / (shotsPerMinute);
        linearAirResistance = 4.8f / 10 * 4 * bullet.EmpMultiplier * bullet.SqrRadius / bullet.Mass;
        square = Mathf.PI * bullet.SqrRadius;
    }
    void Start()
    {
        // EventManager.Instance.AddListener(GlobalEvent.PlayerDied, this);
    }
    // Update is called once per frame
    IEnumerator Flick()
    {
        flicker.enabled = true;
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        flicker.enabled = false;
    }
    // float tLoss = 0;
    float C = 480;
    void Update()
    {
        // tLoss = temperature / 500;
        if (temperature > 273)
            temperature -= (temperature - 273) / 60 * Time.deltaTime;

        // float sin = Mathf.Sin(Mathf.Deg2Rad * bullet.angle / 2);
        // float square = Mathf.PI / 4 * Mathf.Pow(bullet.caliber / 1000, 2) * sin;
        // float C = GetCParameter(startSpeed/ soundSpeed);
        //linearAirResistance = 4.8f / 10 * bullet.empericalMultiplier * Mathf.Pow(bullet.caliber / 1000, 2) / bullet.Mass;

        //airMult =  airToughness * square * sin * sin;
        //square = Mathf.PI / 4 * Mathf.Pow(bullet.caliber / 1000, 2);


        Transform bul = null;
        startSpeed = 1566f * length*length * bullet.Powder;
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            GetComponent<AudioSource>().Play();
            StartCoroutine(Flick());
            firePoint.localRotation = Quaternion.Euler(Random.insideUnitSphere * (angleError + (temperature - 273) / 400)) * firePoint.localRotation;
            float totalSpeed = startSpeed + 0.01f * temperature - 9.33f / 100000 * temperature * temperature;
            bul = PoolForWeapon.Instance.GetObjectToPosition(firePoint.position).transform;

            bul.GetComponent<ProjectleFinal>().SetParameters(firePoint.forward.normalized * totalSpeed, linearAirResistance, bullet.Mass, square, bullet.Derrivation);

            firePoint.localRotation = oldRotation;
            temperature += totalSpeed * totalSpeed * (bullet.Mass + 9 / 1000) / 2 / 3 / C / 1f;
        }
        t += Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (t > delay)
            {
                // Debug.Break();
                //AudioSource.PlayClipAtPoint(fireSound, firePoint.position);
                GetComponent<AudioSource>().Play();
                StartCoroutine(Flick());
                firePoint.localRotation = Quaternion.Euler(Random.insideUnitSphere * (angleError + (temperature - 273) / 400)) * firePoint.localRotation;
                float totalSpeed = startSpeed + 0.01f * temperature - 9.33f / 100000 * temperature * temperature;
                bul = PoolForWeapon.Instance.GetObjectToPosition(firePoint.position).transform;
                bul.GetComponent<ProjectleFinal>().SetParameters(firePoint.forward.normalized * totalSpeed, linearAirResistance, bullet.Mass, square, bullet.Derrivation);
                t = 0;
                firePoint.localRotation = oldRotation;
                temperature += totalSpeed * totalSpeed * (bullet.Mass + 9 / 1000) / 2 / 3 / C / 1f;
            }
        }
        if (auto)
        {
            if (t > delay)
            {
                //c++;
                //    GetComponent<AudioSource>().Play();
                //    StartCoroutine(Flick());
                firePoint.localRotation = Quaternion.Euler(Random.insideUnitSphere * (angleError + (temperature - 273) / 400)) * firePoint.localRotation;
                float totalSpeed = startSpeed + 0.01f * temperature - 9.33f / 100000 * temperature * temperature;
                bul = PoolForWeapon.Instance.GetObjectToPosition(firePoint.position).transform;
                bul.GetComponent<ProjectleFinal>().SetParameters(firePoint.forward.normalized * totalSpeed, linearAirResistance, bullet.Mass, square, bullet.Derrivation);
                t = 0;
                firePoint.localRotation = oldRotation;

                //print("dT: " + totalSpeed * totalSpeed * (bullet.mass+ 9) / 1000 / 2 / 3 / C / 1f + " (" + temperature +  ") Speed: " + totalSpeed + " (" + startSpeed + ") Error:" + (error + (temperature-273) / 1000) + " (" + error + ")");

                temperature += totalSpeed * totalSpeed * (bullet.Mass + 9 / 1000) / 2 / 3 / C / 1f;

                // print("speed: " + totalSpeed * totalSpeed * bullet.mass / 1000 / 2 / 3 / C / 0.8f);
            }
        }
    }
}
