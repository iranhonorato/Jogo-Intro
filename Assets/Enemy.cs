using UnityEngine;

// Inimigo com comportamento configurável pelo Inspector:
// - Parado: fica imóvel, funciona como obstáculo perigoso
// - Patrulha: vai e volta em uma direção pré-determinada
// - Perseguicao: segue o jogador o tempo todo
// - Sentinela: patrulha, mas persegue o jogador quando ele chega perto
//   e volta a patrulhar quando o jogador se afasta
// Ao encostar no jogador, ele perde tempo, é empurrado e fica invencível por um instante.
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

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Color corOriginal;
    private Transform alvo;
    private Vector2 pontoInicial;
    private bool indo = true;
    private bool perseguindo;
    private float pausadoAte;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Garante que o inimigo apareça na tela, mesmo se foi criado como objeto vazio
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        if (sr.sprite == null)
        {
            sr.sprite = CriarSpritePadrao();
            sr.color = corPadrao;
        }
        if (sr.color.a < 0.1f) sr.color = corPadrao;
        if (sr.sortingOrder < 10) sr.sortingOrder = 10; // desenha na frente do cenário
        corOriginal = sr.color;

        // Z = 0 para não ficar atrás da câmera
        Vector3 posicao = transform.position;
        posicao.z = 0f;
        transform.position = posicao;

        // Garante um collider do tipo trigger para detectar o jogador
        // (criado depois do sprite para pegar o tamanho certo)
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
    }

    private static readonly Color corPadrao = new Color(0.55f, 0.2f, 0.8f);
    private static Sprite spritePadrao;

    // Sprite quadrado branco de 1x1 unidade gerado por código
    private static Sprite CriarSpritePadrao()
    {
        if (spritePadrao != null) return spritePadrao;

        const int tamanho = 64;
        Texture2D textura = new Texture2D(tamanho, tamanho);
        Color[] pixels = new Color[tamanho * tamanho];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        textura.SetPixels(pixels);
        textura.Apply();

        spritePadrao = Sprite.Create(textura, new Rect(0, 0, tamanho, tamanho), new Vector2(0.5f, 0.5f), tamanho);
        return spritePadrao;
    }

    void Start()
    {
        pontoInicial = rb.position;

        PlayerMovement jogador = FindAnyObjectByType<PlayerMovement>();
        if (jogador != null) alvo = jogador.transform;
    }

    void FixedUpdate()
    {
        if (Time.time < pausadoAte) return;

        switch (comportamento)
        {
            case Comportamento.Parado:
                break;

            case Comportamento.Patrulha:
                Patrulhar();
                break;

            case Comportamento.Perseguicao:
                Perseguir();
                break;

            case Comportamento.Sentinela:
                AtualizarSentinela();
                break;
        }
    }

    private void Patrulhar()
    {
        Vector2 pontoFinal = pontoInicial + direcaoPatrulha.normalized * distanciaPatrulha;
        Vector2 destino = indo ? pontoFinal : pontoInicial;

        MoverPara(destino);

        if (Vector2.Distance(rb.position, destino) < 0.05f)
        {
            indo = !indo;
        }
    }

    private void Perseguir()
    {
        if (alvo == null) return;
        MoverPara(alvo.position);
    }

    private void AtualizarSentinela()
    {
        float distancia = alvo != null ? Vector2.Distance(rb.position, alvo.position) : Mathf.Infinity;

        if (!perseguindo && distancia <= raioDeteccao) perseguindo = true;
        else if (perseguindo && distancia > raioDesistir) perseguindo = false;

        if (sr != null) sr.color = perseguindo ? corPerseguindo : corOriginal;

        if (perseguindo) Perseguir();
        else Patrulhar();
    }

    private void MoverPara(Vector2 destino)
    {
        rb.MovePosition(Vector2.MoveTowards(rb.position, destino, velocidade * Time.fixedDeltaTime));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TentarAtacar(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        TentarAtacar(other);
    }

    private void TentarAtacar(Collider2D other)
    {
        PlayerMovement jogador = other.GetComponent<PlayerMovement>();
        if (jogador == null) return;

        Vector2 direcao = (Vector2)other.transform.position - rb.position;
        if (direcao == Vector2.zero) direcao = Vector2.up;

        if (jogador.LevarDano(direcao))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PerderTempo(penalidadeTempo);
            }
            pausadoAte = Time.time + pausaAposAtacar;
        }
    }

    // Mostra um contorno roxo na aba Scene, mesmo se o inimigo não tiver sprite
    void OnDrawGizmos()
    {
        Gizmos.color = corPadrao;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }

    // Desenha no editor o caminho da patrulha e o raio de detecção
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
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(transform.position, raioDesistir);
        }
    }
}
