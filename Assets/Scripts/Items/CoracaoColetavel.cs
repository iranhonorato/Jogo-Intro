using UnityEngine;

// Item de coração: recupera vida de quem encostar nele.
// Depende só do componente Vida (e não do PlayerMovement), então funciona com qualquer objeto que tenha vida.
public class CoracaoColetavel : MonoBehaviour
{
    [Tooltip("Quantos corações este item recupera")]
    public int cura = 1;

    private static readonly Color Cor = new Color(1f, 0.25f, 0.35f);

    private Vector3 escalaBase;

    void Awake()
    {
        // Permite criar o item a partir de um objeto vazio: adiciona sprite e collider se faltarem
        if (!TryGetComponent(out SpriteRenderer sr)) sr = gameObject.AddComponent<SpriteRenderer>();
        if (sr.sprite == null)
        {
            sr.sprite = SpriteFactory.Coracao();
            sr.color = Cor;
        }
        sr.sortingOrder = OrdemDesenho.Itens; // na frente do cenário, atrás do jogador

        if (!TryGetComponent(out Collider2D col)) col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true; // detecta o jogador sem bloqueá-lo

        escalaBase = transform.localScale;
    }

    void Update()
    {
        // Pulsa levemente para chamar a atenção do jogador
        transform.localScale = escalaBase * (1f + 0.08f * Mathf.Sin(Time.time * 5f));
    }

    // Stay além do Enter: se o jogador estava com a vida cheia em cima do coração
    // e levar dano, o coração é coletado sem ele precisar sair e voltar
    void OnTriggerEnter2D(Collider2D other) => TentarCurar(other);
    void OnTriggerStay2D(Collider2D other) => TentarCurar(other);

    private void TentarCurar(Collider2D other)
    {
        if (!other.TryGetComponent(out Vida vida)) return;

        // Com a vida cheia, Curar retorna false e o coração fica no mapa para depois
        if (vida.Curar(cura)) Destroy(gameObject);
    }
}
