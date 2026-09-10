

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Alchemy.Inspector;
using Unity.VisualScripting;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public struct BulletState
{
    public BulletBlueprint BulletBlueprint;
    public Vector2 Position;
    public Vector2 Direction;
    public GameObject BulletObj;
    public Transform Transform;
    [FormerlySerializedAs("_radius")] [HideInInspector] public float Radius;
    public bool Destroy;
    public float LifeTime;
    public int BulletID;
}

public interface IBulletBehaviour
{
    void Execute(ref BulletState bulletState, float deltaTime, BulletManager bulletManager);
    void Start(ref BulletState bulletState, BulletManager bulletManager);
}


[Serializable]
public class TimeTilDeath : IBulletBehaviour {
    public float maxLifetime = 5f;
    public void Execute(ref BulletState bulletState, float deltaTime, BulletManager bulletManager)
    {
        bulletState.LifeTime += deltaTime;
        if (bulletState.LifeTime >= maxLifetime)
        {
            bulletState.Destroy = true;
        }
    }

    public void Start(ref BulletState bulletState, BulletManager bulletManager) {}
}

[Serializable]
public class StraightMove : IBulletBehaviour
{
    [SerializeField] private float _baseSpeed = 2f;
    [SerializeField] private float _randomSpeedOffset = 0;
    public void Execute(ref BulletState bulletState, float deltaTime, BulletManager bulletManager)
    {
        bulletState.Position += bulletState.Direction * bulletManager.StraightMoveData[bulletState.BulletID].Speed * deltaTime;
    }

    public void Start(ref BulletState bulletState, BulletManager bulletManager)
    {
        bulletManager.StraightMoveData[bulletState.BulletID].Speed = _baseSpeed + Random.Range(-_randomSpeedOffset, _randomSpeedOffset);
    }
}

public struct StraightMoveData
{
    public float Speed;
}

[Serializable]
public class Rotation : IBulletBehaviour
{
    [SerializeField] private float _rotAmount = 10f;
    public void Execute(ref BulletState bulletState, float deltaTime, BulletManager bulletManager)
    {
        float degreesToRotate = deltaTime * _rotAmount;
        bulletState.Direction = Quaternion.Euler(0,0,degreesToRotate) * bulletState.Direction;
    }

    public void Start(ref BulletState bulletState, BulletManager bulletManager)
    {
    }
}

[Serializable]
public class Homing : IBulletBehaviour
{

    [SerializeField] private float _maxRotationPerSecond = 2f;
    [SerializeField] private float _rotationSpeed = 0.5f;
    public enum HomingType
    {
        ClosestTransform,
        SpecificTransform
    }

    public HomingType HomingSelector;
    
    private bool _specific => HomingSelector == HomingType.SpecificTransform;
    private bool _nearest => HomingSelector == HomingType.ClosestTransform;
    [ShowIf(nameof(_specific))]
    [SerializeField] private string _specificTarget;

    [ShowIf(nameof(_nearest))] [SerializeField]
    private List<string> _tags;
    
    public void Execute(ref BulletState bulletState, float deltaTime, BulletManager bulletManager)
    {
        Vector2 targetDir = bulletManager.HomingData[bulletState.BulletID].HomingTarget.position - bulletState.Transform.position;
        float angle = Vector2.SignedAngle(bulletState.Direction, targetDir);
        angle *= _rotationSpeed;
        angle = Mathf.Clamp(angle, -_maxRotationPerSecond, _maxRotationPerSecond) * deltaTime;
        bulletState.Direction = Quaternion.Euler(0, 0, angle) * bulletState.Direction;
    }

    public void Start(ref BulletState bulletState, BulletManager bulletManager)
    {
        if(_specific) bulletManager.HomingData[bulletState.BulletID].HomingTarget = GameObject.Find(_specificTarget).transform;
        else if (_nearest)
        { 
            List<GameObject> homingObjects = new List<GameObject>();
            foreach (string tag in _tags)
            {
                GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
                foreach (GameObject target in targets)
                {
                    bulletManager.HomingData[bulletState.BulletID].HomingObjects.Add(target);
                }
            }
        }
    }
}

public struct HomingData
{
    public Transform HomingTarget;
    public List<GameObject> HomingObjects;
}

[Serializable]
public class SinWave : IBulletBehaviour
{
    [SerializeField] private float _angle = 90f;
    [SerializeField]private float _speed = 10f;
    [SerializeField] private float _size = 1f;
    public void Execute(ref BulletState bulletState, float deltaTime, BulletManager bulletManager)
    {
        Vector2 signDir = Quaternion.Euler(0, 0, _angle) * bulletState.Direction;
        bulletState.Position += Mathf.Sin(bulletState.LifeTime * _speed) * signDir * deltaTime * _size * Mathf.PI * 2 * _speed;
    }

