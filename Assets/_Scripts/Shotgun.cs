using UnityEngine;

public class Shotgun : Gun
{
    [Header("Shotgun Spread")]
    public int pellets = 8;
    public float spreadMultiplier = 0.2f;

    public override void Shoot()
    {
        GunshotAudioSource.Play();
        MuzzleFlash.Play();

        ammoInClip--;

        Vector3[] pelletDirections = getPelletDirections();

        foreach (Vector3 pelletDirection in pelletDirections)
        {
            RaycastHit hit;
            if (Physics.Raycast(fpsCamera.transform.position, pelletDirection, out hit, range))
            {
                EnemyHealth enemy = hit.transform.GetComponentInParent<EnemyHealth>();

                Debug.DrawRay(fpsCamera.transform.position, fpsCamera.transform.forward * range, Color.red, 2f);

                if (enemy != null)
                {
                    bool lethalAttack = false;

                    if (!enemy.isDead)
                    {
                        // Apply damage to enemy, if headshot apply double damage
                        lethalAttack = enemy.TakeDamage(damage/pellets);
                        if (hit.transform.name.Contains("Head"))
                            lethalAttack = enemy.TakeDamage(damage/pellets);

                        // Instantiate the blood impact effect
                        GameObject impact = Instantiate(bloodImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(impact, 1f);
                    }

                    if (hit.rigidbody != null || lethalAttack)
                    {
                        hit.rigidbody.AddForce(-hit.normal * (impactForce / pellets));
                    }
                }
                else
                {
                    GameObject impact = Instantiate(worldImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impact, 1f);
                }
            }
        }

        timer = 0f;
    }

    /// <summary>
    /// Generates a Vector3 direction for each of the individual pellets of the shotgun, each with a unique and random spread
    /// </summary>
    /// <returns></returns>
    Vector3[] getPelletDirections()
    {
        Vector3[] pelletSpreads = new Vector3[pellets];

        for (int i = 0; i < pelletSpreads.Length; i++)
        {
            Vector3 direction = fpsCamera.transform.forward; // your initial aim.

            Vector3 spread = fpsCamera.transform.up * Random.Range(-1f, 1f); // add random up or down (because random can get negative too)
            spread += fpsCamera.transform.right * Random.Range(-1f, 1f); // add random left or right

            direction += spread.normalized * Random.Range(0f, spreadMultiplier);

            pelletSpreads[i] = direction;
        }

        return pelletSpreads;
    }
}

