using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//abstract allows the class to be incomplete and must be implemented in some other class. 
public abstract class MovingObject : MonoBehaviour {

    public float moveTime = 0.1f;
    public LayerMask blockingLayer;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2D;
    private float inverseMoveTime; // Will be used to make movementcalculations more efficient.


    // Use this for initialization
    // protected virtual methods can be overwritten by their inheriting classes.
    protected virtual void Start () {
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        inverseMoveTime = 1f / moveTime;


	}

    // out means the method can alter the incoming attribute inside itself. 
    // Can only return one thing, so here used to also return the RayCast.
    // out passes a reference, pointer, so that we can modify the object.   
    protected bool Move (int xDir, int yDir, out RaycastHit2D hit)
    {
        Vector2 start = transform.position;
        Vector2 end = start + new Vector2(xDir, yDir);

        boxCollider.enabled = false;
        hit = Physics2D.Linecast(start, end, blockingLayer);
        boxCollider.enabled = true;

        if (hit.transform == null)
        {
            StartCoroutine(SmoothMovement(end));
            return true; // Move was successful.
        }

        return false; // Move failed. 

    }

	// IEnumerator is used to create the coroutine SmoothMovement
    // Coroutines can be used to continuously update something, but not clogging every frame with it
    // Enumerator ~~ Iterator, basically used for iteration. 
    protected IEnumerator SmoothMovement(Vector3 end) // Protected -> only visible in derived classes.
    {
        // Using sqrMagnitude for efficiency over ."Distance".
    
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

        while (sqrRemainingDistance > float.Epsilon) // float.epsilon = smallest value a float can assume.
        {
            // MoveTowards moves a position in a straight line towards its second argument. 
            Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime*Time.deltaTime);
            rb2D.MovePosition(newPosition);
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            // yield return means we will return to this location in the code the next time the iterator is called
            // In this case waiting for a new frame.
            yield return null;
        }
    }

    protected virtual void AttemptMove<T>(int xDir, int yDir)
        where T : Component
    {
        RaycastHit2D hit;
        bool canMove = Move(xDir, yDir, out hit);

        if (hit.transform == null)
            return;
        T hitComponent = hit.transform.GetComponent<T>();

        if (!canMove && hitComponent != null)
        {
            OnCantMove(hitComponent);
        }
    }

    // Abstract will be overwritten in derived classes. 
    protected abstract void OnCantMove<T>(T component)
        where T : Component;
}
