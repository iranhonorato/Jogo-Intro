using UnityEngine;
using UnityEngine.InputSystem;

// Jogador: movimento, coleta de itens e reação a dano (empurrão + invencibilidade temporária).
// Implementa IDanificavel para que os inimigos causem dano sem depender desta classe.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour, IDanificavel
{
    public float speed = 5f;

    [Header("Dano")]
    [Tooltip("Tempo invencível depois de levar dano")]
    public float tempoInvencivel = 1.5f;
    
    [Tooltip("Força do empurrão ao ser atingido por um inimigo")]
    public float forcaEmpurrao = 10f;

    private const float IntervaloPiscar = 0.2f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 empurrao;
    private float invencivelAte;

    public bool Invencivel => Time.time < invencivelAte;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Sem isso, o Rigidbody "dorme" quando o jogador fica parado e
        // deixa de detectar um inimigo parado encostado nele
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Pisca (liga/desliga o sprite) após recber dano
        if (sr != null)
        {
            sr.enabled = !Invencivel || Mathf.Repeat(Time.time, IntervaloPiscar) < IntervaloPiscar / 2f;
        }
    }

    // FixedUpdate roda em intervalos fixos: é onde se mexe na física
    void FixedUpdate()
    {
        Vector2 velocidade = LerDirecao() * speed + empurrao;

        // O empurrão perde força rapidamente (some em cerca de 0,25s)
        empurrao = Vector2.MoveTowards(empurrao, Vector2.zero, forcaEmpurrao * 4f * Time.fixedDeltaTime);

        rb.MovePosition(rb.position + velocidade * Time.fixedDeltaTime);
    }

    // Lê WASD ou setas. Normalizado para a diagonal não ser mais rápida que as retas.
    private static Vector2 LerDirecao()
    {
        Keyboard teclado = Keyboard.current;
        if (teclado == null) return Vector2.zero;

        Vector2 direcao = Vector2.zero;
        if (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed) direcao.x -= 1f;
        if (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed) direcao.x += 1f;
        if (teclado.sKey.isPressed || teclado.downArrowKey.isPressed) direcao.y -= 1f;
        if (teclado.wKey.isPressed || teclado.upArrowKey.isPressed) direcao.y += 1f;
        return direcao.normalized;
    }

    public bool ReceberDano(Vector2 direcao)
    {
        // Imediatamente após recber dano o personagem fica "Invencivel"
        if (Invencivel) return false;

        invencivelAte = Time.time + tempoInvencivel;

        // Empurra o personagem caso ele colida com um inimigo
        empurrao = (direcao == Vector2.zero ? Vector2.up : direcao.normalized) * forcaEmpurrao;
        return true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(GameManager.TagColetavel)) return;

        Destroy(other.gameObject);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ItemColetado();
        }
    }
}
