using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    public static EntityManager instance;
    public string resourceFolder;
    public SpawnEntity[] spawnableEntities;

    private void Awake()
    {
        instance = this;
    }

    public void SpawnEntityAt(Vector3 position, EntityType type)
    {
        foreach(SpawnEntity se in spawnableEntities)
        {
            if (se.type == type)
            {
                GameObject obj = Resources.Load<GameObject>(resourceFolder + se.resourceFileName);
                Instantiate(obj, position, Quaternion.identity);
            }
        }
    }
}

[Serializable]
public struct SpawnEntity
{
    public EntityType type;
    public string resourceFileName;
}

public enum EntityType { Cow };