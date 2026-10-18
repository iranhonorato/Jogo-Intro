using UnityEngine;

// Inimigo que tira tempo do jogador ao encostar nele.
//
// O movimento usa o padrão Strategy (ver ComportamentosInimigo.cs): o Enemy não sabe
// COMO se mover; ele só escolhe a estratégia certa conforme o Inspector e a executa.
// O dano usa a interface IDanificavel, então o Enemy não depende do PlayerMovement.
[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    public enum Comportamento { Parado, Patrulha, Perseguicao, Sentinela }

    [Header("Comportamento")]
    public Comportamento comportamento = Comportamento.Patrulha;
    public float velocidade = 2.5f;

    [Header("Patrulha (vai e volta)")]
    public Vector2 direcaoPatrulha = Vector2.right;
    public float distanciaPatrulha = 4f;

    [Header("Sentinela")]
    [Tooltip("Distância em que o inimigo começa a perseguir o jogador")]
    public float raioDeteccao = 4f;
    [Tooltip("Distância em que o inimigo desiste e volta a patrulhar")]
    public float raioDesistir = 6f;
    public Color corPerseguindo = new Color(1f, 0.5f, 0f);

    [Header("Dano")]
    [Tooltip("Segundos tirados do tempo restante ao encostar no jogador")]
    public float penalidadeTempo = 5f;
    [Tooltip("Tempo que o inimigo fica parado depois de acertar o jogador")]
    public float pausaAposAtacar = 0.5f;

    private static readonly Color CorPadrao = new Color(0.55f, 0.2f, 0.8f);

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Color corOriginal;
    private Vector2 pontoInicial;
    private float pausadoAte;
    private IComportamentoInimigo estrategia;

    // Usados pelas estratégias
    public Vector2 Posicao => rb.position;
    public Transform Alvo { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // movido só por código (sem gravidade nem empurrões)
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // movimento mais suave na tela

        GarantirVisual();
        GarantirCollider(); // depois do visual, para o collider pegar o tamanho do sprite
    }

    void Start()
    {
        // Feito no Start (e não no Awake) para respeitar valores definidos logo após a criação,
        // como faz a EnemyFactory
        pontoInicial = rb.position;

        PlayerMovement jogador = FindAnyObjectByType<PlayerMovement>();
        if (jogador != null) Alvo = jogador.transform;

        estrategia = CriarEstrategia();
    }

    // Traduz a opção do Inspector para a estratégia correspondente
    private IComportamentoInimigo CriarEstrategia()
    {
        switch (comportamento)
        {
            case Comportamento.Patrulha:
                return new ComportamentoPatrulha(pontoInicial, direcaoPatrulha, distanciaPatrulha);
            case Comportamento.Perseguicao:
                return new ComportamentoPerseguicao();
            case Comportamento.Sentinela:
                var patrulha = new ComportamentoPatrulha(pontoInicial, direcaoPatrulha, distanciaPatrulha);
                return new ComportamentoSentinela(patrulha, raioDeteccao, raioDesistir);
            default:
                return new ComportamentoParado();
        }
    }

    void FixedUpdate()
    {
        if (Time.time < pausadoAte) return; // parado logo depois de atacar
        estrategia.Atualizar(this);
    }

    public void MoverPara(Vector2 destino)
    {
        rb.MovePosition(Vector2.MoveTowards(rb.position, destino, velocidade * Time.fixedDeltaTime));
    }

    public void MostrarAlerta(bool alerta)
    {
        sr.color = alerta ? corPerseguindo : corOriginal;
    }

    // Enter: no primeiro toque. Stay: continua tentando enquanto encostado
    // (o dano só acontece de novo quando a invencibilidade do jogador acabar).
    void OnTriggerEnter2D(Collider2D other) => TentarAtacar(other);
    void OnTriggerStay2D(Collider2D other) => TentarAtacar(other);

    private void TentarAtacar(Collider2D other)
    {
        if (!other.TryGetComponent(out IDanificavel alvoDano)) return;

        Vector2 direcao = (Vector2)other.transform.position - rb.position;
        if (!alvoDano.ReceberDano(direcao)) return; // alvo invencível: nada acontece

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PerderTempo(penalidadeTempo);
        }
        pausadoAte = Time.time + pausaAposAtacar;
    }

    // ---------- Configuração automática ----------

    // Permite criar o inimigo a partir de um objeto vazio: adiciona sprite e cor se faltarem
    private void GarantirVisual()
    {
        if (!TryGetComponent(out sr)) sr = gameObject.AddComponent<SpriteRenderer>();

        if (sr.sprite == null)
        {
            sr.sprite = SpriteFactory.Quadrado();
            sr.color = CorPadrao;
        }
        sr.sortingOrder = OrdemDesenho.Inimigos; // na frente do cenário, atrás do jogador
        corOriginal = sr.color;
    }

    private void GarantirCollider()
    {
        if (!TryGetComponent(out Collider2D col)) col = gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true; // detecta o jogador sem bloqueá-lo fisicamente
    }

    // ---------- Ajudas visuais no editor (aba Scene) ----------

    // Contorno roxo sempre visível, mesmo se o inimigo não tiver sprite
    void OnDrawGizmos()
    {
        Gizmos.color = CorPadrao;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }

    // Com o inimigo selecionado: rota da patrulha (amarelo) e raios da sentinela
    void OnDrawGizmosSelected()
    {
        Vector2 inicio = Application.isPlaying ? pontoInicial : (Vector2)transform.position;

        if (comportamento == Comportamento.Patrulha || comportamento == Comportamento.Sentinela)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(inicio, inicio + direcaoPatrulha.normalized * distanciaPatrulha);
        }

        if (comportamento == Comportamento.Sentinela)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, raioDeteccao);
            Gizmos.color = corPerseguindo;
            Gizmos.DrawWireSphere(transform.position, raioDesistir);
        }
    }
}
