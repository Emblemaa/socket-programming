using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterLibrary : MonoBehaviour
{
    public static CharacterLibrary Instance { get; private set; }

    [SerializeField] private List<Sprite> characterList;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public Sprite GetCharacter(int id)
    {
        id = Mathf.Clamp(id, 0, characterList.Count-1);
        return characterList[id];
    }

    public int GetCount()
    {
        return characterList.Count;
    }
}
