using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]

/*
 *  \brief A script that controls how the player moves based on the gravity state and the player inputs.
 */
public class GravityBasedPlayerController : MonoBehaviour
{
    [Header("Player")]
    public GameObject playerCamera; //!< The object used as the player camera.

    [Header("Layer")]
    public int[] ignoreLayers; //!< The layers that the ground check raycast will ignore.

    [Header("Movement Speed")]
    public float speed = 8f; //!< The overall speed of the player.
    public float sprintMultiplier = 1.5f; //!< The number multiplied by the speed to calculate the player's sprint speed.
    [Range(0f, 1f)] public float airSpeedMultiplier = 0.5f; //!< The number multiplied by the speed when the player is not grounded.
    private float currentAirMultiplier = 1f; //!< The current number multiplied by the player's speed in air time.
    public float airSpeedTransition = 1f; //!< The speed at which the player's movement speed will transition to air speed.
    public bool gravityLock = true; //!< A boolean that controls whether or not the player will move along the strongest gravity source's circumference.
    [HideInInspector] public bool gravityLockToGround = false; //!< An extention boolean from the gravity lock boolean that controls whether or not the player will lock onto the floor only.
    private bool gravityLockState = false; //!< A boolean that determines whether or not the gravity lock mechanic is currently active.

    [Header("Movement Jump")]
    public float jumpForce = 6f; //!< The overall jump force of the player.
    public float jumpPause = 0.5f; //!< The time in seconds between possible jumps.
    private float jumpPauseTimer = 0f; //!< The timer used to determine how much time is left to be able to jump again.

    [HideInInspector] public bool isGrounded = false; //!< A boolean that determines whether or not the player is currently touching the ground. [Read Only]
    private float groundCheckDistance = 0.1f; //!< The distance from the base of the player to check the ground. (Lower number means that the player needs to be closer to the ground in order for the ground check to be true.)

    [Header("Cosmic Settings")]
    public float gravity = 3f; //!< Gravity multiplier for the gravity source.
    public float gravityRotationSpeed = 5f; //!< The speed at which the player will rotate so that the local upwards direction is pointing away from the average gravity positions.
    [HideInInspector] public bool gravitySmoothRotation = true; //!< A boolean that controls whether or not the player will rotate in a smooth or linear way.
    public float gravitySmoothDampen = 5f; //!< The width of the smooth rotation parabola's latus rectum. (Higher number will ease out more abruptly.)
    private float newRotationSpeed; //!< The evaluated rotation speed used to rotate the player.

    [Header("Rigidbody Settings")]
    public float mass = 1f; //!< The rigidbody mass.
    public float drag = 0f; //!< The rigidbody drag.
    public float angularDrag = 25f; //!< The rigidbody angular drag.
    public bool useGravity = true; //!< A boolean that controls the rigidbody gravity state.
    public bool isKinematic = false; //!< A boolean that controls the rigidbody kinematic state.

    public float maxVelocity = 50f; //!< The max velocity the player can go. (Used to prevent the player from clipping into the ground.)

    public static string forwardMoveKey = "w"; //!< Key used to move forward.
    public static string leftMoveKey = "a"; //!< Key used to move left.
    public static string backMoveKey = "s"; //!< Key used to move back.
    public static string rightMoveKey = "d"; //!< Key used to move right.
    public static string jumpKey = "space"; //!< Key used to jump.
    public static string sprintKey = "left shift"; //!< Key used to sprint.

    private Rigidbody rigidBody; //!< References the player's rigidbody component.

    private List<Vector3> gravityPoint = new List<Vector3>(); //!< A list of points for each gravity source the player is within.
    private List<float> gravityStrength = new List<float>(); //!< A list of gravities from each gravity source the player is within.
    private Vector3 strongestGravityPoint; //!< The position of the gravity source with the strongest current gravitational pull.

