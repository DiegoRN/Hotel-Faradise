using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MovementKate : MonoBehaviour
{
    public float weight = 0f;
    public float speed = 2f;
    public float jumpForce = 5.0f;
    public float iceSpeed;

    private Rigidbody2D _body;
    private Animator _anim;
    private BoxCollider2D _box;

    [SerializeField] private LayerMask m_WhatIsGround; // A mask determining what is ground to the character
    [SerializeField] private LayerMask m_WhatIsIce; // A mask determining what is ground to the character
    [SerializeField] private Transform m_GroundCheck;	// A position marking where to check if the player is grounded.
    const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
    private bool m_Grounded;   // Whether or not the player is grounded.
    private bool m_onIce;
    private Vector3 lastPos;
    public UnityEvent OnLandEvent;
    public UnityEvent OnIceEvent;

    void Start()
    {
        _body = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _box = GetComponent<BoxCollider2D>();
        iceSpeed = 20;
        lastPos = transform.position;
    }

    private void Awake()
    {
        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        if (OnIceEvent == null)
            OnIceEvent = new UnityEvent();
    }
    async void Update()
    {
        if (!m_onIce)
        {
            float deltaX = 0f;
            if (Mathf.Abs(Input.GetAxis("Horizontal_originalK") * speed) > Mathf.Abs(Input.GetAxis("Horizontal joyconL joystick") * speed))
            {
                deltaX = Input.GetAxis("Horizontal_originalK") * speed;
            }
            else
            {
                deltaX = Input.GetAxis("Horizontal joyconL joystick") * speed;
            }

            _anim.SetFloat("speed", Mathf.Abs(deltaX));
            if (!Mathf.Approximately(deltaX, 0f))
            {
                transform.localScale = new Vector3(Mathf.Sign(deltaX), 1f, 1f);
            }
            Vector2 movement = new Vector2(deltaX, _body.velocity.y);
            _body.velocity = movement;
        }
        else if (!isMoving())
        {
            float deltaX = 0f;
            if (Mathf.Abs(Input.GetAxis("Horizontal_originalK") * speed) > Mathf.Abs(Input.GetAxis("Horizontal joyconL joystick") * speed))
            {
                deltaX = Input.GetAxis("Horizontal_originalK") * speed;
            }
            else
            {
                deltaX = Input.GetAxis("Horizontal joyconL joystick") * speed;
            }

            _anim.SetFloat("speed", Mathf.Abs(deltaX));
            if (!Mathf.Approximately(deltaX, 0f))
            {
                transform.localScale = new Vector3(Mathf.Sign(deltaX), 1f, 1f);
            }
            Vector2 movement = new Vector2(deltaX, _body.velocity.y);
            _body.velocity = movement;
        }




        if (m_Grounded && !m_onIce && (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Joystick1Button0)))
        {
            _anim.SetTrigger("jumping");
            _body.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
        else if (m_onIce && !isMoving() && (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Joystick1Button0)))
        {
            _anim.SetTrigger("jumping");
            _body.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        if (m_onIce)
        {
            //Debug.Log("estoy en hielo xdd");
        }
        if (m_Grounded)
        {
            //Debug.Log("estoy en el suelo ");
        }
    }

    private void FixedUpdate()
    {
        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        bool wasOnIce = m_onIce;
        m_onIce = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                m_Grounded = true;
                if (!wasGrounded)
                    OnLandEvent.Invoke();
            }
        }

        Collider2D[] collidersIce = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsIce);
        for (int i = 0; i < collidersIce.Length; i++)
        {
            if (collidersIce[i].gameObject != gameObject)
            {
                m_onIce = true;
                if (!wasOnIce)
                    OnLandEvent.Invoke();
            }
        }
    }

    private bool isMoving()
    {
        return _body.velocity.magnitude > 0.3;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log(collision.gameObject.tag.ToString());
        if (collision.CompareTag("0Gravity"))
        {
            _body.gravityScale = 0;
        }
        else {
            _body.gravityScale = 3;
        }
        if (collision.CompareTag("Balanza"))
        {
            collision.GetComponent<contrapeso>().changeWeight(weight);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("0Gravity"))
        {
            _body.gravityScale = 3;
        }
        if (collision.CompareTag("Balanza"))
        {
            collision.GetComponent<contrapeso>().changeWeight(-weight);
        }
    }

}