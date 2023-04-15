using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Rigidbody rigidbodyComponent;

    private Vector2 move;

    private bool isGrounded;
    private bool canDoubleJump;

    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpPower = 10f;
    [SerializeField] private float doubleJumpPower = 5f;

    private void Start()
    {
        rigidbodyComponent = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        rigidbodyComponent.velocity = transform.TransformDirection(
            move.x * moveSpeed,
            rigidbodyComponent.velocity.y,
            move.y * moveSpeed
        );

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rigidbodyComponent.AddForce(
                new Vector3(
                    0,
                    jumpPower,
                    0
                ),
                ForceMode.Impulse
            );

            isGrounded = false;
            canDoubleJump = true;
        } else {
            if (Input.GetButtonDown("Jump") && !isGrounded && canDoubleJump)
            {
                rigidbodyComponent.AddForce(
                    new Vector3(
                        0,
                        doubleJumpPower,
                        0
                    ),
                    ForceMode.Impulse
                );

                canDoubleJump = false;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            isGrounded = true;
        }
    }

    private void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
    }
}
