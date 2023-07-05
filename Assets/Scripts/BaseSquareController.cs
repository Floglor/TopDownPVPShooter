﻿using System.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class BaseSquareController : NetworkBehaviour, IStunnable
{
    protected float Speed = 10f;
    
    private float _xMove;
    private float _yMove;

    protected bool IsStunned;
    public  Rigidbody2D Rb;
    private Vector2 _currentMovement;
    private Coroutine _stunCoroutine;
        
    public void LookTowards(Vector3 target)
    {
        Vector3 objectPosition = transform.position;

        float angle = Mathf.Atan2(target.y - objectPosition.y, target.x - objectPosition.x) *
                      Mathf.Rad2Deg;

        //transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
        Rb.rotation = angle;
    }
    
    private void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
    }
        
    private void SetCurrentMovement()
    {
        _currentMovement = new Vector2(_xMove, _yMove).normalized * Speed;
    }

    [ServerRpc]
    protected void MoveServerRpc(float horizontal, float vertical)
    {
        _xMove = horizontal;
        _yMove = vertical;
        SetCurrentMovement();
    }

    public void SetMovement(float horizontal, float vertical)
    {
        _xMove = horizontal;
        _yMove = vertical;
    }
    

    public void MoveClient(float horizontal, float vertical)
    {
        _xMove = horizontal;
        _yMove = vertical;
        
        SetCurrentMovement();

        if (!IsStunned)
           Rb.velocity = _currentMovement * Time.fixedDeltaTime;

        _currentMovement = Vector2.zero;
    }
    
    public void MoveClientWithTime(float horizontal, float vertical, float tickRate)
    {
        _xMove = horizontal;
        _yMove = vertical;
        
        SetCurrentMovement();
        
        if (!IsStunned)
            Rb.velocity = _currentMovement * tickRate;

        _currentMovement = Vector2.zero;
    }
    
    public void MoveClientWithTimeTransform(float horizontal, float vertical, float tickRate)
    {
        // Calculate the movement vector based on the horizontal and vertical inputs.
        Vector3 movement = new Vector3(horizontal, 0f, vertical);

        // Normalize the movement vector so that diagonal movement isn't faster.
        movement = movement.normalized * Speed * tickRate;

        // Apply the movement to the client's transform.
        transform.Translate(movement);
    }
    private void FixedUpdate()
    {
       //if (!IsStunned)
       //    _rb.velocity = _currentMovement * Time.fixedDeltaTime;
//
       //_currentMovement = Vector2.zero;
    }
        
    public void Stun(Vector2 force, float rotation, float duration, float rotationDuration, bool fromPlayer)
    {
        if (fromPlayer && IsStunned)
        {
            LoseRound();
        }

        if (_stunCoroutine != null)
        {
            StopCoroutine(_stunCoroutine);
        }
        
        Rb.AddForce(force, ForceMode2D.Impulse);

        StartCoroutine(RotateOverTime(rotation, rotationDuration));

        IsStunned = true;
        _stunCoroutine = StartCoroutine(RemoveStunAfterDuration(duration));
    }

    private void LoseRound()
    {
        Debug.Log("Round lost");
    }


    private IEnumerator RotateOverTime(float rotation, float duration)
    {
        Vector3 startRotation = transform.eulerAngles;
        Vector3 endRotation = startRotation + new Vector3(0f, 0f, rotation);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.eulerAngles = Vector3.Lerp(startRotation, endRotation, t);
            yield return null;
        }

        transform.eulerAngles = endRotation;
    }

    private IEnumerator RemoveStunAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        IsStunned = false;
    }
}