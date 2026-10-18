using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody2D rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {

        rb = GetComponent<Rigidbody2D>();

    }

    // FixedUpdate is called at a fixed rate, used for physics
    void FixedUpdate() {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        float moveHorizontal = 0f;
        float moveVertical = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveHorizontal -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveHorizontal += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) moveVertical -= 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) moveVertical += 1f;

        Vector2 movement = new Vector2(moveHorizontal, moveVertical).normalized;

        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Coletavel"))
        {
            Destroy(other.gameObject);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ItemColetado();
            }
        }
    }
}
