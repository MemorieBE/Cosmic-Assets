using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

/*
 *  \brief A script that controls how a rigidbody moves based on a single planet's gravity state.
 */
public class GravityBasedRigidbody : MonoBehaviour
{
    [Header("Movement")]
    public float maxVelocity = 50f; //!< The maximum velocity the game object can go. (Used to prevent the player from clipping into the ground.)

    [Header("Cosmic Settings")]
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
        // Limits the player's velocity.
        if (rigidBody.velocity.magnitude > maxVelocity) rigidBody.velocity = rigidBody.velocity.normalized * maxVelocity;
    }

    void OnTriggerStay(Collider collisionData)
    {
        if (collisionData.gameObject.GetComponent<GravitySource>() != null)
        {
            GravitySource gravitySource = collisionData.gameObject.GetComponent<GravitySource>();
            gravitySource.cosmicRBDetected = true;

            float currentPull = 0f;

            if (gravitySource.gradualGravity)
            {
                float gravityMaxRadius = gravitySource.range * Mathf.Max(collisionData.gameObject.transform.lossyScale.x, collisionData.gameObject.transform.lossyScale.y, collisionData.gameObject.transform.lossyScale.z);
                float gravityMinRadius = gravityMaxRadius * gravitySource.rangeTerminalCutoff;
                float gravityLength = gravityMaxRadius - gravityMinRadius;

                currentPull = (gravityLength - (Vector3.Distance(gameObject.transform.position, collisionData.gameObject.transform.position) - gravityMinRadius)) / gravityLength * 2f;
                if (currentPull >= 2f) currentPull = 2f;
                currentPull *= gravitySource.gravitationalForce;
            }
            else
            {
                currentPull = gravitySource.gravitationalForce;
            }

            rigidBody.AddForce((collisionData.gameObject.transform.position - gameObject.transform.position).normalized * Time.deltaTime * gravity * currentPull);
        }
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
