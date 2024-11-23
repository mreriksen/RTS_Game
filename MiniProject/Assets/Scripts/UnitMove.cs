using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitSelectionAndMovement : MonoBehaviour
{
    public RectTransform selectionBox;
    public LayerMask groundLayer;
    public LayerMask unitLayerMask;
    public LayerMask resourcesLayer;
    public NavMeshAgent agent;

    private Vector2 startMousePosition;
    private List<GameObject> selectedUnits = new List<GameObject>();
    private Camera cam;
    private bool isDragging = false;
   

    void Start()
    {
        cam = Camera.main;
        selectionBox.gameObject.SetActive(false);
    }

    void Update()
    {
        if (GetComponent<WorkerUnit>().currentHealth > 1)
        {
            HandleMouseInput();
        }
        else
        {
            for (int i = 0;  i == 1;)
            {
                i++;
                Dead();
            }
        }
        
    }

    void HandleMouseInput()
    {
        // Left mouse button down - start selection or unit move
        if (Input.GetMouseButtonDown(0))
        {
            startMousePosition = Input.mousePosition;
            selectionBox.gameObject.SetActive(false);
            ClearSelection();

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, unitLayerMask))
            {
                SelectSingleUnit(hit.collider.gameObject);
            }
            else
            {
                isDragging = true;  // Start box selection if no unit was clicked

            }

        }

        // Left mouse button held - update selection box size
        if (Input.GetMouseButton(0) && isDragging)
        {
            UpdateSelectionBox(Input.mousePosition);
        }

        // Left mouse button released - end selection
        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                SelectUnitsInBox();
                isDragging = false;
                selectionBox.gameObject.SetActive(false);
            }
        }

        // Right mouse button down - move selected units
        if (Input.GetMouseButtonDown(1))
        {
            MoveSelectedUnits();
        }

        // Press 'A' to attack the target and move
        if (Input.GetKeyDown(KeyCode.A))
        {
            AttackAndMove();
        }
    }

    void UpdateSelectionBox(Vector2 currentMousePosition)
    {
        if (!selectionBox.gameObject.activeInHierarchy)
            selectionBox.gameObject.SetActive(true);

        float width = currentMousePosition.x - startMousePosition.x;
        float height = currentMousePosition.y - startMousePosition.y;

        selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
        selectionBox.anchoredPosition = startMousePosition + new Vector2(width / 2, height / 2);
    }

    void SelectSingleUnit(GameObject unit)
    {
        if (unit.CompareTag("SelectableUnit"))
        {
            selectedUnits.Add(unit);
        }
    }

    void SelectUnitsInBox()
    {
        Vector2 min = selectionBox.anchoredPosition - (selectionBox.sizeDelta / 2);
        Vector2 max = selectionBox.anchoredPosition + (selectionBox.sizeDelta / 2);

        foreach (GameObject unit in GameObject.FindGameObjectsWithTag("SelectableUnit"))
        {
            Vector3 screenPos = cam.WorldToScreenPoint(unit.transform.position);

            if (screenPos.x >= min.x && screenPos.x <= max.x && screenPos.y >= min.y && screenPos.y <= max.y)
            {
                selectedUnits.Add(unit);
            }
        }
    }

    public void ClearSelection()
    {
        selectedUnits.Clear();
    }

    void MoveSelectedUnits()
    {
        
        // Perform a raycast to get the ground hit position
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            Debug.Log(hit.collider.gameObject.tag);
            Vector3 destination = hit.point;

            // Loop through selected units and move them
            foreach (GameObject unit in selectedUnits)
            {
                Debug.Log(unit.gameObject.tag);
                GetComponent<WorkerUnit>().StopGathering();
                if (unit == null)
                {
                    continue; // Skip destroyed units
                }

                agent = unit.GetComponent<NavMeshAgent>();
                if (agent != null && agent.isActiveAndEnabled)
                {
                    // Check if the unit's NavMeshAgent is valid and set destination
                    if (unit.activeInHierarchy) // Check if the unit is alive
                    {
                        agent.SetDestination(destination);
                        
                    }
                    else
                    {
                        Debug.LogWarning($"Unit {unit.name} is not active, skipping movement.");
                    }
                }
                else
                {
                    Debug.LogWarning($"NavMeshAgent not found or inactive on {unit.name}, skipping movement.");
                }
            }
            Debug.Log("Outside");
            if (Physics.Raycast(ray, out RaycastHit hitResource, Mathf.Infinity, groundLayer))
            {
                Debug.Log("Hit", hitResource.collider.gameObject);
                foreach (GameObject unit in selectedUnits)
                {
                    WorkerUnit workerUnit = unit.GetComponent<WorkerUnit>();
                    if (workerUnit != null)
                    {
                        Debug.Log("Work Work");
                        workerUnit.StartGathering(hitResource.collider.gameObject);
                        agent = unit.GetComponent<NavMeshAgent>();
                        if (agent != null)
                        {
                            agent.SetDestination(destination);
                        }
                    }
                }
            }
        }
    }

    void AttackAndMove()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            // If the clicked target is an enemy, move and attack it
            if (hit.collider.CompareTag("Enemy"))
            {
                foreach (GameObject unit in selectedUnits)
                {
                    if (unit == null)
                    {
                        continue;
                    }

                    WorkerUnit workerUnit = unit.GetComponent<WorkerUnit>();
                    if (workerUnit != null)
                    {
                        workerUnit.MoveAndAttack(hit.collider.gameObject);
                    }
                }
            }
        }
    }
    void Dead() 
    {
        StopAllCoroutines();
        agent = null;
    }
}
