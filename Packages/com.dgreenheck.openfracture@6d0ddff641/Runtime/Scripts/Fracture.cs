using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
public class Fracture : MonoBehaviour
{
    public FractureableSO fractureableSO;
    /// <summary>
    /// The number of times this fragment has been re-fractured.
    /// </summary>
    [HideInInspector]
    public int currentRefractureCount = 0;

    /// <summary>
    /// Collector object that stores the produced fragments
    /// </summary>
    private GameObject fragmentRoot;

    [ContextMenu("Print Mesh Info")]
    public void PrintMeshInfo()
    {
        var mesh = this.GetComponent<MeshFilter>().mesh;
        Debug.Log("Positions");

        var positions = mesh.vertices;
        var normals = mesh.normals;
        var uvs = mesh.uv;

        for (int i = 0; i < positions.Length; i++)
        {
            Debug.Log($"Vertex {i}");
            Debug.Log($"POS | X: {positions[i].x} Y: {positions[i].y} Z: {positions[i].z}");
            Debug.Log($"NRM | X: {normals[i].x} Y: {normals[i].y} Z: {normals[i].z} LEN: {normals[i].magnitude}");
            Debug.Log($"UV  | U: {uvs[i].x} V: {uvs[i].y}");
            Debug.Log("");
        }
    }

    public void CauseFracture()
    {
        fractureableSO.callbackOptions.CallOnFracture(null, gameObject, transform.position);
        this.ComputeFracture();
    }

    public void CauseFracture(Collider instigator, Vector3 collisionPoint)
    {
        fractureableSO.callbackOptions.CallOnFracture(instigator, gameObject, collisionPoint);

        this.ComputeFracture();
    }

