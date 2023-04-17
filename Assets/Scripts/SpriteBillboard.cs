
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteBillboard : MonoBehaviour
{
    void Update()
    {
        var camera = Camera.main.transform.rotation.eulerAngles;
        
        transform.rotation = Quaternion.Euler(
            camera.x,
            camera.y,
            camera.y
        );
    }
}
