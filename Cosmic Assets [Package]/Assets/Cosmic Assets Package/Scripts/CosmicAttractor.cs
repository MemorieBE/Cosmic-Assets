using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

/*
 *  \brief A script that controls how multiple planet get pulled together via their gravitational forces.
 */
public class CosmicAttractor : MonoBehaviour
{
    [Header("Math")]
    public bool lockDensity = false; //!< A boolean that locks the density and automatically changes the rigidbody mass and the transform scale.
    public float density = 1f; //!< The current density depending on the rigidbody mass and the transform scale.
    private float densityValueCheck = 0f; //!< The value of the density in the last frame to update the density.
    private float massValueCheck = 0f; //!< The value of the rigidbody mass in the last frame to update the mass.
    private float volumeValueCheck = 0f; //!< The value of the transform scale in the last frame to update the volume.
    public float metersPerUnit = 1f; //!< The amount of meters per unity in world space.
    const float G = 6.67408f; //!< The gravitational constant.
    public float gMultiplier = 10f; //!< The value that multiplie the gravitational constant among all cosmic attractors in the scene.
    public bool validateAll = false; //!< A boolean that updates and validates all changes in the editor.

    [Header("Interaction Based Setting")]
    public Collider[] planetColliders; //!< An array of all colliders used by the planet.
    public bool interactionBasedSetting = false; //!< A boolean that controls whether or not the planet is using these interaction based settings.
    public string interactionBasedInstantiatedObjectNames = "InteractionBasedInstantiatedObject"; //!< The name of the instantiated interaction based objects.
    private bool checkIBSValueChanged; //!< A boolean that determines whether or not the interaction based setting boolean has been changed.

    [HideInInspector] public GameObject interactionBasedInstantiatedObjectsParent; //!< The interaction based instantiated objects' parent.
    [HideInInspector] public GameObject[] interactionBasedInstantiatedObject = new GameObject[0]; //!< An interaction based instantiated object.

    public static List<CosmicAttractor> cosmicAttractors; //!< A list of all other cosmic attractors in the current scene.

    private Rigidbody rigidBody; //!< The game object rigidbody.

    void OnValidate()
    {

        rigidBody = gameObject.GetComponent<Rigidbody>();

        {
            float averageVolume = 
                (gameObject.transform.lossyScale.x + gameObject.transform.lossyScale.y + gameObject.transform.lossyScale.z) / 3f;

            if (densityValueCheck != density)
            {
                if (densityValueCheck == 0f) densityValueCheck = density;

                rigidBody.mass = density * (averageVolume * metersPerUnit);

                densityValueCheck = density;
                massValueCheck = rigidBody.mass;
                volumeValueCheck = (averageVolume * metersPerUnit);
            }

            if (massValueCheck != rigidBody.mass || volumeValueCheck != (averageVolume * metersPerUnit) || validateAll)
            {
                if (massValueCheck == 0f) massValueCheck = rigidBody.mass;
                if (volumeValueCheck == 0f) volumeValueCheck = (averageVolume * metersPerUnit);

                if (lockDensity)
                {
                    if (massValueCheck != rigidBody.mass)
                    {
                        gameObject.transform.localScale *= (rigidBody.mass / massValueCheck);

                        densityValueCheck = density;
                        massValueCheck = rigidBody.mass;
                        volumeValueCheck = (averageVolume * metersPerUnit);
                    }
                    if (volumeValueCheck != (averageVolume * metersPerUnit))
                    {
                        rigidBody.mass = density * (averageVolume * metersPerUnit);

                        densityValueCheck = density;
                        massValueCheck = rigidBody.mass;
                        volumeValueCheck = (averageVolume * metersPerUnit);
                    }
                }
                else
                {
                    density = rigidBody.mass / (averageVolume * metersPerUnit);

                    densityValueCheck = density;
                    massValueCheck = rigidBody.mass;
                    volumeValueCheck = (averageVolume * metersPerUnit);
                }
            }

            validateAll = false;
        }
    }

    void Awake()
    {
        rigidBody = gameObject.GetComponent<Rigidbody>();

        checkIBSValueChanged = interactionBasedSetting;
    }

    void Start()
    {
        InteractionBasedSettingValueChanged();
    }

