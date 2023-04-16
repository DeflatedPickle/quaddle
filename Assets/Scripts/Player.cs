using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Rigidbody rigidbodyComponent;

    [Header("Component")]
    [SerializeField] private SpriteRenderer sprite;

    [SerializeField] private ParticleSystem jumpParticles;
    [SerializeField] private ParticleSystem thrustParticles;

    private Vector2 move;

    private bool isGrounded;
    private int doubleJumpSupply;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpPower = 10f;
    [SerializeField] private float doubleJumpPower = 5f;
    [SerializeField] private int doubleJumpCount = 5;

    [Header("Animation")]
    [SerializeField] private Vector2 jumpSqueeze;
    [SerializeField] private float jumpSqueezeSpeed = 0.1f;
    [SerializeField] private Vector2 thrustSqueeze;
    [SerializeField] private float thrustSqueezeSpeed = 0.2f;

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

            StartCoroutine(Squeeze(jumpSqueeze.x, jumpSqueeze.y, jumpSqueezeSpeed));
            jumpParticles.Play();

            isGrounded = false;
        } else {
            if (Input.GetButtonDown("Jump") && !isGrounded && doubleJumpSupply > 0)
            {
                rigidbodyComponent.AddForce(
                    new Vector3(
                        0,
                        doubleJumpPower,
                        0
                    ),
                    ForceMode.Impulse
                );

                StartCoroutine(Squeeze(thrustSqueeze.x, thrustSqueeze.y, thrustSqueezeSpeed));
                thrustParticles.Play();

                doubleJumpSupply--;
            }
        }
    }

    IEnumerator Squeeze(float x, float y, float seconds)
    {
        var originalSize = Vector3.one;
        var newSize = new Vector3(x, y, originalSize.z);

        var t = 0f;

        while (t <= 1)
        {
            t += Time.deltaTime / seconds;

            sprite.transform.localScale = Vector3.Lerp(originalSize, newSize, t);
            
            yield return null;
        }

        t = 0f;

        while (t <= 1)
        {
            t += Time.deltaTime / seconds;

            sprite.transform.localScale = Vector3.Lerp(newSize, originalSize, t);

            yield return null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            isGrounded = true;
            doubleJumpSupply = doubleJumpCount;
        }
    }

    private void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
    }
}
