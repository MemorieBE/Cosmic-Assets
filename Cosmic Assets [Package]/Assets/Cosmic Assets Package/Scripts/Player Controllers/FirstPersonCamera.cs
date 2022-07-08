using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *  \brief A script that controls the player's first person camera and how it rotates with the mouse.
 */
public class FirstPersonCamera : MonoBehaviour
{
    [Header("Camera")]
    public GameObject playerHeadCamera; //!< The camera used for the player's head.
    public Vector2 mouseSensitivity = new Vector2(100f, 100f); //!< The mouse sensitivity for both X and Y.
    private float xRotation = 0f; //!< The playerHeadCamera set rotation around the local X axis.

    void Start()
    {
        // Locks the cursor for first person movement.
        Cursor.lockState = CursorLockMode.Locked;
    }

    void FixedUpdate()
    {
        // Gets the total movement of the mouse's X and Y values since the last fixed update.
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity.x * Time.fixedDeltaTime * 10f;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity.y * Time.fixedDeltaTime * 10f;

        // Removes mouseY value from the playerHeadCamera's set X rotation.
        xRotation -= mouseY;
        // Limit's the playerHeadCamera's X rotation by 90 degrees. (Player can't look beyond up and down.)
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Applies xRotation value to the playerHeadCamera's X rotation.
        playerHeadCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        // Rotates the player around the local Y axis to the angle of mouseX.
        gameObject.transform.RotateAround(gameObject.transform.position, gameObject.transform.TransformDirection(Vector3.up), mouseX);
    }
}
