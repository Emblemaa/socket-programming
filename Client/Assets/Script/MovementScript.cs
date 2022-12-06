using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementScript : MonoBehaviour
{
    public float speed = 10f;  
    private Rigidbody rb;
    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();  
    }
     
    void Update()
    {
        float xMove = Input.GetAxis("Horizontal"); 
        float zMove = Input.GetAxis("Vertical");
        rb.AddForce(new Vector3(xMove, 0, zMove) * speed * Time.deltaTime, ForceMode.Impulse);
        Vector3 direction = rb.velocity;
        direction.y = 0;

        transform.LookAt(transform.position + direction, Vector3.up);
        animator.SetBool("isRunning", direction.magnitude != 0);

        if(Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("attack");
            ServerHandler.Instance.SendPackage("PLAYER_ATTACK", SendType.TCP);
            foreach(Puppet puppet in FindObjectsOfType<Puppet>())
            {
                Vector3 dir = puppet.transform.position - transform.position;
                if (dir.magnitude < 2.5)
                {
                    Vector3 position = transform.position + dir*0.75f;
                    position.y += 1.5f;
                }
            }
        }
    }
}
