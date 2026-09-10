using System.Collections;
using System.Collections.Generic;
using Alchemy.Inspector;
using UnityEngine;


[CreateAssetMenu(menuName = "BulletSystem/BulletSpawner/BulletSpawner")]
public class BulletSpawnerData : ScriptableObject
{
    public BulletBlueprint BulletBlueprint;
    [SerializeReference]
    public IBulletSpawner BulletSpawnPattern;
    [Min(0f)]
    public float SpawnDuration = 0.0f;
    // [Min(1)]
    // public int NumSpawns = 0;
    // private bool moreThanOneSpawn => NumSpawns > 1;
    // [ShowIf(nameof(moreThanOneSpawn))]
    // public float timeBetweenSpawns = 0.0f;
}
