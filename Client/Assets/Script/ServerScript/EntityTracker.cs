using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityTracker : MonoBehaviour
{
    private static int CLIENT_TICK = 30;
    private float sendTime = 0;
    public PlaneController controller;
    protected virtual string GetState()
    {
        Vector3 pos = transform.position;
        return pos.x.ToString("0.00")+"|"+pos.y.ToString("0.00")+"|"+pos.z.ToString("0.00")+"|"+transform.rotation.x+"|"+transform.rotation.y+"|"+transform.rotation.z+"|"+transform.rotation.w+"|"+controller.roll;
    }
    void Update()
    {
        sendTime += Time.deltaTime;
        if (sendTime > 1f / CLIENT_TICK)
        {
            sendTime = 0;
            ServerHandler.Instance.SendPackage("UPDATE_INFO#" + GetState(), SendType.UDP);
        }
    }
}
