using System.Collections;
using UnityEngine;

public class MeleeCollider : MonoBehaviour
{
    public Melee melee;

    float timer = 10f;
    bool hitEnemy = false;

    private void Update()
    {
        if (hitEnemy && timer >= 0.2f)
        {
            hitEnemy = false;
        }

        timer += Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy" && !hitEnemy)
        {
            hitEnemy = true;
            timer = 0f;
            melee.AttackEnemy(other);
        }
    }
}
