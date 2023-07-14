using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MovementConnor : MonoBehaviour
{
    public float speed = 2f; 
    public float jumpForce = 5.0f;
    public float gravityScale = 10;
    public float fallingGravityScale = 40;
    public float friccionHielo = 0.05f;    
    public float timeSliding = 0.8f;
    public GameObject gunSight;

    private Rigidbody2D _body;
    private Animator _anim;
    private BoxCollider2D _box;

    [SerializeField] private LayerMask m_WhatIsGround; // A mask determining what is ground to the character
    [SerializeField] private LayerMask m_WhatIsIce; // A mask determining what is ground to the character
    [SerializeField] private Transform m_GroundCheck;	// A position marking where to check if the player is grounded.
    const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
	public bool m_Grounded;   // Whether or not the player is grounded.
    public bool m_onIce;
    public bool m_onIceDiagonal;
    public bool movementAllowed;
    public bool jumpAllowed;
    private bool aiming = false;    
    public   bool first;
    private bool enRampa = false;
    [SerializeField] private float rotateGunsight = 0.0f;
    [SerializeField] private float lookingAtRight = 1.0f;
    private bool onLadder = false;
    private bool climbing = false;
    private float last_speed;
    public UnityEvent OnLandEvent;
    public UnityEvent OnIceEvent;

    Vector2 movement = new Vector2(0,0);

    void Start()
    {
        _body = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _box = GetComponent<BoxCollider2D>();
        
        movementAllowed = true;
        jumpAllowed = true;
        first = false;
    }

    private void Awake() {
        if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();

        if (OnIceEvent == null)
			OnIceEvent = new UnityEvent();
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.Joystick2Button5))
        {
            aiming = true;
            _body.velocity = new Vector2(0, _body.velocity.y);
            gunSight.SetActive(true);
            float verticalAxis = Input.GetAxis("Vertical joyconR joystick");
            if(Math.Abs(verticalAxis) > 0.4f)
            {
                float sign = Math.Sign(verticalAxis);
                rotateGunsight += sign * 100f * Time.deltaTime;
                if (Math.Abs(rotateGunsight) > 90) rotateGunsight = 90 * sign;
                gunSight.transform.rotation = Quaternion.Euler(0, 0, - rotateGunsight * lookingAtRight);
            }
        }
        else
        {
            aiming = false;
            gunSight.SetActive(false);
        }

        if (!m_Grounded && !m_onIceDiagonal && !m_onIce){
            movementAllowed = true;
            jumpAllowed = false;
            first = false;
        }
        else if(m_onIceDiagonal){
            movementAllowed = false;
            jumpAllowed = true;
            first = false;
        }
        else if(!m_onIce){
            first = false;
            movementAllowed = true;
            jumpAllowed = true;
        }
        else {
            movementAllowed = true;
            jumpAllowed = true;
        }

        if (movementAllowed && !aiming)
        {
            float deltaX = 0f;
            if (Mathf.Abs(Input.GetAxis("Horizontal_originalC") * speed) > Mathf.Abs(Input.GetAxis("Horizontal joyconR joystick") * speed))
            {
                deltaX = Input.GetAxis("Horizontal_originalC") * speed;
            }
            else
            {
                deltaX = Input.GetAxis("Horizontal joyconR joystick") * speed;
            }
            if (deltaX > 0) lookingAtRight = 1.0f;
            else if (deltaX < 0) lookingAtRight = -1.0f;
            _anim.SetFloat("speed", Mathf.Abs(deltaX));
            if (!Mathf.Approximately(deltaX, 0f))
            {
                transform.localScale = new Vector3(Mathf.Sign(deltaX), 1f, 1f);
            }
            if(!m_onIce || first){
                 movement = new Vector2(deltaX, _body.velocity.y);
                 last_speed = movement.x;
                 _body.velocity = movement;
                 
            }
            else{
                movement = new Vector2(last_speed, _body.velocity.y);
                if(friccionHielo > 0.1f){friccionHielo = 0.1f;}
                else if(friccionHielo < 0.01f){friccionHielo = 0.01f;}
                if(last_speed > 0 ){last_speed = last_speed - friccionHielo;}
                if(last_speed < 0 ){last_speed = last_speed + friccionHielo;}
                Invoke("KeepMoving",timeSliding);
                
            }
            _body.velocity = movement;
        }


        if (onLadder && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Joystick2Button0)) && Time.timeScale != 0)
        {
            if (!climbing)
            {
                climbing = true;
                _body.velocity = new Vector2(0, 0);
            }

            _body.gravityScale = 0;

            transform.position = new Vector3(transform.position.x, transform.position.y + 0.02f, transform.position.z);
        }
        else
        {
            climbing = false;
        }

        if (!climbing && jumpAllowed && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Joystick2Button0) && Time.timeScale != 0))
        {
            _anim.SetTrigger("jumping");
            _body.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        if(_body.velocity.y >= 0 && !climbing && !enRampa)
        {
            _body.gravityScale = gravityScale;
        }
        else if (_body.velocity.y < 0 && !climbing && !enRampa)
        {
            _body.gravityScale = fallingGravityScale;
        }
        

       /* if (m_onIce)
        {
            Debug.Log("estoy en hielo xdd");
        }
        if (m_Grounded)
        {
            Debug.Log("estoy en el suelo ");
        }
        if(m_onIceDiagonal){
            Debug.Log("estoy en rampa de hielo");
        }*/
    }

    private void FixedUpdate() {
        bool wasGrounded = m_Grounded;
		m_Grounded = false;

        bool wasOnIce = m_onIce;
		m_onIce = false; 
        m_onIceDiagonal = false;
       

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
            if (collidersIce[i].gameObject.tag == "DiagonalGroundIce")
            {
                m_onIceDiagonal = true;
            }
		}
    }

    private void KeepMoving() {
        first = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log(collision.gameObject.tag.ToString());
        if (collision.CompareTag("0Gravity"))
        {
            _body.gravityScale = 0;
            enRampa = true;
        }
        else
        {
            _body.gravityScale = gravityScale;
            
        }
        if (collision.CompareTag("Ladder"))
        {
            onLadder = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("0Gravity"))
        {
            _body.gravityScale = gravityScale;
            enRampa = false;
        }
        if (collision.CompareTag("Ladder"))
        {
            onLadder = false;
        }
    }
    /*private (Vector2, Vector2) getGroundCheckCorners()
    {
        Vector3 max = _box.bounds.max;
        Vector3 min = _box.bounds.min;
        Vector2 corner1 = new Vector2(max.x, min.y - .1f);
        Vector2 corner2 = new Vector2(min.x, min.y - .2f);
        return (corner1, corner2);
    }
    public bool grounded
    {
        get
        {
            var (corner1, corner2) = getGroundCheckCorners();
            Collider2D hit = Physics2D.OverlapArea(corner1, corner2);
            return (hit != null);
        }
    }*/

}
