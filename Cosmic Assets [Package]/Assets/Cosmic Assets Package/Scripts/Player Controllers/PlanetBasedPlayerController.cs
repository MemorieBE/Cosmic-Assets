using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]

/*
 *  \brief A script that controls how the player moves based on a single planet's gravity state and the player inputs.
 */
public class PlanetBasedPlayerController : MonoBehaviour
{
    [Header("Player")]
    public GameObject playerCamera; //!< The object used as the player camera.

    [Header("Layer")]
    public int[] ignoreLayers; //!< The layers that the ground check raycast will ignore.

    [Header("Movement Speed")]
    public float speed = 8f; //!< The overall speed of the player.
    public float sprintMultiplier = 1.5f; //!< The number multiplied by speed to calculate the player's sprint speed.
    public bool angleBasedMovement = false; //!< A boolean that controls whether or not the player moves along the angle or circumference of the planet.

    [Header("Movement Jump")]
    public float jumpForce = 6f; //!< The overall jump force of the player.
    public float jumpPause = 0.5f; //!< The time in seconds between possible jumps.
    private float jumpPauseTimer = 0f; //!< The timer used to determine how much time is left to be able to jump again.

    [HideInInspector] public bool isGrounded = false; //!< Whether or not the player is currently touching the ground. [Read Only]
    private float groundCheckDistance = 0.1f; //!< The distance from the base of the player to check the ground. (Lower number means that the player needs to be closer to the ground in order for the ground check to be true.)

    [Header("Cosmic Settings")]
    public Transform gravitySource; //!< The transform of the gravity source. (Used for position only.)
    public float gravity = 3f; //!< Gravity multiplier for the gravity source.

    [Header("Rigidbody Settings")]
    public float mass = 1f; //!< The rigidbody mass.
    public float drag = 0f; //!< The rigidbody drag.
    public float angularDrag = 25f; //!< The rigidbody angular drag.
    public bool useGravity = true; //!< A boolean that controls the rigidbody gravity state.
    public bool isKinematic = false; //!< A boolean that controls the rigidbody kinematic state.

    public float maxVelocity = 50f; //!< The max velocity the player can go. (Used to prevent the player from clipping into the ground.)
    public bool noHorizontalVelocity = true; //!< Limits the players velocity to move towards the gravity source only.

    public static string forwardMoveKey = "w"; //!< Key used to move forward.
    public static string leftMoveKey = "a"; //!< Key used to move left.
    public static string backMoveKey = "s"; //!< Key used to move back.
    public static string rightMoveKey = "d"; //!< Key used to move right.
    public static string jumpKey = "space"; //!< Key used to jump.
    public static string sprintKey = "left shift"; //!< Key used to sprint.

    private Rigidbody rigidBody; //!< References the player's rigidbody component.

