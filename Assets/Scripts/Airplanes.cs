using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Airplanes : MonoBehaviour
{
    // Start is called before the first frame update
    public Animator airplaneAnimator;

    void Start()
    {
        airplaneAnimator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame 
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            airplaneAnimator.SetTrigger("Start");
        }
    }
}
