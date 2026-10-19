using UnityEngine;
using UnityEngine.InputSystem;

// Movimento do jogador: lê o teclado e move o corpo pela física.
//
// As outras responsabilidades do jogador ficam em componentes próprios:
// - DanoJogador: reação a dano (invencibilidade, piscar, empurrão);
// - Vida: corações;
// - a coleta fica nos próprios itens (ItemColetavel, CoracaoColetavel).
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;

    // O empurrão some em cerca de 0,25s
    private const float FatorDesaceleracaoEmpurrao = 4f;

    private Rigidbody2D rb;
    private Vector2 empurrao;
    private float desaceleracaoEmpurrao;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Sem isso, o Rigidbody "dorme" quando o jogador fica parado e
        // deixa de detectar um inimigo parado encostado nele
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        rb.freezeRotation = true; // não gira ao bater nas quinas do labirinto
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // não "pula" paredes finas em alta velocidade
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // movimento suave na tela entre os passos da física

        if (TryGetComponent(out SpriteRenderer sr)) sr.sortingOrder = OrdemDesenho.Jogador; // por cima de itens e inimigos
    }

    // FixedUpdate roda em intervalos fixos: é onde se mexe na física
    void FixedUpdate()
    {
        Vector2 velocidade = LerDirecao() * speed + empurrao;
        empurrao = Vector2.MoveTowards(empurrao, Vector2.zero, desaceleracaoEmpurrao * Time.fixedDeltaTime);

        // Define a velocidade e deixa a física mover o corpo: assim as paredes bloqueiam o jogador.
        // (MovePosition é feito para corpos Kinematic; em um corpo Dynamic, como o jogador,
        // ele força a posição e pode atravessar paredes.)
        rb.linearVelocity = velocidade;
    }

    // Soma um impulso ao movimento, que perde força rapidamente (usado pelo DanoJogador)
    public void Empurrar(Vector2 impulso)
    {
        empurrao = impulso;
        desaceleracaoEmpurrao = impulso.magnitude * FatorDesaceleracaoEmpurrao;
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
}
