using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour {
    private CharacterController controller;
    public Transform groundCheck;
    private Vector3 move;
    private Vector3 velocity;
    public bool isGrounded;

    public float speed = 6f;
    public float jumpSpeed = 5f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public float gravity = -2f;
    public float groundDistance = .1f;
    public LayerMask groundMask;

    public bool flying = true;

    void Start() {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        move = transform.right * x + transform.forward * z;

        if (Input.GetKeyDown(KeyCode.Q)) {
            flying = !flying;
        }

        if (!flying) {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            if (isGrounded && velocity.y < 0)
                velocity.y = -2f;

            if (Input.GetButtonDown("Jump") && isGrounded) {
                velocity.y = jumpSpeed;
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        } else {
            if (Input.GetButtonDown("Jump")) {
                move += Vector3.up;
            } else if (Input.GetButtonDown("Fire3")) {
                move += Vector3.down;
            }
        }

        controller.Move(move * speed * Time.deltaTime);
    }
}
