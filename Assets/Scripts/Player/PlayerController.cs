using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 40f;
    public Animator animator;
    private Rigidbody2D rb;
    public Vector2 moveInput;
    public InputSystem_Actions controls;
    private DynamicJoystick joystick;

    private void Awake()
    {
        controls = new InputSystem_Actions();
        rb = GetComponent<Rigidbody2D>();
        joystick = FindAnyObjectByType<DynamicJoystick>();
    }

    private void OnEnable()
    {
        controls.Player.Enable();
        controls.Player.Move.performed += ctx => { moveInput = ctx.ReadValue<Vector2>(); SetWalkAnimation(); };
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    private void FixedUpdate()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        playerPC();
#else
        playerMobile();
#endif
    }

    void playerPC()
    {
        Vector2 input = moveInput;

        // Verifica si el joystick está activo y usa su input
        if (joystick != null && (Mathf.Abs(joystick.Horizontal) > 0.1f || Mathf.Abs(joystick.Vertical) > 0.1f))
        {
            if (Mouse.current.leftButton.isPressed) {
                joystick.gameObject.SetActive(true);
                input = new Vector2(joystick.Horizontal, joystick.Vertical);
            }
            else
            {
               // joystick.gameObject.SetActive(false); // OCULTAR JOYSTICK SI NO SE USA
            }
        }

        rb.MovePosition(rb.position + input * speed * Time.fixedDeltaTime);
    }

    void playerMobile()
    {
        Vector2 input = new Vector2(joystick.Horizontal, joystick.Vertical);
        rb.MovePosition(rb.position + input * speed * Time.fixedDeltaTime);
    }

    void SetWalkAnimation() 
    {
        if (moveInput.y == 0 && moveInput.x != 0)
        {
            animator.SetLayerWeight(1, 1);
            animator.SetLayerWeight(0, 0);
        }
        else 
        {
            animator.SetLayerWeight(1, 0);
            animator.SetLayerWeight(0, 1);
        }

        if (moveInput.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        if (moveInput.y > 0)
        {
            animator.SetBool("walkingForward", false);
        }
        else if(moveInput.y < 0)
        {
            animator.SetBool("walkingForward", true);
        }

    }
}
