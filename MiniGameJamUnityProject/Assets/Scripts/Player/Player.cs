using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
{
    public float WalkSpeed;
    public float DashSpeed;
    public float MaxIFrames = 0.2f;
    private float _iFrameTimer;
    public float IFrameDelay = 1f;
    private Vector2 _moveDir;
    private Vector2 _dashDir = Vector2.right;
    private SpriteRenderer _spriteRenderer;
    private Color dashColor;
    private Color normalColor;

    public void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        normalColor  = _spriteRenderer.color;
        dashColor = new Color(0.5f, 0.5f, 0.5f);
        
    }

    public void Update()
    {
        _iFrameTimer -= Time.deltaTime;
        if (_iFrameTimer <= 0)
        {
            float xMove = 0;
            float yMove = 0;
            if (Keyboard.current.aKey.isPressed)
            {
                xMove = -1;
            }
            else if (Keyboard.current.dKey.isPressed)
            {
                xMove = 1;
            }

            if (Keyboard.current.wKey.isPressed)
            {
                yMove = 1;
            }
            else if (Keyboard.current.sKey.isPressed)
            {
                yMove = -1;
            }
            _moveDir = new Vector2(xMove, yMove);
            _moveDir = _moveDir.normalized;
            if (_moveDir != Vector2.zero) _dashDir = _moveDir;
            this.transform.position += (Vector3)(WalkSpeed * _moveDir * Time.deltaTime);
            _spriteRenderer.color = normalColor;
        }
        else
        {
            this.transform.position += (Vector3)(_dashDir * DashSpeed * Time.deltaTime);
            _spriteRenderer.color = dashColor;
        }

        if (_iFrameTimer <= -IFrameDelay && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            _iFrameTimer = MaxIFrames;
        }
        
    }

    public bool Damage()
    {
        if (_iFrameTimer < 0)
        {
            Debug.Log("PLAYER HIT!");
            return true;
        }
        return false;
    }

}
