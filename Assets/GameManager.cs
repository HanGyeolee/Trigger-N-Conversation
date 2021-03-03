using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject EventSystem;

    private void Awake()
    {
        var g = GameObject.FindWithTag("GameController");

        if (g == null) 
            Instantiate(EventSystem);
    }
}
