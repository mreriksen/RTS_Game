using System.Collections.Generic;
using UnityEngine;

public class UnitSelectionManager : MonoBehaviour
{
    public RectTransform selectionBox;  // UI element for selection box (assign in Inspector)
    public LayerMask unitLayerMask;     // Layer mask for unit detection

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
        HandleMouseInput();
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))  // Left mouse button down
        {
            startMousePosition = Input.mousePosition;
            selectionBox.gameObject.SetActive(false);
            ClearSelection();

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, unitLayerMask))
            {
                // If we clicked on a unit, select it
                SelectSingleUnit(hit.collider.gameObject);
            }
            else
            {
                // Start box selection if clicked on empty space
                isDragging = true;
            }
        }

        if (Input.GetMouseButton(0) && isDragging)  // While holding down left mouse button
        {
            UpdateSelectionBox(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))  // Left mouse button released
        {
            if (isDragging)
            {
                SelectUnitsInBox();
                isDragging = false;
                selectionBox.gameObject.SetActive(false);
            }
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
        if (unit.CompareTag("SelectableUnit"))  // Check if it's a selectable unit
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

    void ClearSelection()
    {
        selectedUnits.Clear();
    }
}