    void Awake()
    {
        // Sets the rigidbody variable to the player's rigidbody component.
        rigidBody = gameObject.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (gravityPoint.Count > 0 && gravityPoint.Count == gravityStrength.Count)
        {
            Vector3 averageGravityPoint = Vector3.zero;

            float highestStrength = 0f;

            for (int i = 0; i < gravityPoint.Count; i++)
            {
                averageGravityPoint += gameObject.transform.position + ((gravityPoint[i] - gameObject.transform.position).normalized * gravityStrength[i]);

                if (gravityStrength[i] > highestStrength)
                {
                    highestStrength = gravityStrength[i];
                    strongestGravityPoint = gravityPoint[i];

                    gravityLockState = true;
                }
                else if (gravityStrength[i] == highestStrength)
                {
                    gravityLockState = false;
                }
            }

            averageGravityPoint /= gravityPoint.Count;

            {   // Gravitational Rotation //

                // Calculates the axis and angle the player needs to rotate in order to align with the gravity source.
                Vector3 targetAxis = Vector3.Cross(gameObject.transform.TransformDirection(Vector3.up),
                    (gameObject.transform.position - averageGravityPoint).normalized);
                float targetAngle = Vector3.Angle(gameObject.transform.TransformDirection(Vector3.up),
                    (gameObject.transform.position - averageGravityPoint).normalized);

                // Applies calculated axis and angle to the player's rotation.
                if (gameObject.transform.TransformDirection(Vector3.up) != (gameObject.transform.position - averageGravityPoint).normalized)
                {
                    if (gravitySmoothRotation)
                    {
                        newRotationSpeed = gravityRotationSpeed * ((-1f * gravitySmoothDampen) * Mathf.Pow((180f - targetAngle) / 180f, 2f) + gravitySmoothDampen);
                    }
                    else
                    {
                        newRotationSpeed = gravityRotationSpeed;
                    }

                    if (targetAngle > Vector3.Distance(gameObject.transform.position, averageGravityPoint) * Time.fixedDeltaTime * 0.1f * newRotationSpeed)
                    {
                        gameObject.transform.RotateAround(gameObject.transform.position, targetAxis, Vector3.Distance(gameObject.transform.position, averageGravityPoint) * Time.fixedDeltaTime * 0.1f * newRotationSpeed);
                    }
                    else
                    {
                        gameObject.transform.RotateAround(gameObject.transform.position, targetAxis, targetAngle);
                    }
                }
            }

            gravityPoint.Clear();
            gravityStrength.Clear();
        }
        else
        {
            gravityLockState = false;
        }

        if (!gravityLock)
        {
            gravityLockState = false;
        }
        else if (gravityLockToGround && isGrounded)
        {
            gravityLockState = true;
        }

        {   // Player Movement //

            // Gets local direction of player movement.
            Vector3 localDirection = Vector3.zero;
            if (Input.GetKey(forwardMoveKey)) localDirection += Vector3.forward;
            if (Input.GetKey(leftMoveKey)) localDirection += Vector3.left;
            if (Input.GetKey(backMoveKey)) localDirection += Vector3.back;
            if (Input.GetKey(rightMoveKey)) localDirection += Vector3.right;

            localDirection = localDirection.normalized;

            // Changes current speed according the player's state.
            float currentSpeed;
            if (Input.GetKey(sprintKey)) currentSpeed = speed * sprintMultiplier;
            else currentSpeed = speed;
            if (!isGrounded)
            {
                if (airSpeedMultiplier < currentAirMultiplier - airSpeedTransition * Time.fixedDeltaTime) currentAirMultiplier -= airSpeedTransition * Time.fixedDeltaTime;
                else currentAirMultiplier = airSpeedMultiplier;
            }
            else currentAirMultiplier = 1f;
            currentSpeed *= currentAirMultiplier;

            if (gravityLockState)
            {
                Vector3 movementAxis = gameObject.transform.TransformDirection(Quaternion.Euler(Vector3.up * 90f) * localDirection);

                gameObject.transform.RotateAround(strongestGravityPoint, movementAxis, currentSpeed / (2f * Mathf.PI * Vector3.Distance(gameObject.transform.position, strongestGravityPoint)) * 360f * Time.fixedDeltaTime);
            }
            else
            {
                Vector3 movement = gameObject.transform.TransformDirection(localDirection) * currentSpeed;

                // Moves player towards direction.
                gameObject.transform.position += movement * Time.fixedDeltaTime;
            }
        }

        // Limits the player's velocity.
        if (rigidBody.velocity.magnitude > maxVelocity) rigidBody.velocity = rigidBody.velocity.normalized * maxVelocity;

        // Sets global isGrounded variable to false before ground check.
        isGrounded = false;
    }

