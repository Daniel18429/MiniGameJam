using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Alchemy.Inspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class BulletSpawner : MonoBehaviour
{
    private BulletSpawnerData _data;
    private BulletBlueprint _bulletBlueprint;
    
    private int _numBulletsToSpawn;
    private int _nthBullet = 0;
    private float _spawnDuration = 0.0f;
    private float timeBetweenSpawns = 0.0f;
    private float timer = 0;
    
    private IBulletSpawner _bulletSpawnPattern;

    public void Init(BulletSpawnerData data)
    {
        _data = data;
        _bulletSpawnPattern = _data.BulletSpawnPattern;
        _spawnDuration = _data.SpawnDuration;
        _bulletBlueprint = _data.BulletBlueprint;

    }
    // Start is called before the first frame update
    void Start()
    {
        bool hasDeath = false;
        foreach (IBulletBehaviour bb in _bulletBlueprint.Behaviours)
        {
            if (bb.GetType() == typeof(TimeTilDeath))
            {
                hasDeath = true;
            }
        }
        
        if (!hasDeath)
        {
            _bulletBlueprint.Behaviours.Add(new TimeTilDeath());
        }
        
        bool hasDamage = false;
        foreach (IBulletCollisionEffect bce in _bulletBlueprint.CollisionEffects)
        {
            if(bce.GetType() == typeof(BasicBullet)) hasDamage = true;
        }
        
        if (!hasDamage)
        {
            _bulletBlueprint.CollisionEffects.Add(new BasicBullet());
        }

        _numBulletsToSpawn = Mathf.Max(_bulletSpawnPattern.NumBullets(), 1);
        timeBetweenSpawns = _spawnDuration / _numBulletsToSpawn;
    }

    // Update is called once per frame
    void Update()
    {
        if (_nthBullet == _numBulletsToSpawn) return;
        if (timeBetweenSpawns == 0)
        {
            for (int i = 0; i < _numBulletsToSpawn; i++)
            {
                (Vector2 pos, Vector2 rot) = _bulletSpawnPattern.SpawnNthBullet(_nthBullet, this);
                SpawnBullet(pos, rot.normalized);
                _nthBullet++;
            }

            return;
        }
        
        timer -= Time.deltaTime;
        while (timer <= 0 && _nthBullet < _numBulletsToSpawn)
        { 
            timer += timeBetweenSpawns;
            (Vector2 pos, Vector2 rot) = _bulletSpawnPattern.SpawnNthBullet(_nthBullet, this);
            SpawnBullet(pos, rot.normalized);
            _nthBullet++;
        }
    }

    void SpawnBullet(Vector2 position, Vector2 rotation)
    {
        BulletManager.Instance.SetBullet(_bulletBlueprint, position, rotation);
    }
}

public interface IBulletSpawner
{
    public int NumBullets();
    public (Vector2, Vector2) SpawnNthBullet(int n, BulletSpawner bulletSpawner);
}

[Serializable]
public class SpawnFromTransform : IBulletSpawner
{
    public Transform spawnTransform;
    public int BulletCount;
    public float RandomOffset;
    public float PermanentOffset;
    public int NumBullets()
    {
        return BulletCount;
    }
    
    public (Vector2, Vector2) SpawnNthBullet(int n, BulletSpawner bulletSpawner)
    {
        Vector2 position = spawnTransform.position;
        Vector2 rotation = spawnTransform.rotation.eulerAngles;
        float randomRotation = Random.Range(-RandomOffset, RandomOffset);
        rotation = Quaternion.Euler(0,0, randomRotation) * rotation;
        rotation = Quaternion.Euler(0, 0, PermanentOffset) * rotation;
        return (position, rotation);
    }
}

[Serializable]
public class NGonSpawner : IBulletSpawner
{
    [Min(3)]
    public int nGon = 3;
    
    [Min(1)]
    public int bulletsPerSide = 2;
    
    [Min(0.0001f)]
    public float PolygonSize = 2;

    public enum RotationPattern
    {
        AwayFromCenter,
        BulletSpawnerRotation
    }

    public RotationPattern RotationType;
    
    public float RotationOffset;
    
    public int NumBullets()
    {
        return nGon * bulletsPerSide;
    }

    public (Vector2, Vector2) SpawnNthBullet(int n, BulletSpawner bulletSpawner)
    {
        Vector3 spawnPoint = bulletSpawner.transform.position;
        spawnPoint.z = 0;
        Vector3 rotationOffset = Vector3.up;
        float anglePerSection = 360.0f / nGon;
        int sectionIndex = Mathf.FloorToInt(n / bulletsPerSide);
        Vector3 startRotation = Quaternion.Euler(0,0, anglePerSection * sectionIndex) * rotationOffset;
        Vector3 endRotation = Quaternion.Euler(0,0, anglePerSection * (sectionIndex + 1)) * rotationOffset; 
        Vector3 startVertex = spawnPoint + startRotation * PolygonSize;
        Vector3 endVertex = spawnPoint + endRotation * PolygonSize;
        float percentLine = (float)(n % bulletsPerSide) / bulletsPerSide;

        Vector2 spawnPos = Vector2.Lerp(startVertex, endVertex, percentLine);
        Vector2 spawnRot = Vector2.zero;
        if (RotationType == RotationPattern.AwayFromCenter)
        {
            spawnRot = spawnPos - (Vector2)spawnPoint;
        }
        else if (RotationType == RotationPattern.BulletSpawnerRotation)
        {
            spawnRot = bulletSpawner.transform.up;
        }
        spawnRot = Quaternion.Euler(0, 0, RotationOffset) * spawnRot;
        return (spawnPos, spawnRot);
    }
}