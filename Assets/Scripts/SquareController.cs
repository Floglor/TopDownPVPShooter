using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SquareController : MonoBehaviour, IStunnable
{
    private InputManager _inputManager;
    [SerializeField] private float _speed = 10f;

    private float _xMove;
    private float _yMove;

    private bool _isStunned;
    private Rigidbody2D _rb;

    private Vector2 _currentMovement;
    
    private Coroutine _stunCoroutine;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _inputManager = InputManager.instance;
        Stun(new Vector2(0, 10), 3f, 1f, 1f);
    }

    private void Update()
    {
        if (_isStunned) return;
        LookTowardsCamera();
        ManageMovement();
    }

    private void LookTowardsCamera()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        Vector3 objectPosition = transform.position;

        float angle = Mathf.Atan2(mousePosition.y - objectPosition.y, mousePosition.x - objectPosition.x) *
                      Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
    }

    private void FixedUpdate()
    {
        if (!_isStunned)
            _rb.velocity = _currentMovement * Time.fixedDeltaTime;

        _currentMovement = Vector2.zero;
    }

    private void ManageMovement()
    {
        if (_inputManager.GetKey(KeyBindAction.Up))
        {
            _yMove = 1;
        }
        else if (_inputManager.GetKey(KeyBindAction.Down))
        {
            _yMove = -1;
        }

        if (_inputManager.GetKey(KeyBindAction.Left))
        {
            _xMove = -1;
        }
        else if (_inputManager.GetKey(KeyBindAction.Right))
        {
            _xMove = 1;
        }
 
        _currentMovement = new Vector2(_xMove, _yMove).normalized * _speed;

        _xMove = 0;
        _yMove = 0;
    }

    public void Stun(Vector2 force, float rotation, float duration, float rotationDuration)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        
        if (_stunCoroutine != null)
        {
            StopCoroutine(_stunCoroutine);
        }
        
        rb.AddForce(force, ForceMode2D.Impulse);

        StartCoroutine(RotateOverTime(rotation, rotationDuration));

        _isStunned = true;
        _stunCoroutine = StartCoroutine(RemoveStunAfterDuration(duration));
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
        _isStunned = false;
    }
}