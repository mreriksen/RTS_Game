using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyUnit : MonoBehaviour
{
    public int health;
    public int maxHealth;
    public float speed;
    public int damage;
    public float attackSpeed = 10f;
    public float closestDistance = 4f;

    public void Update()
    {
        attack();
        dead();
    }

    void attack()
    {
        delay(attackSpeed);
        WorkerUnit[] allWorkers = FindObjectsOfType<WorkerUnit>();
        WorkerUnit closestWorker = null;
        

        if (health < maxHealth)
        {
            Debug.Log("Angy");
            foreach (var worker in allWorkers)
            {
                float distance = Vector3.Distance(transform.position, worker.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestWorker = worker;
                }
            }

            if (closestWorker != null)
            {
                closestWorker.currentHealth -= damage;
                Debug.Log("Angy HIT");
            }
        }
    }
    private IEnumerator delay(float a)
    {
        yield return new WaitForSeconds(a);
    }

    void dead()
    {
        if (health < 1)
        {
            Destroy(gameObject);
        }
    }
}
