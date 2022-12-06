using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerSwing : MonoBehaviour
{
    public PlaneController controller;
    void Update()
    {
        if(controller == null)
        {
            transform.RotateAround(transform.position, transform.up, 20 * 4 * Time.deltaTime);
            return;
        }
        transform.RotateAround(transform.position, transform.up, controller.throttle * 4 * Time.deltaTime);
    }
}
