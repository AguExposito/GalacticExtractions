using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 40f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
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
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
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
                joystick.gameObject.SetActive(false); // OCULTAR JOYSTICK SI NO SE USA
            }
        }

        rb.MovePosition(rb.position + input * speed * Time.fixedDeltaTime);
    }

    void playerMobile()
    {
        Vector2 input = new Vector2(joystick.Horizontal, joystick.Vertical);
        rb.MovePosition(rb.position + input * speed * Time.fixedDeltaTime);
    }
}
