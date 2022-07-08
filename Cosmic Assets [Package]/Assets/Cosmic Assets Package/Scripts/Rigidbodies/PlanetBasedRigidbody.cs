using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

/*
 *  \brief A script that controls how a rigidbody moves based on the gravity state.
 */
public class PlanetBasedRigidbody : MonoBehaviour
{
    [Header("Movement")]
    public float maxVelocity = 50f; //!< The maximum velocity the game object can go. (Used to prevent the player from clipping into the ground.)

    [Header("Cosmic Settings")]
    public Transform gravitySource; //!< The transform of the gravity source.
    public float gravity = 3f; //!< The set gravity.

    [Header("Rigidbody Settings")]
    public float mass = 1f; //!< The rigidbody mass.
    public float drag = 0f; //!< The rigidbody drag.
    public float angularDrag = 25f; //!< The rigidbody angular drag.
    public bool useGravity = true; //!< A boolean that controls the rigidbody gravity state.
    public bool isKinematic = false; //!< A boolean that controls the rigidbody kinematic state.

    private Rigidbody rigidBody; //!< The game object rigidbody.

    void Awake()
    {
        rigidBody = gameObject.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        rigidBody.AddForce((gravitySource.transform.position - gameObject.transform.position).normalized * Time.deltaTime * gravity * 100f);

        // Limits the player's velocity.
        if (rigidBody.velocity.magnitude > maxVelocity) rigidBody.velocity = rigidBody.velocity.normalized * maxVelocity;
    }

    void Update()
    {
        // Set rigidbody values every frame. [Runtime]
        rigidBody.mass = mass;
        rigidBody.drag = drag;
        rigidBody.angularDrag = angularDrag;
        rigidBody.useGravity = false;
        rigidBody.isKinematic = isKinematic;
    }

    void OnValidate()
    {
        // Sets the rigidbody variable to the player's rigidbody component.
        rigidBody = gameObject.GetComponent<Rigidbody>();

        // Set rigidbody values every validation. [Editor]
        rigidBody.mass = mass;
        rigidBody.drag = drag;
        rigidBody.angularDrag = angularDrag;
        rigidBody.useGravity = false;
        rigidBody.isKinematic = isKinematic;
    }
}
