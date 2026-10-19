using System.Collections.Generic;
using UnityEngine;

// Padrão Builder: monta a fase passo a passo a partir de uma descrição em texto (MapaFase).
// O Builder decide O QUE vai em cada lugar; os objetos em si são criados pelas fábricas
// (EnemyFactory, ItemFactory, SpriteFactory).
public class ConstrutorFase
{
    private readonly string[] mapa;
    private readonly Vector2 origem;
    private readonly float celula;
    private readonly int colunas;
    private readonly int linhas;

    private Labirinto labirinto;
    private Transform pastaFase; // objeto pai que agrupa a fase na Hierarchy

    // Resultado da construção, usado pelo GameManager
    public Transform Raiz => pastaFase;   // destruída ao trocar de fase
    public Porta Porta { get; private set; } // null se o mapa não tiver porta

    public ConstrutorFase(string[] mapa, Vector2 origem, float tamanhoCelula)
    {
        this.mapa = mapa;
        this.origem = origem;
        celula = tamanhoCelula;
        linhas = mapa.Length;
        colunas = mapa[0].Length;

        foreach (string linha in mapa)
        {
            if (linha.Length != colunas)
                Debug.LogError("MapaFase: todas as linhas do mapa precisam ter o mesmo tamanho.");
        }
    }

    public Labirinto Construir()
    {
        pastaFase = new GameObject("Fase").transform;

        CriarGrade();
        CriarParedes();
        PosicionarJogador();
        PosicionarItens();
        CriarCoracoes();
        CriarPorta();
        CriarInimigos();

        return labirinto;
    }

    // ---------- Passos da construção ----------

    private void CriarGrade()
    {
        bool[,] parede = new bool[colunas, linhas];
        for (int x = 0; x < colunas; x++)
        {
            for (int y = 0; y < linhas; y++) parede[x, y] = Simbolo(x, y) == '#';
        }
        labirinto = new Labirinto(parede, origem, celula);
    }

    // Junta paredes vizinhas em retângulos maiores (em vez de um objeto por célula).
    // Menos objetos, e o jogador desliza pelas paredes sem "enganchar" nas emendas.
    private void CriarParedes()
    {
        bool[,] usada = new bool[colunas, linhas];

        for (int y = 0; y < linhas; y++)
        {
            for (int x = 0; x < colunas; x++)
            {
                if (!EhParedeLivre(x, y, usada)) continue;

                // Estica para a direita enquanto houver parede...
                int largura = 1;
                while (EhParedeLivre(x + largura, y, usada)) largura++;

                // ...e depois para cima, enquanto a linha inteira de cima também for parede
                int altura = 1;
                while (LinhaDeParede(x, y + altura, largura, usada)) altura++;

                for (int i = 0; i < largura; i++)
                    for (int j = 0; j < altura; j++) usada[x + i, y + j] = true;

                CriarParede(x, y, largura, altura);
            }
        }
    }

    private void PosicionarJogador()
    {
        PlayerMovement jogador = Object.FindAnyObjectByType<PlayerMovement>();
        if (jogador == null) return;

        foreach (Vector2Int c in Posicoes('P'))
        {
            jogador.Posicionar(labirinto.CentroDe(c));
            break; // só existe um jogador
        }
        AjustarTamanho(jogador.gameObject, MapaFase.TamanhoJogador);
    }

    // Reaproveita os itens que já estão na cena (movendo-os para as posições do mapa).
    // Se faltar item, cria a partir do prefab; se sobrar, o excedente é removido.
    private void PosicionarItens()
    {
        GameObject[] existentes = ItensDaCena();
        List<Vector2Int> posicoes = new List<Vector2Int>(Posicoes('I'));

        for (int i = 0; i < posicoes.Count; i++)
        {
            Vector2 posicao = labirinto.CentroDe(posicoes[i]);
            GameObject item;
            if (i < existentes.Length)
            {
                item = existentes[i];
                item.transform.position = posicao;
            }
            else
            {
                item = ItemFactory.CriarItem(posicao);
                if (item == null) continue;
                item.transform.SetParent(pastaFase, true);
            }
            AjustarTamanho(item, MapaFase.TamanhoItem);
        }

        // Itens sobrando: desativados na hora (para não entrarem na contagem) e destruídos
        for (int i = posicoes.Count; i < existentes.Length; i++)
        {
            existentes[i].SetActive(false);
            Object.Destroy(existentes[i]);
        }
    }

    private void CriarCoracoes()
    {
        foreach (Vector2Int c in Posicoes('C'))
        {
            GameObject coracao = ItemFactory.CriarCoracao(labirinto.CentroDe(c));
            if (coracao != null) coracao.transform.SetParent(pastaFase, true);
        }
    }

