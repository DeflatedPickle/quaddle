using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using Yarn.Unity;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    private Rigidbody rigidbodyComponent;

    [Header("Intro")]
    [SerializeField] private int stuckCounter = 3;
    [SerializeField] private bool inTrash = true;
    [SerializeField] private Collider trashCanCollider;
    [SerializeField] private Rigidbody trashCanRigidbody;

    [Header("Data")]
    [SerializeField] public bool canMove = false;
    [SerializeField] private Vector2 movement;
    [SerializeField] private int doubleJumpSupply;
    [SerializeField] private bool isJumping;
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isFloating;
    [SerializeField] private bool isSucking;
    [SerializeField] private bool inLove;

    [Header("Component")]
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Animator animator;
    [SerializeField] private DecalProjector shadow;

    [SerializeField] private ParticleSystem jumpParticles;
    [SerializeField] private ParticleSystem thrustParticles;
    [SerializeField] private ParticleSystem floatParticles;
    [SerializeField] private ParticleSystem loveParticles;

    [Header("Sprite")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite suckSprite;
    [SerializeField] private Sprite loveSprite;

    [Header("Sounds")]
    [SerializeField] private AudioSource purr;
    [SerializeField] private float purrVolume;
    [SerializeField] private AudioSource hmmm;
    [SerializeField] private AudioSource jump;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpPower = 10f;
    [SerializeField] private float doubleJumpPower = 5f;
    [SerializeField] private int doubleJumpCount = 5;
    [SerializeField] private int suckCircle = 6;

    [Header("Animation")]
    [SerializeField] private Vector2 jumpSqueeze;
    [SerializeField] private float jumpSqueezeSpeed = 0.1f;
    [SerializeField] private Vector2 thrustSqueeze;
    [SerializeField] private float thrustSqueezeSpeed = 0.2f;

    private void Awake()
    {
        rigidbodyComponent = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        TrashEscape();

        if (!canMove) return;

        rigidbodyComponent.velocity = transform.TransformDirection(
            movement.x * moveSpeed,
            rigidbodyComponent.velocity.y,
            movement.y * moveSpeed
        );

        if (isJumping && isGrounded)
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

            if (!jump.isPlaying)
            {
                StartCoroutine(FadeIn(jump, purrVolume, 2f));
            }

            isGrounded = false;
            isJumping = false;
        } else {
            if (isJumping && !isGrounded && doubleJumpSupply > 0)
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

                if (!jump.isPlaying)
                {
                    StartCoroutine(FadeIn(jump, purrVolume, 2f));
                }

                doubleJumpSupply--;
                isJumping = false;
            }

            if (isFloating && !isGrounded && doubleJumpSupply <= 0)
            {
                rigidbodyComponent.drag = 6;
                floatParticles.Play();
            }
            else
            {
                rigidbodyComponent.drag = 0;
            }
        }

        if (isSucking)
        {
            sprite.sprite = suckSprite;

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, suckCircle, 1 << 6);

            foreach (var obj in hitColliders)
            {
                if (obj.GetComponent<Rigidbody>() != null)
                {
                    obj.GetComponent<Rigidbody>().AddForce(
                        (transform.position - obj.transform.position).normalized,
                        ForceMode.Impulse
                    );
                }
            }
        }
        else if (inLove)
        {
            sprite.sprite = loveSprite;
            loveParticles.Play();
            
            if (!purr.isPlaying)
            {
                StartCoroutine(FadeIn(purr, purrVolume, 2f));
            }
        }
        else if (!inLove)
        {
            sprite.sprite = normalSprite;
            StartCoroutine(FadeOut(purr, 2f));
        }
        else
        {
            sprite.sprite = normalSprite;
        }
    }

    private void FixedUpdate() 
    {
        int layerMask = 1 << 8;
        layerMask = ~layerMask;
        
        RaycastHit hit;

        if (Physics.Raycast(
            transform.position,
            transform.TransformDirection(Vector3.down),
            out hit, Mathf.Infinity, layerMask)
            )
        {
            var lerp = Mathf.Lerp(1f, 0.2f, hit.distance / 10);
            shadow.fadeFactor = lerp;
        }
    }

    public IEnumerator Squeeze(float x, float y, float seconds)
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

    public static IEnumerator FadeOut(AudioSource audioSource, float fadeTime) {
		float startVolume = audioSource.volume;
		while (audioSource.volume > 0) {
			audioSource.volume -= startVolume * Time.deltaTime / fadeTime;
			yield return null;
		}
		audioSource.Stop();
	}

	public static IEnumerator FadeIn(AudioSource audioSource, float maxVolume, float fadeTime) {
			audioSource.Play();
			audioSource.volume = 0f;
			while (audioSource.volume < maxVolume) {
				audioSource.volume += Time.deltaTime / fadeTime;
				yield return null;
		}
	}

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            isGrounded = true;
            isFloating = false;
            doubleJumpSupply = doubleJumpCount;
        }
    }

    private void OnMove(InputValue value)
    {
        movement = value.Get<Vector2>();

        if (movement.x > 0)
        {
            animator.SetInteger("Horizontal Tilt", 1);
        }
        else if (movement.x < 0)
        {
            animator.SetInteger("Horizontal Tilt", -1);
        } else {
            animator.SetInteger("Horizontal Tilt", 0);
        }

        if (movement.y > 0)
        {
            animator.SetInteger("Vertical Tilt", 2);
        }
        else if (movement.y < 0)
        {
            animator.SetInteger("Vertical Tilt", -2);
        } else {
            animator.SetInteger("Vertical Tilt", 0);
        }
    }

    private void OnJump(InputValue value)
    {
        isJumping = value.isPressed;
    }

    private void OnFloat(InputValue value)
    {
        isFloating = value.isPressed;
    }

    private void OnSuck(InputValue value)
    {
        isSucking = value.isPressed;
    }

    private void OnPurr(InputValue value)
    {
        inLove = value.isPressed;
    }

    void OnDrawGizmos()
    {
        int layerMask = 1 << 8;
        layerMask = ~layerMask;
        
        RaycastHit hit;

        if (Physics.Raycast(
            transform.position,
            transform.TransformDirection(Vector3.down),
            out hit, Mathf.Infinity, layerMask)
            )
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * hit.distance);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, suckCircle);
    }

    private void TrashEscape()
    {
        if (inTrash)
        {
            sprite.transform.localScale = new Vector3(0.4f, 1f, 1f);
            canMove = false;

            if (isJumping)
            {
                if (stuckCounter > 0)
                {
                    StartCoroutine(Squeeze(jumpSqueeze.x, jumpSqueeze.y, jumpSqueezeSpeed));
                    jumpParticles.Play();

                    // make a cute sound
                    
                    isJumping = false;
                    stuckCounter--;
                }
                else
                {
                    sprite.transform.localScale = new Vector3(1f, 1f, 1f);

                    rigidbodyComponent.AddForce(
                        new Vector3(
                            0,
                            doubleJumpPower,
                            0
                        ),
                        ForceMode.Impulse
                    );

                    trashCanCollider.enabled = true;
                    trashCanRigidbody.useGravity = true;

                    canMove = true;
                    inTrash = false;
                }
            }
        }
    }

    [YarnCommand("quad_hmmm")]
    public void QuadHmmm()
    {
        StartCoroutine(FadeIn(hmmm, purrVolume, 2f));
    }
}
