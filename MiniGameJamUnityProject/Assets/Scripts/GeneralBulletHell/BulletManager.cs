using System;
using System.Collections;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class BulletManager : MonoBehaviour
{
    [HideInInspector]
    public static BulletManager Instance;
    
    // BulletPool variables
    private const int MAX_BULLETS = 20000;
    [SerializeReference] BulletState[] activeBullets = new BulletState[MAX_BULLETS];
    [SerializeField] private int _currentBullet = 0;
    private GameObject _bulletPrefab;

    // Array vars for specific behaviour data
    [HideInInspector]
    public StraightMoveData[] StraightMoveData = new StraightMoveData[MAX_BULLETS];
    [HideInInspector]
    public HomingData[] HomingData = new HomingData[MAX_BULLETS];
    [HideInInspector]
    public MultiPatternData[] MultiPatternData = new MultiPatternData[MAX_BULLETS];
    [HideInInspector]
    public DelayPatternData[] DelayPatternData = new DelayPatternData[MAX_BULLETS];
    [HideInInspector]
    public TerminatePatternData[] TerminatePatternData = new TerminatePatternData[MAX_BULLETS];

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        } 
        Instance = this;
        
        _bulletPrefab = Resources.Load("Prefabs/Bullet") as GameObject;
        foreach (ref BulletState bullet in activeBullets.AsSpan())
        {
            bullet.BulletObj = Instantiate(_bulletPrefab, this.transform);
            SceneVisibilityManager.instance.DisablePicking(bullet.BulletObj, true);
            bullet.Transform = bullet.BulletObj.transform;
            bullet.BulletObj.SetActive(false);
        }
    }
    

    public void Update()
    {
        // Perform all bullet behaviours for every bullet in pool
        // Looping backwards because we may modify the list
        for(int i = _currentBullet - 1; i >= 0; i--)
        {
            int bulletID = i;
            ref BulletState state = ref activeBullets[bulletID];
            BulletBlueprint bulletBlueprint = state.BulletBlueprint;
            foreach (IBulletBehaviour bulletBehaviour in bulletBlueprint.Behaviours)
            {
                bulletBehaviour.Execute(ref state, Time.deltaTime, this);
            }
            state.Transform.position = state.Position;
            Collider2D[] results = new Collider2D[3];
            int layerMask = LayerMask.GetMask("Obstacle", "Player");
            Physics2D.OverlapCircleNonAlloc(state.Transform.position, state.Radius, results, layerMask);
            if (results[0] != null)
            {
                bool destroyBullet = false;
                foreach (IBulletCollisionEffect collisionEffect in state.BulletBlueprint.CollisionEffects)
                {
                    destroyBullet = (destroyBullet || collisionEffect.OnCollision(ref state, results, this));
                }
                state.Destroy = destroyBullet;
            }
            if (state.Destroy) RefactorBullet(bulletID);
            
        }
    }

    // Logic for spawning new bullet
    public GameObject SetBullet(BulletBlueprint bulletBlueprint, Vector2 position, Vector2 direction)
    {
        if (_currentBullet == MAX_BULLETS)
        {
            throw new Exception("MAX BULLETS USED");
        }
        
        // Reset all bullet data and start the bullet
        ref BulletState bulletState = ref activeBullets[_currentBullet];
        bulletState.BulletBlueprint = bulletBlueprint;
        bulletState.Position = position;
        bulletState.Direction = direction;
        ResetBullet(ref bulletState);
        GameObject tempBullet = bulletState.BulletObj;
        tempBullet.gameObject.SetActive(true);
        _currentBullet++;
        return tempBullet;
    }

    // "Deleting" a bullet
    public void RefactorBullet(int i)
    {
        // Turn off bullet
        activeBullets[i].BulletObj.SetActive(false);
        
        // Swap deleted bullet and active bullet in last location in activebullets
        BulletState deadBullet = activeBullets[i];
        int endIdx = _currentBullet - 1;
        activeBullets[i] = activeBullets[endIdx];
        activeBullets[i].BulletID = i;
        activeBullets[endIdx] = deadBullet;
        
        // ADD SWAPPING FUNCTIONALITY FOR ANY ARRAYS WITH CUSTOM BULLET DATA BELOW
        // Swap custom data from bullets
        StraightMoveData[i] = StraightMoveData[endIdx];
        HomingData[i] = HomingData[endIdx];
        MultiPatternData[i] = MultiPatternData[endIdx];
        DelayPatternData[i] = DelayPatternData[endIdx];
        TerminatePatternData[i] = TerminatePatternData[endIdx];
        
        // Stop tracking dead bullet
        _currentBullet--;
    }

    // Sets bullet to fresh state
    public void ResetBullet(ref BulletState bullet)
    {
        
        // Set basic vars
        bullet.BulletObj.SetActive(true);
        bullet.Direction = bullet.Transform.up;
        bullet.Destroy = false;
        bullet.LifeTime = 0;
        bullet.BulletID = _currentBullet;
        bullet.Radius = bullet.BulletObj.GetComponent<SpriteRenderer>().bounds.size.x * 0.5f * 0.9f;
        
        // Start all behaviours
        foreach (IBulletBehaviour bulletBlueprint in bullet.BulletBlueprint.Behaviours)
        {
            bulletBlueprint.Start(ref bullet, this);
        }
    }
}