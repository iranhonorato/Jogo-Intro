using UnityEngine;

// Padrão Factory: concentra em um só lugar o "como montar um inimigo".
// Quem precisa de inimigos (ex.: ConstrutorFase) só diz o que quer, sem conhecer os detalhes.
public static class EnemyFactory
{
    private const float Escala = 0.4f; // cabe com folga nos corredores do labirinto (1 unidade)

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
}
