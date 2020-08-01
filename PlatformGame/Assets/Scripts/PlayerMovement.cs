using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public GameObject Canvas;

    public float movePower = 4f;
    public float jumpPower = 8f;

    public float horizontal;
    private float tempHorizontal;

    private Transform JoystickBackGround;
    private Transform Joystick;
    private Transform JumpButton;
    private Text BoostText;

    private float canvasCenterX;
    private float joystickRadius;
    private float jumpButtonRadius;
    private Vector3 center;
    private Vector3 axis;

    private new Rigidbody2D rigidbody;
    private Animator animator;
    private Vector3 movement;

    private bool isClicking = false;

    private float boost = 0.0f;
    private bool checkTick = false;
    private int boostTick = 0;

    private List<Touch> touches;

    private void Start()
    {
        Input.multiTouchEnabled = true;

        JoystickBackGround = Canvas.transform.Find("MoveArea");
        Joystick = JoystickBackGround.Find("Joystick");
        JumpButton = Canvas.transform.Find("JumpButton");
        BoostText = Canvas.transform.Find("BoostText").GetComponent<Text>();

        rigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        horizontal = 0;
        canvasCenterX = Canvas.transform.position.x / 2;
        canvasCenterX = Canvas.GetComponent<RectTransform>().position.x / 2;
        joystickRadius = JoystickBackGround.GetComponent<RectTransform>().transform.position.x / 2;
        jumpButtonRadius = JumpButton.GetComponent<RectTransform>().transform.position.x / 2;

        center = JoystickBackGround.transform.position;
        Joystick.position = center;
        axis = Vector3.zero;
    }

    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Space))
            Jump();
        if (isClicking)
            Jump();

        Move();

        if (checkTick)
        {
            if (boostTick > 0)
                boostTick--;

            else
            {
                BoostText.text = "Boost : 0";
                boost = 0;
            }
        }

        bool joystickTemp = false;
        bool jumpButtonTemp = false;

        touches = new List<Touch>(Input.touches);
        foreach (Touch touch in touches)
        {
            Vector3 touchPos = new Vector3(touch.position.x, touch.position.y);

            if (touchPos.x < canvasCenterX)
            {
                joystickTemp = true;

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                    case TouchPhase.Stationary:
                    case TouchPhase.Moved:
                        joystickPointerDown(touch);
                        break;
                        
                    case TouchPhase.Ended:
                        joystickPointerUp();
                        break;
                }
            }

            else if (Vector3.Distance(touchPos, JumpButton.transform.position) < jumpButtonRadius)
            {
                jumpButtonTemp = true;

                switch (touch.phase)
                {
                    case TouchPhase.Stationary:
                    case TouchPhase.Began:
                    case TouchPhase.Moved:
                        isClicking = true;
                        break;
                        
                    case TouchPhase.Ended:
                        isClicking = false;
                        break;
                }
            }
        }  

        if (!joystickTemp)
            joystickPointerUp();
        if (!jumpButtonTemp)
            isClicking = false;
    }

    private void Move()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        horizontal = horizontal > 0 ? 1 : horizontal < 0 ? -1 : 0;

        if (rigidbody.velocity.y == 0)
        {
            tempHorizontal = horizontal;

            if (tempHorizontal != 0)
            {
                float yRotation = tempHorizontal > 0 ? 0 : -180;
                transform.rotation = Quaternion.Euler(0, yRotation, 0);
                animator.SetBool("isMoving", true);
            }

            else
                animator.SetBool("isMoving", false);
        }

        else
            animator.SetBool("isMoving", false);

        Vector3 moveVelocity = tempHorizontal * Vector3.right + new Vector3(boost, 0);
        transform.position += moveVelocity * movePower * Time.deltaTime;

        float yVelocity = rigidbody.velocity.y;
        if (yVelocity != 0)
        {
            if (yVelocity > 0)
            {
                animator.SetBool("startedJump", true);
                animator.SetBool("isJumping", false);
            }
            else
            {
                animator.SetBool("startedJump", false);
                animator.SetBool("isJumping", true);
            }
        }

        else
            checkTick = true;

        if (transform.position.y < -30)
            reload();
    }

    private void Jump()
    {
        if (rigidbody.velocity.y != 0)
            return;

        if (Mathf.Abs(tempHorizontal) == 1 && (boost == 0 || (tempHorizontal > 0 && boost > 0) || (tempHorizontal < 0 && boost < 0)))
        {
            boost += 0.5f * tempHorizontal;
            BoostText.text = "Boost : " + boost;

            checkTick = false;
            boostTick = 1;
        }

        else
            boost = 0;

        float yRotation = tempHorizontal > 0 ? 0 : -180;
        transform.rotation = Quaternion.Euler(0, yRotation, 0);

        Vector3 jumpVelocity = new Vector3(0, jumpPower);
        rigidbody.AddForce(jumpVelocity, ForceMode2D.Impulse);
    }

    private void joystickPointerDown(Touch touch)
    {
        Vector3 fingerPos = touch.position;

        if (Vector3.Distance(touch.position, JoystickBackGround.position) <= joystickRadius)
            Joystick.position = fingerPos;
        else
            Joystick.position = center + axis * joystickRadius;

        axis = (fingerPos - center).normalized;

        horizontal = axis.x;
    }

    private void joystickPointerUp()
    {
        Joystick.position = center;
        axis = Vector3.zero;
        horizontal = 0;
    }

    private void reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        transform.position += new Vector3(0, 0.0001f);
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Block"))
        {
            tempHorizontal = horizontal;

            animator.SetBool("startedJump", false);
            animator.SetBool("isJumping", false);
        }
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Enemy"))
            reload();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Enemy"))
            reload();
    }

}
