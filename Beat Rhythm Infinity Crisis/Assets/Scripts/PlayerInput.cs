using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour {


    Animator animator;

	// Use this for initialization
	void Start () {

        animator = gameObject.GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey("s"))
        {

            animator.SetInteger("Dance", 1);
        }
        if (Input.GetKey("d"))
        {

            animator.SetInteger("Dance", 2);
        }
        if (Input.GetKey("f"))
        {

            animator.SetInteger("Dance", 3);
        }
        if (Input.GetKey("g"))
        {

            animator.SetInteger("Dance", 4);
        }
        if(Input.GetKeyUp("s") || Input.GetKeyUp("d") || Input.GetKeyUp("f") || Input.GetKeyUp("g")){
            animator.SetInteger("Dance", 0);
        }
	}
}
