using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    [SerializeField] private float destructTime = 1.5f;
    void Update()
    {
        destructTime -= Time.deltaTime;
        if(destructTime < 0)
        {
            Destroy(gameObject);
        }
    }
}
