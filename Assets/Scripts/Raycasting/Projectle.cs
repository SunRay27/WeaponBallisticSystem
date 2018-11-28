using System.Collections;
using UnityEngine;
using MyExtensions;
using System.Collections.Generic;

public class Projectle : CustomMonoBehaviour {

    float section = 0;//Section square
    float mass = 0.01f;// Properties from weapon
    float airResitance = 1;//Linear Resistance multiplier
    float hardness = 80;//TODO: Real-time hardness

    //Forces and positions
    Vector3 startPos, prevPos;
    Vector3 velocity;
    Vector3 airForce;

    //DEBUG//float startSpeed;
    //DEBUG//bool wall = false;
    float t = 0;

    //TODO: count derrivation based on speed, gun lenght etc.
    float derrivationMultiplier = 1;//Horizontal derviation multiplier
    float derrivationHeight = 1;//Vertical derivation multiplier

    public void SetParameters(Vector3 startVelocity, float linearResistanceMultiplier,float bulletMass,float sectionSquare,float derrivationMultiplier)
    {
        //DEBUG//startSpeed = startVelocity.magnitude;
        //Debug.Log("   Resist:" + linearResistanceMultiplier + "   Mass:" + bulletMass + "   Section:" + sectionSquare);
        startPos = transform.position;
        prevPos = startPos;
        velocity = startVelocity;

        this.derrivationMultiplier = derrivationMultiplier;
        derrivationHeight = Mathf.Sin(Vector3.Angle(Physics.gravity, velocity) * Mathf.Deg2Rad);
        airResitance = linearResistanceMultiplier;

        mass = bulletMass;
        section = sectionSquare;

        transform.forward = velocity.normalized;
        
        StartCoroutine(DisableParticles());
    }

    IEnumerator DisableParticles()
    {
        yield return new WaitForSeconds(2.5f);
        GetComponent<ParticleSystem>().Stop();
        yield return new WaitForSeconds(75f);
        PoolForWeapon.Instance.ReturnObjectToPool(transform);
    }
    float GetCParameter(float M)
    {
        if (M < 0.7f)
            return 0.157f;
        else if (M >= 0.7f && M < 1.2f)
            return -15.4444f * Mathf.Pow(M, 4) + 52.2f * Mathf.Pow(M, 3) - 63.8122f * Mathf.Pow(M, 2) + 33.738f * M - 6.387f;
        else if (M >= -1.2f && M < 3.5f)
            return 0.0210368f * Mathf.Pow(M, 2) - 0.153221f * M + 0.538572f;
        else
            return 0.26f;
    }
   /* float K(float M)
    {
        if (M < 0.7f)
            return 0.71f * M * M - 0.214f * M + 1.5f;
        if (M >= 0.7f && M < 1.5f)
            return -6.25f * M * M + 13.875f * M - 4.95f;
        if (M >= 1.5f && M < 3.2f)
            return -0.506667f * M * M * M + 3.91333f * M * M - 9.49667f * M + 8.95f;
        if (M >= 3.2f)
            return 2.030692f;
        return 0;
    }*/
    
