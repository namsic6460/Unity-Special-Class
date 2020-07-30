using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{

    public float movePower = 4f;
    public float jumpPower = 8f;

    public static float horizontal;
    private float tempHorizontal;

    new Rigidbody2D rigidbody;
    Animator animator;
    Vector3 movement;

    bool isJumping = true;

    float boost = 0.0f;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        Move();
        if (!Jump()) boost = 0;
    }

    private void Move()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        if (!isJumping)
        {
            tempHorizontal = horizontal;

            if (tempHorizontal != 0)
            {
                float yRotation = tempHorizontal > 0 ? 0 : -180;
                transform.rotation = Quaternion.Euler(0, yRotation, 0);
            }
        }

        else
            animator.SetBool("isMoving", false);

        if (!(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.LeftArrow)))
        {
            animator.SetBool("isMoving", false);
            boost = 0;
        }

        else
            animator.SetBool("isMoving", true);

        Vector3 moveVelocity = tempHorizontal * Vector3.right + new Vector3(boost, 0);
        transform.position += moveVelocity * movePower * Time.deltaTime;

        //if(tempHorizontal > 0)
        //    transform.Translate(moveVelocity * movePower * Time.deltaTime, Space.Self);
        //else
        //    transform.Translate(-moveVelocity * movePower * Time.deltaTime, Space.Self);

        //rigidbody.AddForce(moveVelocity * movePower * Time.deltaTime, ForceMode2D.Impulse);

        //float moveVelocity = tempHorizontal + boost;
        //rigidbody.velocity = new Vector3(moveVelocity * movePower * Time.deltaTime, rigidbody.velocity.y);

        float yVelocity = rigidbody.velocity.y;
        if (yVelocity != 0)
        {
            isJumping = true;

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

        if (transform.position.y < -30)
            reload();
    }

    private bool Jump()
    {
        if (isJumping)
            return true;

        if(Input.GetKey(KeyCode.Space))
        {
            isJumping = true;

            if (Mathf.Abs(tempHorizontal) == 1 && (boost == 0 || (tempHorizontal > 0 && boost > 0) || (tempHorizontal < 0 && boost < 0)))
                boost += 0.2f * tempHorizontal;
            else
                boost = 0;

            float yRotation = tempHorizontal > 0 ? 0 : -180;
            transform.rotation = Quaternion.Euler(0, yRotation, 0);

            Vector3 jumpVelocity = new Vector3(0, jumpPower);
            rigidbody.AddForce(jumpVelocity, ForceMode2D.Impulse);
            return true;
        }

        return false;
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
            isJumping = false;
            animator.SetBool("startedJump", false);
            animator.SetBool("isJumping", false);
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Block"))
            isJumping = true;
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Enemy"))
            reload();
    }

    private void reload()
    {
        isJumping = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
