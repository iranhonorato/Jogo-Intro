using System.Collections.Generic;
using UnityEngine;

// Grade do labirinto: sabe onde há parede e calcula caminhos pelos corredores.
//
// Os inimigos são kinematic (movidos por código), então a física não os impede de atravessar
// paredes. Por isso eles pedem ao Labirinto o próximo ponto do caminho, em vez de andar
// em linha reta até o destino.
public class Labirinto
{
    private static readonly Vector2Int[] Vizinhos =
        { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    // Distância máxima do centro da célula para considerar o inimigo "alinhado" antes de virar
    private const float Alinhamento = 0.01f;

    private readonly bool[,] parede; // [coluna, linha]; a linha 0 é a de baixo
    private readonly Vector2 origem; // canto inferior esquerdo da grade no mundo
    private readonly float tamanhoCelula;

    // Reaproveitados entre as buscas para não gerar lixo de memória a cada frame
    private readonly int[] proximoPasso;
    private readonly Queue<int> fila = new Queue<int>();

    public int Colunas { get; }
    public int Linhas { get; }

    public Labirinto(bool[,] parede, Vector2 origem, float tamanhoCelula)
    {
        this.parede = parede;
        this.origem = origem;
        this.tamanhoCelula = tamanhoCelula;
        Colunas = parede.GetLength(0);
        Linhas = parede.GetLength(1);
        proximoPasso = new int[Colunas * Linhas];
    }

    // Fora da grade conta como parede
    public bool EhParede(Vector2Int celula)
    {
        return celula.x < 0 || celula.y < 0 || celula.x >= Colunas || celula.y >= Linhas
            || parede[celula.x, celula.y];
    }

    public Vector2Int CelulaDe(Vector2 posicao)
    {
        return new Vector2Int(Mathf.FloorToInt((posicao.x - origem.x) / tamanhoCelula),
                              Mathf.FloorToInt((posicao.y - origem.y) / tamanhoCelula));
    }

    public Vector2 CentroDe(Vector2Int celula)
    {
        return origem + new Vector2(celula.x + 0.5f, celula.y + 0.5f) * tamanhoCelula;
    }

    // Anda a partir de 'celula' na 'direcao' e retorna a última célula livre antes de uma parede.
    // Usado para descobrir até onde vai o corredor de uma patrulha.
    public Vector2Int UltimaLivre(Vector2Int celula, Vector2Int direcao)
    {
        while (!EhParede(celula + direcao)) celula += direcao;
        return celula;
    }

    // Ponto para onde andar agora, saindo de 'posicao' rumo a 'destino' pelos corredores.
    public Vector2 ProximoPasso(Vector2 posicao, Vector2 destino)
    {
        Vector2Int atual = CelulaDe(posicao);
        Vector2Int alvo = CelulaDe(destino);

        // Já na célula do destino (ou fora da grade): vai direto
        if (atual == alvo || EhParede(atual) || EhParede(alvo)) return destino;
        if (!BuscarProximaCelula(atual, alvo, out Vector2Int proxima)) return destino; // sem caminho

        // Antes de virar, alinha com o centro da célula atual no eixo perpendicular ao movimento.
        // Assim o inimigo faz as curvas sem cortar os cantos das paredes.
        Vector2 centro = CentroDe(atual);
        bool horizontal = proxima.x != atual.x;
        float desalinhamento = horizontal ? Mathf.Abs(posicao.y - centro.y) : Mathf.Abs(posicao.x - centro.x);

        return desalinhamento > Alinhamento ? centro : CentroDe(proxima);
    }

    // Busca em largura (BFS) que parte do DESTINO e se espalha pelas células livres.
    // Cada célula visitada guarda o vizinho de onde a busca veio, ou seja, o próximo passo
    // rumo ao destino. Quando a busca chega ao início, a resposta já está pronta.
    private bool BuscarProximaCelula(Vector2Int inicio, Vector2Int destino, out Vector2Int proxima)
    {
        for (int i = 0; i < proximoPasso.Length; i++) proximoPasso[i] = -1; // -1 = não visitada
        fila.Clear();

        int idInicio = Indice(inicio);
        int idDestino = Indice(destino);
        proximoPasso[idDestino] = idDestino;
        fila.Enqueue(idDestino);

        while (fila.Count > 0)
        {
            int id = fila.Dequeue();
            if (id == idInicio)
            {
                proxima = Celula(proximoPasso[id]);
                return true;
            }

            Vector2Int celula = Celula(id);
            foreach (Vector2Int direcao in Vizinhos)
            {
                Vector2Int vizinho = celula + direcao;
                if (EhParede(vizinho)) continue;

                int idVizinho = Indice(vizinho);
                if (proximoPasso[idVizinho] != -1) continue; // já visitada

                proximoPasso[idVizinho] = id;
                fila.Enqueue(idVizinho);
            }
        }

        proxima = inicio;
        return false;
    }

    // A grade é guardada em um vetor simples: índice = linha * colunas + coluna
    private int Indice(Vector2Int celula) => celula.y * Colunas + celula.x;
    private Vector2Int Celula(int indice) => new Vector2Int(indice % Colunas, indice / Colunas);
}
