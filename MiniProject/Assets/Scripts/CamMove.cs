using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float moveSpeed = 10f;  // Speed of the camera movement

    void FixedUpdate()
    {
        // Get horizontal and vertical input based on arrow keys
        float horizontal = Input.GetAxis("Horizontal");  // Left (-1) / Right (+1)
        float vertical = Input.GetAxis("Vertical");      // Down (-1) / Up (+1)

        // Calculate the direction of movement
        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

        // Move the camera
        transform.position += direction * moveSpeed * Time.deltaTime;
    }
}
