using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class WorkerUnit : MonoBehaviour
{
    // Worker properties
    [Header("Worker Stats")]
    public int maxHealth = 50;
    public int currentHealth;
    public float speed = 3.5f;
    public int healingRate = 1;

    [Header("Combat Stats")]
    public int attack = 5;
    public float attackRange = 1.2f;
    public float attackCooldown = 1.0f;

    [Header("Gathering")]
    public int gatherRate = 5;
    public float gatherInterval = 2.0f;

    [Header("Building")]
    public GameObject[] buildingPrefabs;
    public int[] buildingGoldCosts;
    public int[] buildingLumberCosts;
    public GameObject previewMaterial;

    // Internal variables
    private float lastAttackTime = -Mathf.Infinity;
    private bool isGathering = false;
    private bool isBuilding = false;
    private bool isPlacing = false;
    private bool isAttacking = false;
    private bool isBuildingMode = false;

    private NavMeshAgent agent;
    private Coroutine gatherCoroutine;
    private Coroutine attackCoroutine;
    private GameObject currentTarget;
    private GameObject previewBuilding;
    private Vector3 targetBuildPosition;

    private static WorkerUnit activeBuilder = null;

    void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;

        StartCoroutine(HealOverTime());
    }

    void Update()
    {
        if (currentHealth <= 0)
        {
            Die();
        }

        HandleBuildingModeToggle();
        HandleBuildingPreview();
    }

    public void MoveTo(Vector3 destination, System.Action onArrival)
    {
        if (agent != null)
        {
            agent.SetDestination(destination);
            StartCoroutine(WaitUntilReachedDestination(onArrival));
        }
    }

    private IEnumerator WaitUntilReachedDestination(System.Action onArrival)
    {
        while (agent.remainingDistance > agent.stoppingDistance)
            yield return null;

        onArrival?.Invoke();
    }

    public void StartGathering(GameObject resource)
    {
        Debug.Log("test StartGathering");
        if (!isGathering)
        {
            isGathering = true;
            gatherCoroutine = StartCoroutine(GatherResource(resource));
        }
    }

    public void StopGathering()
    {
        if (isGathering)
        {
            isGathering = false;
            if (gatherCoroutine != null)
            {
                StopCoroutine(gatherCoroutine);
            }
        }
    }

    private IEnumerator GatherResource(GameObject resource)
    {
        while (isGathering)
        {
            // Wait for the specified interval before gathering again
            yield return new WaitForSeconds(gatherInterval);

            // Gather resources based on the resource type
            if (resource != null)
            {
                Debug.Log($"Gathered {gatherRate} resources from {resource.tag}");

                if (resource.tag == "tree")
                {
                    ResourceManager.lumberAmount += gatherRate;
                }
                else if (resource.tag == "Goldmine")
                {
                    Debug.Log("Money");
                    ResourceManager.goldAmount += gatherRate;
                }
            }
        }
    }

    public void Attack(GameObject target)
    {
        if (Time.time - lastAttackTime < attackCooldown)
        {
            Debug.Log("Attack on cooldown.");
            return;
        }

        if (target.CompareTag("Enemy") && Vector3.Distance(transform.position, target.transform.position) <= attackRange)
        {
            Debug.Log($"Attacking {target.name}");
            target.GetComponent<enemyUnit>().health -= attack;
            lastAttackTime = Time.time;
        }
        else
        {
            Debug.Log("Target out of range.");
        }
    }

    public void MoveAndAttack(GameObject target)
    {
        StopAttacking();
        currentTarget = target;

        agent.SetDestination(target.transform.position);
        attackCoroutine = StartCoroutine(FollowAndAttackCoroutine());
    }

    private IEnumerator FollowAndAttackCoroutine()
    {
        isAttacking = true;

        while (currentTarget != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (distanceToTarget <= attackRange)
                Attack(currentTarget);
            else
                agent.SetDestination(currentTarget.transform.position);

            yield return new WaitForSeconds(0.5f);
        }

        isAttacking = false;
    }

    public void StopAttacking()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            isAttacking = false;
            currentTarget = null;
        }
    }

    private void HandleBuildingModeToggle()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isBuildingMode = !isBuildingMode;
            Debug.Log(isBuildingMode ? "Building mode activated!" : "Building mode deactivated.");
        }

        if (isBuildingMode && !isPlacing)
        {
            for (int i = 0; i < buildingPrefabs.Length; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    StartPlacingBuilding(i);
                    isBuildingMode = false;
                }
            }
        }
    }

    public void StartPlacingBuilding(int buildingIndex)
    {
        if (!CanAffordBuilding(buildingIndex)) return;

        activeBuilder = this;
        isPlacing = true;
        previewBuilding = Instantiate(buildingPrefabs[buildingIndex]);
        SetBuildingPreviewProperties(previewBuilding);
        DeductResources(buildingIndex);
    }

    private void HandleBuildingPreview()
    {
        if (!isPlacing || activeBuilder != this) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Ensure the building is aligned with the ground
            Vector3 correctedPosition = hit.point;
            
            // Adjust Y position based on the collider's extents (if it has one)
            Collider buildingCollider = previewBuilding.GetComponent<Collider>();
            if (buildingCollider != null)
            {
                correctedPosition.y = 0;
            }
            previewBuilding.transform.position = correctedPosition;
            SetPreviewColor(IsValidPlacement(correctedPosition) ? Color.green : Color.red);
        }

        // Place building on left-click
        if (Input.GetMouseButtonDown(0) && IsValidPlacement(previewBuilding.transform.position))
        {
            PlaceBuilding(previewBuilding.transform.position);
        }
        else if (Input.GetMouseButtonDown(1)) // Cancel on right-click
        {
            CancelBuildingPlacement();
        }
    }

    private void PlaceBuilding(Vector3 position)
    {
        // Destroy the preview and reset placement state
        Destroy(previewBuilding);
        isPlacing = false;
        activeBuilder = null;

        // Adjust position to ensure the building is aligned with the ground
        Vector3 finalPosition = position;
        if (Physics.Raycast(new Ray(position + Vector3.up * 10, Vector3.down), out RaycastHit hit, Mathf.Infinity))
        {
            finalPosition = hit.point;
            Collider buildingCollider = buildingPrefabs[0].GetComponent<Collider>();
            if (buildingCollider != null)
            {
                finalPosition.y += buildingCollider.bounds.extents.y; // Adjust for collider height
            }
        }

        MoveTo(finalPosition, () => StartConstruction(finalPosition));
    }


    private void StartConstruction(Vector3 buildPosition)
    {
        if (isBuilding) return;

        isBuilding = true;
        StartCoroutine(BuildingConstructionDelay(3.0f, buildPosition));
    }

    private IEnumerator BuildingConstructionDelay(float delay, Vector3 position)
    {
        yield return new WaitForSeconds(delay);
        Instantiate(buildingPrefabs[0], position, Quaternion.identity); // Adjust building index if needed
        isBuilding = false;
    }

    private void CancelBuildingPlacement()
    {
        if (previewBuilding != null)
        {
            Destroy(previewBuilding);
            isPlacing = false;
            activeBuilder = null;
        }
    }

    private bool CanAffordBuilding(int buildingIndex)
    {
        return ResourceManager.goldAmount >= buildingGoldCosts[buildingIndex] &&
               ResourceManager.lumberAmount >= buildingLumberCosts[buildingIndex];
    }

    private void DeductResources(int buildingIndex)
    {
        ResourceManager.goldAmount -= buildingGoldCosts[buildingIndex];
        ResourceManager.lumberAmount -= buildingLumberCosts[buildingIndex];
    }

    private bool IsValidPlacement(Vector3 position) => true;

    private void SetBuildingPreviewProperties(GameObject building)
    {
        foreach (var renderer in building.GetComponentsInChildren<Renderer>())
        {
            renderer.material.color = new Color(1, 1, 1, 0.5f);
        }
    }

    private void SetPreviewColor(Color color)
    {
        foreach (var renderer in previewBuilding.GetComponentsInChildren<Renderer>())
        {
            renderer.material.color = color;
        }
    }

    private IEnumerator HealOverTime()
    {
        while (currentHealth < maxHealth)
        {
            yield return new WaitForSeconds(1f);
            currentHealth = Mathf.Min(currentHealth + healingRate, maxHealth);
            Debug.Log($"Healing... Current Health: {currentHealth}");
        }
    }

    private void Die()
    {
        GetComponent<UnitSelectionAndMovement>().ClearSelection();
        StopAllCoroutines();
        GetComponent<WorkerUnit>().gameObject.SetActive(false);
        
    }

}