    public void Start(ref BulletState bulletState, BulletManager bulletManager)
    {
    }
}

[Serializable]
public class MultiPattern : IBulletBehaviour
{
    [SerializeReference]
    public List<IBulletBehaviour> Behaviours = new List<IBulletBehaviour>();

    public void Execute(ref BulletState bulletState, float deltaTime, BulletManager bulletManager)
    {
        foreach (IBulletBehaviour behaviour in bulletManager.MultiPatternData[bulletState.BulletID].Behaviours)
        {
            behaviour.Execute(ref bulletState, deltaTime, bulletManager);
        }
    }

    public void Start(ref BulletState bulletState, BulletManager bulletManager)
    {
        foreach (IBulletBehaviour behaviour in Behaviours)
        {
            behaviour.Start(ref bulletState, bulletManager);
            bulletManager.MultiPatternData[bulletState.BulletID].Behaviours.Add(behaviour);
        }
    }
}

public struct MultiPatternData
{
    public List<IBulletBehaviour> Behaviours;
}


[Serializable]
public class DelayPattern : IBulletBehaviour
{
    [SerializeField] private float _delayTime = 2f;
    [SerializeField] private float _randomDelayOffset = 0f;
    [SerializeReference]
    private IBulletBehaviour _behaviour;

    public void Execute(ref BulletState bulletState, float deltaTime, BulletManager bulletManager)
    {
        ref DelayPatternData data = ref bulletManager.DelayPatternData[bulletState.BulletID];
        if (!data.Started && data.StartTime > Time.time)
        {
            data.Behaviour = _behaviour;
            data.Behaviour.Start(ref bulletState, bulletManager);
            data.Started = true;
        }
        if(data.Started) data.Behaviour.Execute(ref bulletState, deltaTime, bulletManager);
    }

    public void Start(ref BulletState bulletState, BulletManager bulletManager)
    {
        bulletManager.DelayPatternData[bulletState.BulletID].StartTime = Time.time + _delayTime + Random.Range(-_randomDelayOffset, _randomDelayOffset);
        bulletManager.DelayPatternData[bulletState.BulletID].Started = false;
    }
}

public struct DelayPatternData
{
    public IBulletBehaviour Behaviour;
    public float StartTime;
    public bool Started;
}

[Serializable]
public class TerminatingPattern : IBulletBehaviour
{
    public float TerminationTime = 5f;
    [SerializeReference]
    public IBulletBehaviour Behaviour;

    public void Execute(ref BulletState bulletState, float deltaTime, BulletManager bulletManager)
    {
        ref TerminatePatternData data = ref bulletManager.TerminatePatternData[bulletState.BulletID];
        if (data.Terminated) return;
        data.Behaviour.Execute(ref bulletState, deltaTime, bulletManager);
        if(data.TerminateTime <= Time.time) data.Terminated = true;
    }

    public void Start(ref BulletState bulletState, BulletManager bulletManager)
    {
        Behaviour.Start(ref bulletState, bulletManager);
        bulletManager.TerminatePatternData[bulletState.BulletID].Behaviour = Behaviour;
        bulletManager.TerminatePatternData[bulletState.BulletID].TerminateTime = Time.time + TerminationTime;
        bulletManager.TerminatePatternData[bulletState.BulletID].Terminated = false;

    }
}

public struct TerminatePatternData
{
    public IBulletBehaviour Behaviour;
    public float TerminateTime;
    public bool Terminated;
}


public interface IBulletCollisionEffect
{
    public bool OnCollision(ref BulletState bulletState, Collider2D[] colliders, BulletManager bulletManager);
}

public class BasicBullet : IBulletCollisionEffect
{
    public bool OnCollision(ref BulletState bulletState, Collider2D[] colliders, BulletManager bulletManager)
    {
        bool destroyObj = true; 
        foreach (Collider2D obj in colliders)
        {
            if (obj == null) break;
            Player player = obj.GetComponent<Player>();
            if (player != null) destroyObj = player.Damage();
        }
        return destroyObj;
    }
}

[CreateAssetMenu(menuName = "BulletSystem/Bullets/BulletState")]
public class BulletBlueprint : ScriptableObject
{
    [SerializeReference]
    public List<IBulletBehaviour> Behaviours = new List<IBulletBehaviour>();
    [SerializeReference]
    public List<IBulletCollisionEffect> CollisionEffects = new List<IBulletCollisionEffect>();
}