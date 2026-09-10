using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCreator : MonoBehaviour
{
    public static BulletCreator Instance;
    private GameObject bulletSpawner;

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        
        bulletSpawner = Resources.Load("Prefabs/BulletSpawner") as GameObject;
        if (bulletSpawner == null) Debug.Log("NULL DAG NABBIT");
    }

    public static BulletSpawner SpawnBullets(BulletSpawnerData bulletSpawnerData, Vector3 spawnPoint, Quaternion rotation)
    {
        if (Instance == null)
        {
            Debug.LogError("BulletCreator Instance is missing from the scene!");
            return null;
        } 

        if (Instance.bulletSpawner == null)
        {
            Debug.LogError("Failed to load 'Prefabs/BulletSpawner' from Resources.");
            return null;
        }
        GameObject bSpawner = Instantiate(BulletCreator.Instance.bulletSpawner, spawnPoint, rotation);
        BulletSpawner bulletSpawnerScript = bSpawner.GetComponent<BulletSpawner>();
        bulletSpawnerScript.Init(bulletSpawnerData);
        return bulletSpawnerScript;
    }
}