    public override void OnUpdate (float dt)
    {
        //Debug.Log(velocity.magnitude + " -Speed, Force: " + airForce.magnitude);
        prevPos = transform.position;
        
        float soundSpeed = 343;
        airForce = airResitance * velocity.sqrMagnitude * -velocity.normalized * (1 + 13 * Mathf.Deg2Rad * Vector3.Angle(velocity, transform.forward));// * GetCParameter(velocity.magnitude / soundSpeed) * (1/0.157f);// ;
        Vector3 derivation = transform.right * derrivationHeight * Mathf.PI * Physics.gravity.magnitude * 0.001177f; //* 0.45f * 3 * 860 / 35 / (754 * 754 * 754) * 2730 * 2730 / 49 / 10;
        velocity += dt * (airForce +derivation + Physics.gravity);
        Vector3 step = velocity * dt;
        transform.position += step;

        /* Collision debug
          if (!wall)
          Debug.DrawLine(prevPos, transform.position, Color.green);
          else
          Debug.DrawLine(prevPos, transform.position, Color.red);
        */
        t += dt;

        //If it is smth in between two positions, check collision
        RaycastHit hit;
        if (Physics.Linecast(prevPos, transform.position, out hit))
        {
            HandleCollision(hit);
            //For decal System
            //DecalManager.Instance.SetDecal(hit.collider.tag,hit.point,hit.normal);
        }
        
    }
    void HandleCollision(RaycastHit hit)
    {
        //Debug distance and time
        print("delta x: " + (transform.position.x - startPos.x) + " t: " + t + "\n delta y: " + (transform.position.y - startPos.y) + "\n delta z: " + (transform.position.z - startPos.z));

        //If projectile is too slow, kill it
        if (velocity.sqrMagnitude < 5)
        {
            Die();
            return;
        }

        //We collided with something, say it to debugger
        //wall = true;

        //Get all hits (>1, if projectile is too fast and walls count != 1 too)
        RaycastHit[] hits = Physics.RaycastAll(prevPos, transform.position - prevPos, (transform.position - prevPos).magnitude);
        float c = 0;
        //Foreach every hit in hits
        while (c < hits.Length)
        {
            //Count start energy
            float E1 = mass * velocity.sqrMagnitude / 2;

            //Angle of bullet to wall
            float angle = Vector3.Angle(hit.normal, velocity) - 90;

            //Get maximum wall width
            float maxWidth = (E1 / hardness / 100);

            //Get point, where we can see, if we penetrated the wall
            Vector3 checkPoint = hit.point + velocity.normalized * maxWidth;

            DoDamage(hit, velocity);//Apply Damage

            RaycastHit newHit;
            //If there is point, so wall is penetrated
            if (Physics.Raycast(checkPoint, -velocity, out newHit, maxWidth))
            {
                float width = (newHit.point - hit.point).magnitude * 100;//Count real wall width
                float EP = width * hardness;//Count Enegy loss
                float E2 = E1 - EP;//Find difference
                //Save it so we dont need to count it every time
                float projectileMultiplier = 1/velocity.magnitude * width/mass;

                //Make sure E2 is greater than 0
                if (E2 < 0)
                {
                    Die();
                    return;
                }
                //OK penetrating further
                Vector3 n = new Vector3(0, hit.normal.y, 0); //Get wall normal's Y component
                
                //Count new velocity (mostly empirical)
                velocity = velocity.normalized * Mathf.Sqrt((2 / mass) * E2) 
                         - n * projectileMultiplier * Random.Range(1, 2f) * 25
                         + Random.insideUnitSphere * Random.Range(0f, 1f) * projectileMultiplier * hardness * hardness / 500;

                checkPoint = hit.point + velocity.normalized * maxWidth;//Recalculate check point
                Physics.Raycast(checkPoint, -velocity, out newHit, maxWidth);//Find REAL penetration point
                transform.position = newHit.point;//Move projectile to this point
                //DEBUG//  Debug.DrawLine(newHit.point, hit.point, Color.red, 1);
            }
            //Else reflect if RANDOM allows (empirical)
            //TODO: remake reflect system
            else if (Random.Range(0f, 1f) / angle / Mathf.Deg2Rad * (hardness) * section * 120 > 1)
            {
                transform.position = hit.point;//Move projectile to hit point
                //Count vertical angle
                float newAngle = Mathf.Clamp(angle * Mathf.Deg2Rad * Random.Range(1.3f, 2.2f), 0, Mathf.PI / 2f);
                //Count horizontal angle
                float randAngle = Random.Range(-1f, 1f) / 100 / mass * angle * angle * Mathf.Deg2Rad * Mathf.Deg2Rad + Mathf.PI / 2;
                //Reflect velocity by angles
                velocity = Vector3Extension.RandomReflectAngle(velocity, hit.normal, newAngle, randAngle) / (1 + angle * Mathf.Deg2Rad);
                //Normal correction
                velocity *= Mathf.Cos(angle * Mathf.Deg2Rad) * Mathf.Cos(angle * Mathf.Deg2Rad);

                if (velocity.sqrMagnitude < 5)
                {
                    Die();
                    return;
                }
                return;
            }
            else//didn't ricochet, didn't penetrate
            {
                Die();
                return;
            }
            //Check if there are other hits
            if (hits.Length>1)
            {
                Physics.Raycast(newHit.point, velocity, out hit,5);
                //DEBUG// Color lerped =  Color.Lerp(Color.green, Color.red, velocity.magnitude / startSpeed);
                //DEBUG// Debug.DrawLine(newHit.point, hit.point, lerped,1);
            }
            c++;
        }

    }
    void DoDamage(RaycastHit hit, Vector3 velocity)
    {
        if(hit.collider.CompareTag("Damagable"))
        {
            print("Damaged!" + velocity.sqrMagnitude*mass/2/section/100000+ "DG/cm^2" + " Distance: " + (startPos-transform.position).magnitude);
        }
        if (hit.transform.GetComponent<Rigidbody>())
        {
            float force = mass * velocity.magnitude/ hit.transform.GetComponent<Rigidbody>().mass;

            //   if (body.GetComponentInParent<RagdollController>())
            //       body.GetComponentInParent<RagdollController>().GetHit(velocity.normalized, body);
            //   else
            hit.transform.GetComponent<Rigidbody>().AddForceAtPosition(velocity.normalized * force, hit.point,ForceMode.VelocityChange);
        }
  //      if (hit.transform.GetComponentInParent<RagdollController>())
  //          hit.transform.GetComponentInParent<RagdollController>().GetHit(velocity, hit.transform.GetComponent<Rigidbody>());
    }
    void Die()
    {
        velocity = Vector3.zero;
        PoolForWeapon.Instance.ReturnObjectToPool(transform);
        StopAllCoroutines();
        return;
    }
    //Old ricochet system
  /*  void CountCollision(Vector3 pointNormal,Vector3 inSpeed, RaycastHit hit)
    { 
        float angle = Vector3.Angle(pointNormal, inSpeed) - 90;
        float rand = Random.value;
        bool doR = false;

        if (inSpeed.sqrMagnitude / startVel*startVel > 0.7f)
        if ((angle > 45 && rand >= 0.85f && angle <65) ||    //15%
           (angle <= 45 && rand >= 0.7f)) //|| // 30%
           //(angle >= 75 && rand >= 0.98f)) //2%
            doR = true;

        body = hit.transform.GetComponent<Rigidbody>();
       // print(Vector3.Distance(transform.position, startPos));
        if (doR  && Vector3.Distance(transform.position, startPos) > 0.5f && ric < maxRicochet)
        {
            float newAngle = Mathf.Clamp((Vector3.Angle(inSpeed, pointNormal) - 90) * Mathf.Deg2Rad * Random.Range(1f,2f),0,Mathf.PI / 2f);
            float randAngle = Random.Range(Mathf.PI/2.1f, Mathf.PI * 11 / 21);  
            velocity = Vector3Extension.RandomReflectAngle(inSpeed, pointNormal, newAngle, randAngle) / (Mathf.Clamp(angle,10f,90f) / 10) * Random.Range(0.5f, 1f);
            //velocity = Vector3Extension.ReflectAngle(inSpeed, pointNormal, newAngle) * Mathf.Cos(newAngle) / 2;
            if (body != null)
                {
                   float force = mass * velocity.sqrMagnitude * (1 - Mathf.Cos(newAngle));
                if (body.GetComponentInParent<RagdollController>())
                    body.GetComponentInParent<RagdollController>().GetHit(velocity.normalized, body);
                else
                    body.AddForceAtPosition(velocity.normalized * force, hit.point);
            }
            ric++;
            if(velocity.sqrMagnitude < startVel * startVel / 100)
            {
                velocity = Vector3.zero;
                PoolForWeapon.Instance.ReturnObjectToPool(transform);
                StopAllCoroutines();
            }
            
        }
        else
        {
          //   print("final speed" + velocity.magnitude + " Distance:" + (startPos-transform.position).magnitude + "Energy (G/sm^2): " + (mass*velocity.sqrMagnitude/2/sq/10000) + "(" + (mass * velocity.sqrMagnitude / 2) + ")" + " Time: " + t);
            //print("Wood: "+ mass * velocity.sqrMagnitude / 2/hardness + "cm , Brick: " + mass * velocity.sqrMagnitude / 2 / 342.5f + " cm");

            if (body != null)
            {
                float force = mass * velocity.sqrMagnitude;
                
               if(body.GetComponentInParent<RagdollController>())
                body.GetComponentInParent<RagdollController>().GetHit(velocity.normalized, body);
               else
                 body.AddForceAtPosition(velocity.normalized * force, hit.point);
            }
            velocity = Vector3.zero;
            PoolForWeapon.Instance.ReturnObjectToPool(transform);
            StopAllCoroutines(); 
        }
        



    }*/
}
