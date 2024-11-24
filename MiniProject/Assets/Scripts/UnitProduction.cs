using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitProductionBuilding : MonoBehaviour
{
    [System.Serializable]
    public class UnitType
    {
        public string unitName;
        public GameObject unitPrefab;
        public int costGold;
        public int costLumber;
        public float productionTime;
        public int food;
    }

    public List<UnitType> availableUnits = new List<UnitType>(); // List of units this building can produce
    public Transform spawnPoint; // Spawn location for units (now a Transform)

    private Queue<UnitType> productionQueue = new Queue<UnitType>();
    private bool isProducing = false;

    private void Start()
    {
        if (spawnPoint == null) // Check if spawnPoint is not set in the inspector
        {
            spawnPoint = transform; // Default to the building's position if not set
        }
    }

    void Update()
    {
        // Example input for adding units to production queue
        if (Input.GetKeyDown(KeyCode.P))
        {
            ProduceUnit("Peon");
        }
    }

    public void ProduceUnit(string unitName)
    {
        UnitType unitToProduce = availableUnits.Find(unit => unit.unitName == unitName);

        if (unitToProduce != null)
        {
            if (ResourceManager.goldAmount >= unitToProduce.costGold && ResourceManager.lumberAmount >= unitToProduce.costLumber && ResourceManager.food <= ResourceManager.foodMax)
            {
                ResourceManager.goldAmount -= unitToProduce.costGold;
                ResourceManager.lumberAmount -= unitToProduce.costLumber;
                ResourceManager.food += unitToProduce.food;
                productionQueue.Enqueue(unitToProduce);
                Debug.Log($"{unitName} added to production queue.");

                if (!isProducing)
                {
                    StartCoroutine(ProduceUnitFromQueue());
                }
            }
            else
            {
                Debug.Log("Not enough resources to produce this unit.");
            }
        }
    }

    private IEnumerator ProduceUnitFromQueue()
    {
        isProducing = true;

        while (productionQueue.Count > 0)
        {
            UnitType unit = productionQueue.Dequeue();
            Debug.Log($"Producing {unit.unitName}, time required: {unit.productionTime} seconds.");

            yield return new WaitForSeconds(unit.productionTime);

            // Spawn the unit at the spawnPoint's position
            GameObject newUnit = Instantiate(unit.unitPrefab, spawnPoint.position, Quaternion.identity);
            Debug.Log($"{unit.unitName} has been produced!");
        }

        isProducing = false;
    }
}