    void FixedUpdate()
    {
        foreach (CosmicAttractor attractor in cosmicAttractors)
        {
            if (attractor != this) Attract(attractor);
        }

        if (checkIBSValueChanged != interactionBasedSetting)
        {
            InteractionBasedSettingValueChanged();
            checkIBSValueChanged = interactionBasedSetting;
        }

        {
            float averageVolume =
                (gameObject.transform.lossyScale.x + gameObject.transform.lossyScale.y + gameObject.transform.lossyScale.z) / 3f;

            if (densityValueCheck != density)
            {
                if (densityValueCheck == 0f) densityValueCheck = density;

                rigidBody.mass = density * (averageVolume * metersPerUnit);

                densityValueCheck = density;
                massValueCheck = rigidBody.mass;
                volumeValueCheck = (averageVolume * metersPerUnit);
            }

            if (massValueCheck != rigidBody.mass || volumeValueCheck != (averageVolume * metersPerUnit))
            {
                if (massValueCheck == 0f) massValueCheck = rigidBody.mass;
                if (volumeValueCheck == 0f) volumeValueCheck = (averageVolume * metersPerUnit);

                if (lockDensity)
                {
                    if (massValueCheck != rigidBody.mass)
                    {
                        gameObject.transform.localScale *= (rigidBody.mass / massValueCheck);

                        densityValueCheck = density;
                        massValueCheck = rigidBody.mass;
                        volumeValueCheck = (averageVolume * metersPerUnit);
                    }
                    if (volumeValueCheck != (averageVolume * metersPerUnit))
                    {
                        rigidBody.mass = density * (averageVolume * metersPerUnit);

                        densityValueCheck = density;
                        massValueCheck = rigidBody.mass;
                        volumeValueCheck = (averageVolume * metersPerUnit);
                    }
                }
                else
                {
                    density = rigidBody.mass / (averageVolume * metersPerUnit);

                    densityValueCheck = density;
                    massValueCheck = rigidBody.mass;
                    volumeValueCheck = (averageVolume * metersPerUnit);
                }
            }
        }

        rigidBody.useGravity = false;
    }

    void OnEnable()
    {
        if (cosmicAttractors == null) cosmicAttractors = new List<CosmicAttractor>();

        cosmicAttractors.Add(this);
    }

    void OnDisable()
    {
        cosmicAttractors.Remove(this);
    }

    /*!
     *  A method that attracts objects towards this game object.
     *  
     *  \param The object to attract.
     */
    private void Attract (CosmicAttractor objToAttract)
    {
        Vector3 direction = rigidBody.position - objToAttract.rigidBody.position;
        float distance = direction.magnitude * metersPerUnit;

        float forceMagnitude = G * gMultiplier * (rigidBody.mass * objToAttract.rigidBody.mass) / Mathf.Pow(distance, 2);
        Vector3 force = direction.normalized * forceMagnitude;

        objToAttract.rigidBody.AddForce(force);
    }

    /*!
     *  A method that updates the interaction based setting.
     */
    private void InteractionBasedSettingValueChanged()
    {
        if (!interactionBasedSetting)
        {
            InteractionBasedDestroyObjects();
        }
        else
        {
            if (gameObject.transform.parent == null) InteractionBasedInstantiateObjects();
            else if (gameObject.transform.parent.name != interactionBasedInstantiatedObjectNames + " Empty") InteractionBasedInstantiateObjects();
            else return;
        }

        {
            PlanetBasedRigidbody[] planetBasedRigidbodies = FindObjectsOfType<PlanetBasedRigidbody>();
            foreach (PlanetBasedRigidbody planetBasedRigidbody in planetBasedRigidbodies)
            {
                if (gameObject.GetComponent<Collider>() != null)
                {
                    Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), planetBasedRigidbody.gameObject.GetComponent<Collider>(), interactionBasedSetting);
                }

                for (int i = 0; i < planetColliders.Length; i++)
                {
                    Physics.IgnoreCollision(planetColliders[i], planetBasedRigidbody.gameObject.GetComponent<Collider>(), interactionBasedSetting);
                }
            }

