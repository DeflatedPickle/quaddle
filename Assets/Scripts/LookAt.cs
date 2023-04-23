using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 45f;
    [SerializeField] private float distance;

    private Quaternion originalDirection;

    void Start()
    {
        originalDirection = transform.rotation;
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, target.position) < distance)
        {
            Vector3 direction = target.position - transform.position;
            direction.y = 0.0f;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction),
                speed * Time.deltaTime
            );
        }
        else
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                originalDirection,
                speed * Time.deltaTime
            );
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distance);
    }
}
