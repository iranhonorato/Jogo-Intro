using UnityEngine;

// Padrão Factory: concentra em um só lugar o "como montar um inimigo".
// Quem precisa de inimigos (ex.: GameManager) só diz o que quer, sem conhecer os detalhes.
public static class EnemyFactory
{
    private const float Escala = 0.7f;

    public static Enemy Criar(string nome, Vector2 posicao, Enemy.Comportamento comportamento,
                              float velocidade, Vector2 direcaoPatrulha = default, float distanciaPatrulha = 0f)
    {
        GameObject go = new GameObject(nome);
        go.transform.position = posicao;
        go.transform.localScale = Vector3.one * Escala;

        // O Awake do Enemy roda aqui dentro; os campos abaixo são lidos depois, no Start
        Enemy inimigo = go.AddComponent<Enemy>();
        inimigo.comportamento = comportamento;
        inimigo.velocidade = velocidade;
        inimigo.direcaoPatrulha = direcaoPatrulha;
        inimigo.distanciaPatrulha = distanciaPatrulha;
        return inimigo;
    }

    // Um inimigo de cada comportamento, usado quando a cena não tem inimigos próprios.
    // Posições pensadas para a arena atual (x entre -7 e 7, y entre -4 e 4),
    // longe do ponto inicial do jogador (canto superior esquerdo).
    public static void CriarConjuntoPadrao()
    {
        Criar("Inimigo Parado", new Vector2(0f, 0.5f), Enemy.Comportamento.Parado, 0f);
        Criar("Inimigo Patrulha Horizontal", new Vector2(-5f, -2.5f), Enemy.Comportamento.Patrulha, 3f, Vector2.right, 10f);
        Criar("Inimigo Patrulha Vertical", new Vector2(3f, -3.2f), Enemy.Comportamento.Patrulha, 2.5f, Vector2.up, 6.4f);
        Criar("Inimigo Sentinela", new Vector2(5.5f, 2.5f), Enemy.Comportamento.Sentinela, 2.5f, Vector2.left, 4f);
        Criar("Inimigo Perseguidor", new Vector2(6f, -3.3f), Enemy.Comportamento.Perseguicao, 1.5f);
    }
}