    void OnTriggerStay(Collider collisionData)
    {
        // Checks if the collision data's game object has a GravitySource component.
        if (collisionData.gameObject.GetComponent<GravitySource>() != null)
        {
            // Creates a variable used to get the current gravity towards the collision data.
            float currentPull = 0f;

            {   // Gravitational Force //

                // Converts the collision data's GravitySource components into a variable.
                GravitySource gravitySource = collisionData.gameObject.GetComponent<GravitySource>();
                //Sets the cosmicRBDetected variable to true.
                gravitySource.cosmicRBDetected = true;

                // Checks if the gravity source is using gradual gravity.
                if (gravitySource.gradualGravity)
                {
                    // Creates variable used to calculate currentPull.
                    float gravityMaxRadius = gravitySource.range * Mathf.Max(collisionData.transform.lossyScale.x, collisionData.transform.lossyScale.y, collisionData.transform.lossyScale.z);
                    float gravityMinRadius = gravityMaxRadius * gravitySource.rangeTerminalCutoff;
                    float gravityLength = gravityMaxRadius - gravityMinRadius;

                    // Calculates currentPull.
                    currentPull = (gravityLength - (Vector3.Distance(gameObject.transform.position, collisionData.transform.position) - gravityMinRadius)) / gravityLength * 2f;
                    if (currentPull >= 2f) currentPull = 2f;
                    currentPull *= gravitySource.gravitationalForce;
                }
                else
                {
                    // Sets gravitationalForce to currentPull.
                    currentPull = gravitySource.gravitationalForce;
                }

                // Pulls player towards gravity source using the current pull.
                rigidBody.AddForce((collisionData.transform.position - gameObject.transform.position).normalized * Time.deltaTime * gravity * currentPull);
            }

            {
                gravityPoint.Add(collisionData.transform.position);
                gravityStrength.Add(currentPull);
            }


            {   // Grounded //

                // Converts playerLayer to usable layerMask.
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
                if (Physics.Raycast(gameObject.GetComponent<Collider>().ClosestPoint(collisionData.gameObject.transform.position) + (gameObject.transform.position - collisionData.gameObject.transform.position).normalized * groundCheckDistance,
                    (collisionData.gameObject.transform.position - gameObject.transform.position).normalized, out hit, groundCheckDistance * 2f, layerMask))
                    localGrounded = true;
                else localGrounded = false;

                if (localGrounded)
                {
                    if (Input.GetKey(jumpKey) && jumpPauseTimer <= 0f)
                    {
                        // Flattens the velocity along the direction of gravity if the velocity is going towards gravity. (This is so that the jump force doesn't challenge gravity.)
                        if (Vector3.Dot(rigidBody.velocity.normalized, (gameObject.transform.position - collisionData.gameObject.transform.position).normalized) < 0f)
                            rigidBody.velocity = Vector3.ProjectOnPlane(rigidBody.velocity, gameObject.transform.position - collisionData.gameObject.transform.position).normalized;
                        // Jumps when touching ground and pressing jump key.
                        rigidBody.AddForce((collisionData.gameObject.transform.position - gameObject.transform.position).normalized * -100f * jumpForce);
                        // Sets the jump pause timer to the set jump pause value when the player jumps.
                        jumpPauseTimer = jumpPause;
                    }
                    // Sets main grounded variable to true.
                    isGrounded = true;
                }
            }
        }
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