using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedestrianBehavior : MonoBehaviour
{

    private enum pedestrianMovment {
        Player,
        Wiggle,
        Circle,
        Dedicated,
        Wander
    }

    [SerializeField] private pedestrianMovment movementType;

    private float timer = 0;
    private Vector2 velocity = Vector2.zero;
    private float accelerationMagnitude = 0;
    private Vector2 pivot = Vector2.zero;

    // Start is called before the first frame update
    void Start() {
        int chosen = Random.Range(1, 5);
        Debug.Log(chosen);
        movementType = (pedestrianMovment)chosen;

        switch (movementType) {
            case pedestrianMovment.Wiggle:
                WiggleSetUp();
                break;
            case pedestrianMovment.Circle:
                CircleSetUp();
                break;
            case pedestrianMovment.Dedicated:
                DedicatedSetUp();
                break;
            case pedestrianMovment.Wander:
                WanderSetUp();
                break;
        }
    }

    // Update is called once per frame
    void Update() {
        switch (movementType) {
            case pedestrianMovment.Player:
                HandleInput();
                break;
            case pedestrianMovment.Wiggle:
                Wiggle();
                break;
            case pedestrianMovment.Circle:
                Circle();
                break;
            case pedestrianMovment.Wander:
                Wander();
                break;
        }
        transform.Translate(velocity * Time.deltaTime);
        timer -= Time.deltaTime;
    }

    void HandleInput() {
        
    }

// -----------------------------------------------

    void WiggleSetUp() {
        velocity = Vector2.right * (Random.Range(0, 1) * 2 - 1);
    }

    void Wiggle() {
        if (timer <= 0) {
            velocity *= -1;
            timer = Random.Range(.1f, .2f);
        }
    }

// -----------------------------------------------

    void CircleSetUp() {
        float radius = Random.Range(2.5f, 6f);
        accelerationMagnitude = Random.Range(3, 5f);
        Vector2 dir = Random.insideUnitCircle * radius;
        pivot = (Vector2)transform.position + dir;
        velocity = Mathf.Sqrt(accelerationMagnitude * radius) * new Vector2(dir.y, -dir.x).normalized * (Random.Range(0, 1) * 2 - 1);
    }

    void Circle() {
        Vector2 acc = (pivot - (Vector2)transform.position).normalized * accelerationMagnitude;
        velocity += acc * Time.deltaTime;
    }

// -----------------------------------------------

    void DedicatedSetUp() {
        velocity = Random.insideUnitCircle * Random.Range(2.75f, 4.25f);
    }

// -----------------------------------------------

    void WanderSetUp() {
        velocity = Random.insideUnitCircle * Random.Range(2.75f, 4.25f);
        timer = Random.Range(.75f, 2f);
    }

    void Wander() {
        if (timer <= 0) {
            WanderSetUp();
        }
    }

}