    private void CriarPorta()
    {
        foreach (Vector2Int c in Posicoes('D'))
        {
            GameObject porta = new GameObject("Porta");
            porta.transform.SetParent(pastaFase, false);
            porta.transform.position = labirinto.CentroDe(c);
            porta.transform.localScale = new Vector3(celula * 0.9f, celula, 1f); // um pouco mais alta que larga
            Porta = porta.AddComponent<Porta>();
            break; // uma porta por fase
        }
    }

    private void CriarInimigos()
    {
        foreach (char simbolo in EnemyFactory.Simbolos)
        {
            Enemy modelo = EnemyFactory.Modelo(simbolo);
            if (modelo == null) continue;

            foreach (Vector2Int c in Posicoes(simbolo))
            {
                // Patrulhas (e sentinelas) vão de uma parede até a outra no corredor onde foram
                // desenhadas, na direção definida no prefab. Os demais nascem na própria célula.
                Vector2Int inicio = c;
                float distancia = modelo.distanciaPatrulha;
                if (modelo.comportamento == Enemy.Comportamento.Patrulha || modelo.comportamento == Enemy.Comportamento.Sentinela)
                {
                    Vector2Int direcao = Vector2Int.RoundToInt(modelo.direcaoPatrulha.normalized);
                    inicio = labirinto.UltimaLivre(c, -direcao);
                    distancia = Vector2Int.Distance(inicio, labirinto.UltimaLivre(c, direcao)) * celula;
                }

                // Criado já na posição inicial: o Enemy guarda essa posição no Start
                Enemy inimigo = EnemyFactory.Criar(simbolo, labirinto.CentroDe(inicio));
                if (inimigo == null) continue;
                inimigo.distanciaPatrulha = distancia;
                Configurar(inimigo);
            }
        }
    }

    // Todo inimigo da fase recebe o labirinto para andar pelos corredores
    private void Configurar(Enemy inimigo)
    {
        inimigo.Labirinto = labirinto;
        inimigo.transform.SetParent(pastaFase, true);
    }

    // ---------- Ajudantes ----------

    // Converte (coluna, linha) da grade para o caractere do texto (onde a linha 0 é a de cima)
    private char Simbolo(int x, int y) => mapa[linhas - 1 - y][x];

    private IEnumerable<Vector2Int> Posicoes(char simbolo)
    {
        for (int y = 0; y < linhas; y++)
            for (int x = 0; x < colunas; x++)
                if (Simbolo(x, y) == simbolo) yield return new Vector2Int(x, y);
    }

    private bool EhParedeLivre(int x, int y, bool[,] usada)
    {
        return x < colunas && y < linhas && Simbolo(x, y) == '#' && !usada[x, y];
    }

    private bool LinhaDeParede(int x, int y, int largura, bool[,] usada)
    {
        for (int i = 0; i < largura; i++)
            if (!EhParedeLivre(x + i, y, usada)) return false;
        return true;
    }

    private void CriarParede(int x, int y, int largura, int altura)
    {
        GameObject parede = new GameObject("Parede", typeof(SpriteRenderer));
        parede.transform.SetParent(pastaFase, false);
        parede.transform.position = origem + new Vector2(x + largura / 2f, y + altura / 2f) * celula;
        parede.transform.localScale = new Vector3(largura * celula, altura * celula, 1f);

        SpriteRenderer sr = parede.GetComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Quadrado();
        sr.color = Color.white;
        sr.sortingOrder = OrdemDesenho.Cenario;

        // O collider precisa ser adicionado DEPOIS do sprite: ao ser criado, o BoxCollider2D
        // copia o tamanho do sprite, e sem sprite ele ficaria com tamanho zero (sem colisão).
        // O tamanho também é definido explicitamente: 1x1, multiplicado pela escala da parede.
        BoxCollider2D colisor = parede.AddComponent<BoxCollider2D>();
        colisor.size = Vector2.one;
    }

    // Redimensiona um objeto para que seu sprite tenha 'tamanho' unidades (no maior lado)
    private static void AjustarTamanho(GameObject objeto, float tamanho)
    {
        if (!objeto.TryGetComponent(out SpriteRenderer sr) || sr.sprite == null) return;

        Vector3 medidas = sr.sprite.bounds.size; // tamanho do sprite sem escala
        float escala = tamanho / Mathf.Max(medidas.x, medidas.y);
        objeto.transform.localScale = new Vector3(escala, escala, 1f);
    }

    private static GameObject[] ItensDaCena()
    {
        try
        {
            return GameObject.FindGameObjectsWithTag(GameManager.TagColetavel);
        }
        catch (UnityException) // tag não existe no projeto
        {
            return new GameObject[0];
        }
    }
}
