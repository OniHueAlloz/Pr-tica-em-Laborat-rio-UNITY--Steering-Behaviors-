using UnityEngine.UI;
using UnityEngine;

enum Behavior { Idle, Seek, Evade, Flee, Pursuit, Cohesion, WallAvoid }
enum State { Idle, Arrive, Seek, Evade, Flee, Pursuit, Cohesion, WallAvoid }

[RequireComponent(typeof(Rigidbody2D))]
public class SteeringActor : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] Behavior behavior = Behavior.Seek;
    [SerializeField] Transform target = null;
    [SerializeField] Transform wall = null;
    [SerializeField] float maxSpeed = 4f;
    [SerializeField, Range(0.1f, 0.99f)] float decelerationFactor = 0.75f;
    [SerializeField] float arriveRadius = 1.2f;
    [SerializeField] float stopRadius = 0.5f;
    [SerializeField] float evadeRadius = 5f;
    [SerializeField] float fleeRadius = 1f;
    [SerializeField] float stalkRadius = 2.5f;
    [SerializeField] float avoidRadius = 3.5f;

    Text behaviorDisplay = null;
    Rigidbody2D physics;
    State state = State.Idle;
    bool hasFled = false;

    void FixedUpdate()
    {
        if (target != null)
        {
            switch (behavior)
            {
                case Behavior.Idle: IdleBehavior(); break;
                case Behavior.Seek: SeekBehavior(); break;
                case Behavior.Evade: EvadeBehavior(); break;
                case Behavior.Flee: FleeBehavior(); break;
                case Behavior.Pursuit: PursuitBehavior(); break;
                case Behavior.Cohesion: CohesionBehavior(); break;
                case Behavior.WallAvoid: WallAvoidBehavior(); break;
            }
        }

        physics.velocity = Vector2.ClampMagnitude(physics.velocity, maxSpeed);

        behaviorDisplay.text = state.ToString().ToUpper();
    }

    void Awake()
    {
        physics = GetComponent<Rigidbody2D>();
        physics.isKinematic = true;
        behaviorDisplay = GetComponentInChildren<Text>();
    }

    void IdleBehavior()
    {
        physics.velocity = physics.velocity * decelerationFactor;
    }

    void SeekBehavior()
    {
        Vector2 delta = target.position - transform.position;
        Vector2 steering = delta.normalized * maxSpeed - physics.velocity;
        float distance = delta.magnitude;

        if (distance < stopRadius)
        {
            state = State.Idle;
        }
        else if (distance < arriveRadius)
        {
            state = State.Arrive;
        }
        else
        {
            state = State.Seek;
        }

        switch (state)
        {
            case State.Idle:
                IdleBehavior();
                break;
            case State.Arrive:
                var arriveFactor = 0.01f + (distance - stopRadius) / (arriveRadius - stopRadius);
                physics.velocity += arriveFactor * steering * Time.fixedDeltaTime;
                break;
            case State.Seek:
                physics.velocity += steering * Time.fixedDeltaTime;
                break;
        }
    }

    void EvadeBehavior()
    {
        Vector2 delta = target.position - transform.position;
        Vector2 steering = delta.normalized * maxSpeed - physics.velocity;
        float distance = delta.magnitude;

        if (distance > evadeRadius)
        {
            state = State.Idle;
        }
        else
        {
            state = State.Evade;
        }

        switch (state)
        {
            case State.Idle:
                IdleBehavior();
                break;
            case State.Evade:
                physics.velocity -= steering * Time.fixedDeltaTime;
                break;
        }
    }

    void FleeBehavior()
    {
        Vector2 delta = target.position - transform.position;
        Vector2 steering = delta.normalized * maxSpeed - physics.velocity;
        float distance = delta.magnitude;

        if (!hasFled && distance > fleeRadius)
        {
            state = State.Idle;
        }
        else
        {
            hasFled = true;
            state = State.Flee;
        }

        switch (state)
        {
            case State.Idle:
                IdleBehavior();
                break;
            case State.Flee:
                physics.velocity -= steering * Time.fixedDeltaTime;
                break;
        }

    }

    void PursuitBehavior()
    {
        Vector2 delta = target.position - transform.position;
        Vector2 steering = delta.normalized * maxSpeed - physics.velocity;
        float distance = delta.magnitude;

        if (distance < stopRadius)
        {
            state = State.Idle;

            if (transform.parent != target) 
            {
                transform.SetParent(target);
                physics.velocity = Vector2.zero; 
            }
        }
        else if (distance < arriveRadius)
        {
            state = State.Arrive;
        }
        else
        {
            state = State.Pursuit;
        }

        switch (state)
        {
            case State.Idle:
                IdleBehavior();
                break;
            case State.Arrive:
                var arriveFactor = 0.01f + (distance - stopRadius) / (arriveRadius - stopRadius);
                physics.velocity += arriveFactor * steering * Time.fixedDeltaTime;
                break;
            case State.Pursuit:
                physics.velocity += steering * Time.fixedDeltaTime;
                break;
        }
    }

    void CohesionBehavior()
    {
        Vector2 delta = target.position - transform.position;
        Vector2 steering = delta.normalized * maxSpeed - physics.velocity;
        float distance = delta.magnitude;

        if (distance < stalkRadius)
        {
            state = State.Idle;
        }
        else if (distance < (stalkRadius+arriveRadius))
        {
            state = State.Arrive;
        }
        else
        {
            state = State.Cohesion;
        }

        switch (state)
        {
            case State.Idle:
                IdleBehavior();
                break;
            case State.Arrive:
                var arriveFactor = 0.01f + (distance - stalkRadius) / (arriveRadius);
                physics.velocity += arriveFactor * steering * Time.fixedDeltaTime;
                break;
            case State.Cohesion:
                physics.velocity += steering * Time.fixedDeltaTime;
                break;
        }
    }

    void WallAvoidBehavior()
    {
        Collider2D wallCollider = wall.GetComponent<Collider2D>();
        Vector2 closestPoint = wallCollider.ClosestPoint(transform.position);
        Vector2 delta = closestPoint - (Vector2)transform.position;
        Vector2 steering = delta.normalized * maxSpeed - physics.velocity;
        float distance = delta.magnitude;

        if(distance > avoidRadius)
        {
            state = State.Idle;
        }
        else
        {
            state = State.WallAvoid;
        }

        switch(state)
        {
            case State.Idle:
                IdleBehavior();
                break;
            case State.WallAvoid:
                physics.velocity -= steering * Time.fixedDeltaTime;
                break;
        }
    }

    void OnDrawGizmos()
    {
        if (target == null && wall == null)
        {
            return;
        }

        Collider2D wallCollider = wall.GetComponent<Collider2D>();
        Vector2 closestPoint = wallCollider.ClosestPoint(transform.position);

        Gizmos.color = Color.Lerp(Color.red, Color.yellow, 0.35f);
        Gizmos.DrawSphere(closestPoint, 0.1f);

        switch (behavior)
        {
            case Behavior.Idle:
                break;
            case Behavior.Seek:
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(transform.position, arriveRadius);
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, stopRadius);
                break;
            case Behavior.Evade:
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, evadeRadius);
                break;
            case Behavior.Flee:
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, fleeRadius);
                break;
            case Behavior.Pursuit:
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(transform.position, arriveRadius);
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, stopRadius);
                break;
            case Behavior.Cohesion:
                Gizmos.color = Color.Lerp(Color.red, Color.blue, 0.5f);
                Gizmos.DrawWireSphere(transform.position, stalkRadius);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, (stalkRadius + arriveRadius));
                break;
            case Behavior.WallAvoid:
                Gizmos.color = Color.black;
                Gizmos.DrawWireSphere(transform.position, avoidRadius);
                break;
        }

        Gizmos.color = Color.gray;
        Gizmos.DrawLine(transform.position, target.position);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wall"))
        {
            if (wall == null)
            {
                wall = other.transform;
            }
            
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Wall"))
        {
            if (wall == other.transform)
            {
                wall = null;
            }
        }
    }
}