            GravityBasedRigidbody[] gravityBasedRigidbodies = FindObjectsOfType<GravityBasedRigidbody>();
            foreach (GravityBasedRigidbody gravityBasedRigidbody in gravityBasedRigidbodies)
            {
                if (gameObject.GetComponent<Collider>() != null)
                {
                    Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), gravityBasedRigidbody.gameObject.GetComponent<Collider>(), interactionBasedSetting);
                }

                for (int i = 0; i < planetColliders.Length; i++)
                {
                    Physics.IgnoreCollision(planetColliders[i], gravityBasedRigidbody.gameObject.GetComponent<Collider>(), interactionBasedSetting);
                }
            }

            GravityBasedPlayerController[] gravityBasedFirstPersonControllers = FindObjectsOfType<GravityBasedPlayerController>();
            foreach (GravityBasedPlayerController gravityBasedFirstPersonController in gravityBasedFirstPersonControllers)
            {
                if (gameObject.GetComponent<Collider>() != null)
                {
                    Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), gravityBasedFirstPersonController.gameObject.GetComponent<Collider>(), interactionBasedSetting);
                }

                for (int i = 0; i < planetColliders.Length; i++)
                {
                    Physics.IgnoreCollision(planetColliders[i], gravityBasedFirstPersonController.gameObject.GetComponent<Collider>(), interactionBasedSetting);
                }
            }

            PlanetBasedPlayerController[] planetBasedPlayerControllers = FindObjectsOfType<PlanetBasedPlayerController>();
            foreach (PlanetBasedPlayerController planetBasedPlayerController in planetBasedPlayerControllers)
            {
                if (gameObject.GetComponent<Collider>() != null)
                {
                    Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), planetBasedPlayerController.gameObject.GetComponent<Collider>(), interactionBasedSetting);
                }

                for (int i = 0; i < planetColliders.Length; i++)
                {
                    Physics.IgnoreCollision(planetColliders[i], planetBasedPlayerController.gameObject.GetComponent<Collider>(), interactionBasedSetting);
                }
            }
        }
    }

    /*!
     *  A method that instantiates interaction based objects.
     */
    private void InteractionBasedInstantiateObjects()
    {
        interactionBasedInstantiatedObjectsParent = new GameObject(interactionBasedInstantiatedObjectNames + " Empty");
        interactionBasedInstantiatedObjectsParent.transform.parent = gameObject.transform;
        interactionBasedInstantiatedObjectsParent.transform.position = gameObject.transform.position;
        interactionBasedInstantiatedObjectsParent.transform.rotation = gameObject.transform.rotation;
        interactionBasedInstantiatedObjectsParent.transform.localScale = Vector3.one;

        interactionBasedInstantiatedObject = new GameObject[planetColliders.Length];

        for (int i = 0; i < planetColliders.Length; i++)
        {
            interactionBasedInstantiatedObject[i] = Instantiate(planetColliders[i].gameObject, planetColliders[i].gameObject.transform.position, planetColliders[i].gameObject.transform.rotation, interactionBasedInstantiatedObjectsParent.transform);
            interactionBasedInstantiatedObject[i].name = (interactionBasedInstantiatedObjectNames + " (" + i + ")");

            Vector3 lossyScaleToLocalScale = new Vector3((interactionBasedInstantiatedObjectsParent.transform.lossyScale.x / interactionBasedInstantiatedObjectsParent.transform.localScale.x), 
                (interactionBasedInstantiatedObjectsParent.transform.lossyScale.y / interactionBasedInstantiatedObjectsParent.transform.localScale.y), 
                (interactionBasedInstantiatedObjectsParent.transform.lossyScale.z / interactionBasedInstantiatedObjectsParent.transform.localScale.z));

            interactionBasedInstantiatedObject[i].transform.localScale = new Vector3((planetColliders[i].gameObject.transform.lossyScale.x / lossyScaleToLocalScale.x), 
                (planetColliders[i].gameObject.transform.lossyScale.y / lossyScaleToLocalScale.y),
                (planetColliders[i].gameObject.transform.lossyScale.z / lossyScaleToLocalScale.z));

            foreach (Transform child in interactionBasedInstantiatedObject[i].transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            foreach (var component in interactionBasedInstantiatedObject[i].GetComponents<Component>())
            {
                if (!(component is Transform) && !(component is MeshRenderer) && !(component is Collider))
                {
                    Destroy(component);
                }
            }

            interactionBasedInstantiatedObject[i].GetComponent<MeshRenderer>().enabled = false;
            interactionBasedInstantiatedObject[i].GetComponent<Collider>().enabled = true;
            if (gameObject.GetComponent<Collider>() != null)
            {
                Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), interactionBasedInstantiatedObject[i].GetComponent<Collider>(), true);
            }
        }
    }

    /*!
     *  A method that destroys interaction based objects.
     */
    private void InteractionBasedDestroyObjects()
    {
        for (int i = 0; i < interactionBasedInstantiatedObject.Length; i++)
        {
            Destroy(interactionBasedInstantiatedObject[i]);
        }

        if (interactionBasedInstantiatedObjectsParent == null) return;

        for (int i = 0; i < interactionBasedInstantiatedObjectsParent.transform.childCount; i++)
        {
            GameObject objectChild = interactionBasedInstantiatedObjectsParent.transform.GetChild(i).gameObject;
            objectChild.transform.parent.SetParent(null);
        }

        Destroy(interactionBasedInstantiatedObjectsParent);
    }
}