    void OnValidate()
    {
        if (this.transform.parent != null)
        {
            // When an object is fractured, the fragments are created as children of that object's parent.
            // Because of this, they inherit the parent transform. If the parent transform is not scaled
            // the same in all axes, the fragments will not be rendered correctly.
            var scale = this.transform.parent.localScale;
            if ((scale.x != scale.y) || (scale.x != scale.z) || (scale.y != scale.z))
            {
                Debug.LogWarning($"Warning: Parent transform of fractured object must be uniformly scaled in all axes or fragments will not render correctly.", this.transform);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (fractureableSO.triggerOptions.triggerType == TriggerType.Collision)
        {
            if (collision.contactCount > 0)
            {
                // Collision force must exceed the minimum force (F = I / T)
                var contact = collision.contacts[0];
                float collisionForce = collision.impulse.magnitude / Time.fixedDeltaTime;

                // Colliding object tag must be in the set of allowed collision tags if filtering by tag is enabled
                bool tagAllowed = fractureableSO.triggerOptions.IsTagAllowed(contact.otherCollider.gameObject.tag);

                // Object is unfrozen if the colliding object has the correct tag (if tag filtering is enabled)
                // and the collision force exceeds the minimum collision force.
                if (collisionForce > fractureableSO.triggerOptions.minimumCollisionForce &&
                   (fractureableSO.triggerOptions.filterCollisionsByTag && tagAllowed))
                {
                    fractureableSO.callbackOptions.CallOnFracture(contact.otherCollider, gameObject, contact.point);
                    this.ComputeFracture();
                }
            }
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (fractureableSO.triggerOptions.triggerType == TriggerType.Trigger)
        {
            Debug.Log("Trigger Entered!");

            // Colliding object tag must be in the set of allowed collision tags if filtering by tag is enabled
            bool tagAllowed = fractureableSO.triggerOptions.IsTagAllowed(collider.gameObject.tag);

            if (fractureableSO.triggerOptions.filterCollisionsByTag && tagAllowed)
            {
                fractureableSO.callbackOptions.CallOnFracture(collider, gameObject, transform.position);
                this.ComputeFracture();
            }
        }
    }

    void Update()
    {
        if (fractureableSO.triggerOptions.triggerType == TriggerType.Keyboard)
        {
            if (Input.GetKeyDown(fractureableSO.triggerOptions.triggerKey))
            {
                fractureableSO.callbackOptions.CallOnFracture(null, gameObject, transform.position);
                this.ComputeFracture();
            }
        }
    }

    /// <summary>
    /// Compute the fracture and create the fragments
    /// </summary>
    /// <returns></returns>
    private void ComputeFracture()
    {
        var mesh = this.GetComponent<MeshFilter>().sharedMesh;

        if (mesh != null)
        {
            // If the fragment root object has not yet been created, create it now
            if (this.fragmentRoot == null)
            {
                // Create a game object to contain the fragments
                this.fragmentRoot = new GameObject($"{this.name}Fragments");
                this.fragmentRoot.transform.SetParent(this.transform.parent);

                // Each fragment will handle its own scale
                this.fragmentRoot.transform.position = this.transform.position;
                this.fragmentRoot.transform.rotation = this.transform.rotation;
                this.fragmentRoot.transform.localScale = Vector3.one;
            }

            var fragmentTemplate = CreateFragmentTemplate();

            if (fractureableSO.fractureOptions.asynchronous)
            {
                StartCoroutine(Fragmenter.FractureAsync(
                    this.gameObject,
                    fractureableSO.fractureOptions,
                    fragmentTemplate,
                    this.fragmentRoot.transform,
                    () =>
                    {
                        // Done with template, destroy it
                        GameObject.Destroy(fragmentTemplate);

                        // Deactivate the original object
                        this.gameObject.SetActive(false);

                        // Fire the completion callback
                        if ((this.currentRefractureCount == 0) ||
                            (this.currentRefractureCount > 0 && fractureableSO.refractureOptions.invokeCallbacks))
                        {
                            if (fractureableSO.callbackOptions.onCompleted != null)
                            {
                                fractureableSO.callbackOptions.onCompleted.Invoke();
                            }
                        }
                    }
                ));
            }
            else
            {
                Fragmenter.Fracture(this.gameObject,
                                    fractureableSO.fractureOptions,
                                    fragmentTemplate,
                                    this.fragmentRoot.transform);

                // Done with template, destroy it
                GameObject.Destroy(fragmentTemplate);

                // Deactivate the original object
                this.gameObject.SetActive(false);

                // Fire the completion callback
                if ((this.currentRefractureCount == 0) ||
                    (this.currentRefractureCount > 0 && fractureableSO.refractureOptions.invokeCallbacks))
                {
                    if (fractureableSO.callbackOptions.onCompleted != null)
                    {
                        fractureableSO.callbackOptions.onCompleted.Invoke();
                    }
                }
            }
        }

        this.fragmentRoot.AddComponent<Rigidbody>();

        this.fragmentRoot.GetComponent<Rigidbody>().AddExplosionForce
            (10000, this.fragmentRoot.transform.position, 5000);

        //Destroys the fragments
        //Destroy(this.fragmentRoot, 20);
    }

    /// <summary>
    /// Creates a template object which each fragment will derive from
    /// </summary>
    /// <param name="preFracture">True if this object is being pre-fractured. This will freeze all of the fragments.</param>
    /// <returns></returns>
    private GameObject CreateFragmentTemplate()
    {
        // If pre-fracturing, make the fragments children of this object so they can easily be unfrozen later.
        // Otherwise, parent to this object's parent
        GameObject obj = new();
        obj.name = "Fragment";
        obj.tag = this.tag;

        // Update mesh to the new sliced mesh
        obj.AddComponent<MeshFilter>();

        // Add materials. Normal material goes in slot 1, cut material in slot 2
        var meshRenderer = obj.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterials = new Material[2] {
            this.GetComponent<MeshRenderer>().sharedMaterial,
            fractureableSO.fractureOptions.insideMaterial
        };

        // Copy collider properties to fragment
        var thisCollider = this.GetComponent<Collider>();
        var fragmentCollider = obj.AddComponent<MeshCollider>();
        fragmentCollider.convex = true;
        fragmentCollider.sharedMaterial = thisCollider.sharedMaterial;
        fragmentCollider.isTrigger = thisCollider.isTrigger;

        // Copy rigid body properties to fragment
        var fragmentRigidBody = obj.AddComponent<Rigidbody>();

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
            Debug.LogError(gameObject + "'s RIGIDBODY IS NULL");

        rb.mass = fractureableSO.rigidbodyMass;

        fragmentRigidBody.velocity = rb.velocity;
        fragmentRigidBody.angularVelocity = rb.angularVelocity;
        fragmentRigidBody.drag = rb.drag;
        fragmentRigidBody.angularDrag = rb.angularDrag;
        fragmentRigidBody.useGravity = rb.useGravity;

        //obj.GetComponent<Rigidbody>().AddExplosionForce(10000000000, this.transform.position, 500000000000);

        // If refracturing is enabled, create a copy of this component and add it to the template fragment object
        if (fractureableSO.refractureOptions.enableRefracturing &&
           (this.currentRefractureCount < fractureableSO.refractureOptions.maxRefractureCount))
        {
            CopyFractureComponent(obj);
        }

        return obj;
    }

    /// <summary>
    /// Convenience method for copying this component to another component
    /// </summary>
    /// <param name="obj">The GameObject to copy the component to</param>
    private void CopyFractureComponent(GameObject obj)
    {
        var fractureComponent = obj.AddComponent<Fracture>();

        fractureComponent.fractureableSO = (FractureableSO) ScriptableObject.CreateInstance("FractureableSO");

        fractureComponent.fractureableSO.triggerOptions = fractureableSO.triggerOptions;
        fractureComponent.fractureableSO.fractureOptions = fractureableSO.fractureOptions;
        fractureComponent.fractureableSO.refractureOptions = fractureableSO.refractureOptions;
        fractureComponent.fractureableSO.callbackOptions = fractureableSO.callbackOptions;
        fractureComponent.currentRefractureCount = this.currentRefractureCount + 1;
        fractureComponent.fragmentRoot = this.fragmentRoot;
    }
}