    void Awake()
    {
        //Sets the rigidbody variable to the player's rigidbody component.
        rigidBody = gameObject.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        {   // Player Movement //

            //Gets local direction of player movement.
            Vector3 localDirection = Vector3.zero;
            if (Input.GetKey(forwardMoveKey)) localDirection += Vector3.forward;
            if (Input.GetKey(leftMoveKey)) localDirection += Vector3.left;
            if (Input.GetKey(backMoveKey)) localDirection += Vector3.back;
            if (Input.GetKey(rightMoveKey)) localDirection += Vector3.right;

            localDirection = localDirection.normalized;

            Vector3 movementAxis = gameObject.transform.TransformDirection(Quaternion.Euler(Vector3.up * 90f) * localDirection);

            //Changes current speed according the player's state.
            float currentSpeed;
            if (Input.GetKey(sprintKey)) currentSpeed = speed * sprintMultiplier;
            else currentSpeed = speed;

            //Moves player towards direction.
            if (angleBasedMovement)
            {
                gameObject.transform.RotateAround(gravitySource.position, movementAxis, currentSpeed * Time.fixedDeltaTime);
            }
            else
            {
                gameObject.transform.RotateAround(gravitySource.position, movementAxis, currentSpeed / (2f * Mathf.PI * Vector3.Distance(gameObject.transform.position, gravitySource.position)) * 360f * Time.fixedDeltaTime);
            }
        }

        {   // Gravitational Force //

            //Pulls player towards gravity source using the current pull.
            rigidBody.AddForce((gravitySource.transform.position - gameObject.transform.position).normalized * Time.fixedDeltaTime * gravity * 100f);
        }

        {   // Gravitational Rotation //

            //Calculates the axis and angle the player needs to rotate in order to align with the gravity source.
            Vector3 targetAxis = Vector3.Cross(gameObject.transform.TransformDirection(Vector3.up),
                (gameObject.transform.position - gravitySource.transform.position).normalized);
            float targetAngle = Vector3.Angle(gameObject.transform.TransformDirection(Vector3.up),
                (gameObject.transform.position - gravitySource.transform.position).normalized);

            //Applies calculated axis and angle to the player's rotation.
            if (gameObject.transform.TransformDirection(Vector3.up) != (gameObject.transform.position - gravitySource.transform.position).normalized)
                gameObject.transform.RotateAround(gameObject.transform.position, targetAxis, targetAngle);
        }


        {   // Grounded //

            //Converts playerLayer to usable layerMask.
            int layerMask;
            if (ignoreLayers.Length > 0)
            {
                layerMask = 1 << ignoreLayers[0];
                foreach (var ignoredLayer in ignoreLayers.Skip(1))
                {
                    layerMask = (layerMask | 1 << ignoredLayer);
                }
            }
            else layerMask = 0;
            layerMask = ~layerMask;

            // Set's localGrounded to false before local ground check.
            bool localGrounded = false;

            // Checks for ground using raycast.
            RaycastHit hit;
            if (Physics.Raycast(gameObject.GetComponent<Collider>().ClosestPoint(gravitySource.transform.position) + (gameObject.transform.position - gravitySource.transform.position).normalized * groundCheckDistance,
                (gravitySource.transform.position - gameObject.transform.position).normalized, out hit, groundCheckDistance * 2f, layerMask))
                localGrounded = true;
            else localGrounded = false;

            if (localGrounded)
            {
                if (Input.GetKey(jumpKey) && jumpPauseTimer <= 0f)
                {
                    // Flattens the velocity along the direction of gravity if the velocity is going towards gravity. (This is so that the jump force doesn't challenge gravity.)
                    if (Vector3.Dot(rigidBody.velocity.normalized, (gameObject.transform.position - gravitySource.transform.position).normalized) < 0f)
                        rigidBody.velocity = Vector3.ProjectOnPlane(rigidBody.velocity, gameObject.transform.position - gravitySource.transform.position).normalized;
                    // Jumps when touching ground and pressing jump key.
                    rigidBody.AddForce((gravitySource.transform.position - gameObject.transform.position).normalized * -100f * jumpForce);
                    // Sets the jump pause timer to the set jump pause value when the player jumps.
                    jumpPauseTimer = jumpPause;
                }
                // Sets main grounded variable to true.
                isGrounded = true;
            }
        }

        if (noHorizontalVelocity)
        {
            // Makes the player's velocity always going in the direction to the gravity source.
            if (Vector3.Angle((gameObject.transform.position - gravitySource.position).normalized, rigidBody.velocity.normalized) > 90f)
                rigidBody.velocity = (gravitySource.position - gameObject.transform.position).normalized * rigidBody.velocity.magnitude;
            else rigidBody.velocity = (gameObject.transform.position - gravitySource.position).normalized * rigidBody.velocity.magnitude;
        }
        // Limits the player's velocity.
        if (rigidBody.velocity.magnitude > maxVelocity) rigidBody.velocity = rigidBody.velocity.normalized * maxVelocity;
    }

    void Update()
    {
        // Makes the jump pause timer count down if greater than 0.
        if (jumpPauseTimer > 0) jumpPauseTimer -= Time.deltaTime;

        // Set rigidbody values every frame. [Runtime]
        rigidBody.mass = mass;
        rigidBody.drag = drag;
        rigidBody.angularDrag = angularDrag;
        rigidBody.useGravity = false;
        rigidBody.isKinematic = isKinematic;
        rigidBody.freezeRotation = true;
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
        rigidBody.freezeRotation = true;
    }
}
