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
    private Vida vida;

    public bool Invencivel => Time.time < invencivelAte;

    // Os corações ficam no componente Vida. Se o Player da cena ainda não tiver
    // esse componente, ele é adicionado na primeira vez que alguém pedir.
    public Vida Vida
    {
        get
        {
            if (vida == null && !TryGetComponent(out vida)) vida = gameObject.AddComponent<Vida>();
            return vida;
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Sem isso, o Rigidbody "dorme" quando o jogador fica parado e
        // deixa de detectar um inimigo parado encostado nele
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        rb.freezeRotation = true; // não gira ao bater nas quinas do labirinto
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // não "pula" paredes finas em alta velocidade
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // movimento suave na tela entre os passos da física
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = OrdemDesenho.Jogador; // por cima de itens e inimigos
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

        // Define a velocidade e deixa a física mover o corpo: assim as paredes bloqueiam o jogador.
        // (MovePosition é feito para corpos Kinematic; em um corpo Dynamic, como o jogador,
        // ele força a posição e pode atravessar paredes.)
        rb.linearVelocity = velocidade;
    }

    // Coloca o jogador em um ponto do mapa (usado pelo ConstrutorFase). Mantém o Z original.
    public void Posicionar(Vector2 posicao)
    {
        transform.position = new Vector3(posicao.x, posicao.y, transform.position.z);
        rb.position = posicao;
        rb.linearVelocity = Vector2.zero; // não "carrega" o movimento da fase anterior
        empurrao = Vector2.zero;
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
        if (Invencivel || Vida.Morto) return false;

        invencivelAte = Time.time + tempoInvencivel;

        // Empurra o personagem caso ele colida com um inimigo
        empurrao = (direcao == Vector2.zero ? Vector2.up : direcao.normalized) * forcaEmpurrao;

        // Perde um coração (se for o último, a Vida avisa o GameManager pelo evento Morreu)
        Vida.Perder(1);
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
