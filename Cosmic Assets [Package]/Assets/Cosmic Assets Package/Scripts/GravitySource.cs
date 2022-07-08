using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]

/*
 *  \brief A script that controls the gravity source of a transform.
 */
public class GravitySource : MonoBehaviour
{
    [Header("Gravity")]
    public bool gradualGravity = true; //!< A boolean that controls whether or not the gravity will be gradual or linear.
    public float gravitationalForce = 100f; //!< The local gravitational force.
    public float range = 1.5f; //!< The range of the local gravitational force.sxz
    [Range(0f, 1f)] public float rangeTerminalCutoff = 0.5f; //!< The value that determines where the gravity source will reach terminal velocity in gradual gravity.

    [Header("Debug")]
    public bool visualDebug = true; //!< A boolean that controls whether or not the debugs are visible.

    [HideInInspector] public bool cosmicRBDetected; //!< A boolean that determines whether or not a cosmic rigidbody / player controller has entered the gravity source radius.

    void FixedUpdate()
    {
        cosmicRBDetected = false;

        gameObject.transform.rotation = Quaternion.Euler(Vector3.zero);
    }

    void OnDrawGizmosSelected()
    {
        if (visualDebug)
        {
            SphereCollider gravityTrigger = gameObject.GetComponent<SphereCollider>();

            if (gradualGravity)
            {
                Gizmos.color = Color.green; //Max Range Visual.
                Gizmos.DrawWireSphere(gameObject.transform.position + gravityTrigger.center,
                    range * Mathf.Max(Mathf.Abs(gameObject.transform.lossyScale.x), Mathf.Abs(gameObject.transform.lossyScale.y), Mathf.Abs(gameObject.transform.lossyScale.z)));

                Gizmos.color = Color.yellow; //Mid Range Visual.
                Gizmos.DrawWireSphere(gameObject.transform.position + gravityTrigger.center,
                    (range * Mathf.Max(Mathf.Abs(gameObject.transform.lossyScale.x), Mathf.Abs(gameObject.transform.lossyScale.y), Mathf.Abs(gameObject.transform.lossyScale.z))) * ((rangeTerminalCutoff + 1f) / 2f));

                Gizmos.color = Color.red; //Min Range Visual.
                Gizmos.DrawWireSphere(gameObject.transform.position + gravityTrigger.center,
                    (range * Mathf.Max(Mathf.Abs(gameObject.transform.lossyScale.x), Mathf.Abs(gameObject.transform.lossyScale.y), Mathf.Abs(gameObject.transform.lossyScale.z))) * rangeTerminalCutoff);
            }
            else
            {
                if (cosmicRBDetected) Gizmos.color = Color.green;
                else Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(gameObject.transform.position + gravityTrigger.center,
                    range * Mathf.Max(Mathf.Abs(gameObject.transform.lossyScale.x), Mathf.Abs(gameObject.transform.lossyScale.y), Mathf.Abs(gameObject.transform.lossyScale.z)));
            }
        }
    }
    void OnValidate()
    {
        SphereCollider gravityTrigger = gameObject.GetComponent<SphereCollider>();

        gravityTrigger.isTrigger = true;
        gravityTrigger.radius = range;
        gravityTrigger.enabled = true;
    }
}
