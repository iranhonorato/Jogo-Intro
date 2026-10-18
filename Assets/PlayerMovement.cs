using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;

    [Header("Dano")]
    [Tooltip("Tempo invencível depois de levar dano")]
    public float tempoInvencivel = 1.5f;
    [Tooltip("Força do empurrão ao ser atingido por um inimigo")]
    public float forcaEmpurrao = 10f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 empurrao;
    private float invencivelAte;

    public bool Invencivel => Time.time < invencivelAte;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {

        rb = GetComponent<Rigidbody2D>();
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep; // para continuar detectando inimigos parado
        sr = GetComponent<SpriteRenderer>();

    }

    void Update() {
        // Pisca enquanto estiver invencível
        if (sr != null)
        {
            sr.enabled = !Invencivel || Mathf.Repeat(Time.time, 0.2f) < 0.1f;
        }
    }

    // FixedUpdate is called at a fixed rate, used for physics
    void FixedUpdate() {
        Vector2 movement = Vector2.zero;

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) movement.x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) movement.x += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) movement.y -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) movement.y += 1f;
        }

        Vector2 velocidade = movement.normalized * speed + empurrao;

        // O empurrão perde força rapidamente
        empurrao = Vector2.MoveTowards(empurrao, Vector2.zero, forcaEmpurrao * 4f * Time.fixedDeltaTime);

        rb.MovePosition(rb.position + velocidade * Time.fixedDeltaTime);
    }

    // Chamado pelos inimigos. Retorna false se o jogador estiver invencível.
    public bool LevarDano(Vector2 direcao)
    {
        if (Invencivel) return false;

        invencivelAte = Time.time + tempoInvencivel;
        empurrao = direcao.normalized * forcaEmpurrao;
        return true;
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
