using UnityEngine;

// Padrão Factory: concentra a criação dos itens de coração, como a EnemyFactory faz com os inimigos.
public static class ItemFactory
{
    private const float EscalaCoracao = 0.6f;

    public static CoracaoColetavel CriarCoracao(Vector2 posicao)
    {
        GameObject go = new GameObject("Coracao");
        go.transform.position = posicao;
        go.transform.localScale = Vector3.one * EscalaCoracao; // antes do AddComponent: o Awake guarda essa escala

        return go.AddComponent<CoracaoColetavel>();
    }

    // 3 corações usados quando a cena não tem nenhum.
    // Posições pensadas para a arena atual (x entre -7 e 7, y entre -4 e 4),
    // perto de inimigos para que pegar um coração tenha algum risco.
    public static void CriarCoracoesPadrao()
    {
        CriarCoracao(new Vector2(-6.2f, -0.5f));
        CriarCoracao(new Vector2(-1.5f, 3.4f));
        CriarCoracao(new Vector2(5.8f, -1f));
    }
}
