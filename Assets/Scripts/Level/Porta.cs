using UnityEngine;

// Porta de saída da fase. Fica trancada (marrom) até o jogador coletar todos os itens;
// então abre (verde, pulsando) e, ao ser tocada, leva à próxima fase.
//
// Padrão Observer: a porta não fica perguntando ao GameManager se pode abrir;
// ela escuta o evento ItensConcluidos e reage quando ele acontece.
public class Porta : MonoBehaviour
{
    private static readonly Color CorTrancada = new Color(0.55f, 0.35f, 0.18f);
    private static readonly Color CorAberta = new Color(0.3f, 1f, 0.45f);

    private SpriteRenderer sr;
    private Vector3 escalaBase;

    public bool Aberta { get; private set; }

    void Awake()
    {
        if (!TryGetComponent(out sr)) sr = gameObject.AddComponent<SpriteRenderer>();
        if (sr.sprite == null) sr.sprite = SpriteFactory.Porta();
        sr.color = CorTrancada;
        sr.sortingOrder = OrdemDesenho.Itens;

        // Collider depois do sprite (senão ele nasce com tamanho zero)
        if (!TryGetComponent(out Collider2D col)) col = gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true; // detecta o jogador sem bloqueá-lo

        escalaBase = transform.localScale;
    }

    void Start()
    {
        GameManager jogo = GameManager.Instance;
        if (jogo == null) return;

        jogo.ItensConcluidos += Abrir;
        if (jogo.TodosItensColetados) Abrir(); // caso a fase não tenha itens
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.ItensConcluidos -= Abrir;
    }

    void Update()
    {
        // Aberta, pulsa para chamar a atenção
        if (Aberta) transform.localScale = escalaBase * (1f + 0.1f * Mathf.Sin(Time.time * 6f));
    }

    private void Abrir()
    {
        Aberta = true;
        sr.color = CorAberta;
    }

    // Stay além do Enter: se o jogador já estiver em cima da porta quando ela abrir
    void OnTriggerEnter2D(Collider2D other) => TentarEntrar(other);
    void OnTriggerStay2D(Collider2D other) => TentarEntrar(other);

    private void TentarEntrar(Collider2D other)
    {
        if (!Aberta || !other.TryGetComponent(out PlayerMovement _)) return;

        if (GameManager.Instance != null) GameManager.Instance.EntrarNaPorta();
    }
}
