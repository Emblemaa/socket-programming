using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuppetMaster : MonoBehaviour
{
    private Dictionary<string, Puppet> puppets = new Dictionary<string, Puppet>();

    [SerializeField]
    private GameObject PuppetPrefab;
    [SerializeField]
    private GameObject BulletPrefab;

    private void OnEntityUpdate(string para)
    {
        string[] list = para.Split('|');
        string id = list[0];
        Vector3 pos = new Vector3(float.Parse(list[1]), float.Parse(list[2]), float.Parse(list[3]));
        Quaternion rotation = new Quaternion(float.Parse(list[4]), float.Parse(list[5]), float.Parse(list[6]), float.Parse(list[7]));
        float roll = float.Parse(list[8]);
        if (puppets.ContainsKey(id))
        {
            //Update current puppet
            puppets[id].OnEntityUpdate(pos, rotation, roll);
        }
        else
        {
            //Create new puppet
            GameObject newPuppet = Instantiate(PuppetPrefab, pos, Quaternion.identity, null);
            newPuppet.name = id;
            puppets.Add(id, newPuppet.GetComponent<Puppet>());
        }
    }

    private void OnEntityAttack(string para)
    {
        try
        {
            string[] list = para.Split('|');
            string id = list[0];
            if (puppets.ContainsKey(id))
            {
                //Update current puppet
                puppets[id].OnEntityAttack();
            }
            Vector3 pos = new Vector3(float.Parse(list[1]), float.Parse(list[2]), float.Parse(list[3]));
            Quaternion rotation = new Quaternion(float.Parse(list[4]), float.Parse(list[5]), float.Parse(list[6]), float.Parse(list[7]));
            GameObject newPuppet = Instantiate(BulletPrefab, pos, rotation, null);
            newPuppet.name = id;
        } 
        catch (Exception e)
        {

        }
    }

    private void OnEntityDie(string para)
    {
        string[] list = para.Split('|');
        string id = list[0];
        if (puppets.ContainsKey(id))
        {
            //Update current puppet
            puppets[id].OnEntityDie();
        }
    }

    void Awake()
    {
        EventManager.Instance.OnEntityUpdate += OnEntityUpdate;
        EventManager.Instance.OnEntityAttack += OnEntityAttack;
        EventManager.Instance.OnEntityDie += OnEntityDie;
    }

    void OnDisable()
    {
        EventManager.Instance.OnEntityUpdate -= OnEntityUpdate;
        EventManager.Instance.OnEntityAttack -= OnEntityAttack;
        EventManager.Instance.OnEntityDie -= OnEntityDie;
    }
}
