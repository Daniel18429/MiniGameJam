using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loop : MonoBehaviour
{
    private float timer = 0.0f;
    public float loopTime = 0.5f;
    public BulletSpawnerData bulletSpawnerData;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (timer <= 0)
        {
            BulletCreator.SpawnBullets(bulletSpawnerData, this.transform.position, this.transform.rotation);
            timer = loopTime;
        }
        else
        {
            timer -= Time.deltaTime;
        }
    }
}
