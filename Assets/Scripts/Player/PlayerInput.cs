using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private Vector3 velocity = new Vector3(0.02f, 0, 0);

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.RightArrow))
            transform.position += velocity;
        else if (Input.GetKey(KeyCode.LeftArrow))
            transform.position -= velocity;
    }
}
