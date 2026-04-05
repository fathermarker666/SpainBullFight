
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animation : MonoBehaviour
{
    Animator animator;
    int isWalkingHash;
       
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        isWalkingHash = Animator.StringToHash("isWalking");
    }

    // Update is called once per frame
    void Update()
    {
        bool isWalking = animator.GetBool(isWalkingHash);
        bool fp = Input.GetKey("w");
        if (fp)
        { 
            animator.SetBool("isWalking", true);
        }
        if (!fp)
        { 
            animator.SetBool("isWalking", false);
        }

    }
}
