using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Regras da partida: cronômetro, contagem regressiva, itens, vidas, fases, vitória/derrota e recorde.
// É o orquestrador do jogo: conhece o jogador, a fase e os itens, mas NÃO conhece as telas.
// Quem cria o GameManager e as telas é o InicializadorJogo.
//
// Padrões de projeto:
// - Singleton: existe um único GameManager, acessível de qualquer script por GameManager.Instance.
// - Observer: em vez de mexer na tela diretamente, o GameManager dispara eventos
//   (TempoAlterado, ItensConcluidos, PartidaTerminou...). Quem quiser reagir (GameHUD, Porta)
//   se inscreve neles. O GameManager também é observador: escuta o evento Morreu da Vida
//   do jogador para encerrar a partida.
// - Máquina de estados: a partida está sempre em um estado (Menu, Jogando, Transicao ou Fim),
//   e cada estado define o que o Update faz e quais ações são permitidas.
// - A animação de troca de fase é acessada só pela interface ITransicaoFase.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public const string TagColetavel = "Coletavel";
    // Chave nova: o recorde agora é o tempo para passar por todas as fases,
    // então não pode ser comparado com recordes da versão de uma fase só
    private const string ChaveRecorde = "MelhorTempoTodasFases";

    [Header("Mecânica de Tempo")]
    [Tooltip("Tempo inicial (em segundos) para coletar todos os itens")]
    public float tempoLimite = 45f; // o labirinto deixa os caminhos mais longos


    [Tooltip("Segundos ganhos ao coletar cada item")]
    public float bonusPorItem = 3f;

    [Tooltip("Segundos ganhos ao passar pela porta e entrar em uma nova fase")]
    public float bonusPorFase = 30f;

    
    [Tooltip("Abaixo desse tempo restante o contador fica vermelho")]
    public float tempoAlerta = 5f;

    [Header("Fase")]
    [Tooltip("Monta os labirintos descritos em MapaFase: paredes, itens, corações, porta, inimigos e posição do jogador")]
    public bool construirLabirinto = true;

    // Eventos (Observer). O float é a variação do tempo: positivo = bônus, negativo = penalidade.
    public event Action PartidaIniciada;
    public event Action<float> TempoAlterado;
    public event Action ItensConcluidos;        // todos os itens da fase coletados: a porta abre
    public event Action<int> FaseIniciada;      // número da nova fase (começando em 1)
    public event Action<ResultadoPartida> PartidaTerminou;

    // Estado da partida: qualquer um pode ler, só o GameManager altera.
    public float TempoDecorrido { get; private set; }
    public float TempoRestante { get; private set; }
    public int ItensColetados { get; private set; }
    public int ItensTotal { get; private set; }
    public EstadoJogo Estado { get; private set; } = EstadoJogo.Menu;
    public bool JogoAtivo => Estado == EstadoJogo.Jogando;
    public bool TodosItensColetados => ItensColetados >= ItensTotal;
    public int FaseAtual => indiceFase + 1;
    public int TotalFases => construirLabirinto ? MapaFase.Fases.Length : 1;

    // Vida do jogador, exposta para o HUD desenhar os corações
    public Vida VidaJogador { get; private set; }

    // Ao reiniciar com R a cena é recarregada; este valor estático sobrevive à recarga
    // e faz a nova partida começar direto, sem passar pelo menu.
    private static bool pularMenu;

    private int indiceFase;
    private Transform raizFase;         // objetos da fase atual (destruídos ao trocar de fase)
    private Porta porta;                // null se a fase não tiver porta
    private bool concluirFasePendente;  // a troca de fase espera o próximo Update (fora dos eventos de colisão)
    private ITransicaoFase transicao;   // animação da troca de fase (null = troca instantânea)

    void Awake()
    {
        // Singleton: se já existe outro, este é descartado
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        TempoRestante = tempoLimite;

        // Feito no Awake para que o HUD já encontre a vida no Start dele
        PlayerMovement jogador = FindAnyObjectByType<PlayerMovement>();
        if (jogador != null)
        {
            VidaJogador = jogador.gameObject.ObterOuAdicionar<Vida>();
            VidaJogador.Morreu += AoJogadorMorrer;
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (VidaJogador != null) VidaJogador.Morreu -= AoJogadorMorrer;
    }

    void Start()
    {
        // No Start, e não no Awake: o InicializadorJogo adiciona as telas logo depois do GameManager
        transicao = GetComponent<ITransicaoFase>(); // depende só da interface, não da tela em si

        if (construirLabirinto)
        {
            CarregarFase(0);
        }
        else
        {
            PrepararItens();
        }

        if (pularMenu)
        {
            pularMenu = false;
            IniciarPartida();
        }
        else
        {
            Time.timeScale = 0f; // tudo congelado enquanto o menu está aberto
        }
    }

    void Update()
    {
        switch (Estado)
        {
            case EstadoJogo.Jogando:
                if (concluirFasePendente)
                {
                    concluirFasePendente = false;
                    ConcluirFase();
                    if (!JogoAtivo) break; // era a última fase: a partida terminou
                }
                TempoDecorrido += Time.deltaTime;
                TempoRestante -= Time.deltaTime;
                VerificarTempoEsgotado();
                break;

            case EstadoJogo.Fim:
                Keyboard teclado = Keyboard.current;
                if (teclado == null) break;
                if (teclado.rKey.wasPressedThisFrame) RecarregarCena(pularMenu: true);
                else if (teclado.mKey.wasPressedThisFrame) RecarregarCena(pularMenu: false);
                break;

            // EstadoJogo.Menu: nada acontece até o menu chamar IniciarPartida()
            // EstadoJogo.Transicao: tudo pausado enquanto a corrotina TrocarDeFase roda
        }
    }

    // Chamado pelo botão "Iniciar Jogo" do menu (ou direto, ao reiniciar com R)
    public void IniciarPartida()
    {
        if (Estado != EstadoJogo.Menu) return;

        Estado = EstadoJogo.Jogando;
        Time.timeScale = 1f; // descongela o jogo
        PartidaIniciada?.Invoke();
    }

    // Chamado pelo Player ao coletar um item
    public void ItemColetado()
    {
        if (!JogoAtivo) return;

        ItensColetados++;
        AlterarTempo(bonusPorItem);

        if (!TodosItensColetados) return;

        // Com porta, ela abre e o jogador precisa chegar até ela; sem porta, a fase termina aqui
        if (porta != null) ItensConcluidos?.Invoke();
        else concluirFasePendente = true;
    }

    // Chamado pela Porta quando o jogador entra nela (ela só chama se estiver aberta)
    public void EntrarNaPorta()
    {
        if (JogoAtivo && TodosItensColetados) concluirFasePendente = true;
    }

    // Vai para a próxima fase ou, se era a última, vence o jogo
    private void ConcluirFase()
    {
        if (FaseAtual < TotalFases) StartCoroutine(TrocarDeFase());
        else TerminarPartida(MotivoFim.Vitoria);
    }

    // Troca de fase em etapas: escurece a tela, troca o mapa sem o jogador ver e clareia.
    // O jogo fica pausado durante toda a transição (o tempo não corre e ninguém se mexe).
    private IEnumerator TrocarDeFase()
    {
        Estado = EstadoJogo.Transicao;
        Time.timeScale = 0f;

        if (transicao != null) yield return transicao.Escurecer();
        CarregarFase(indiceFase + 1);
        if (transicao != null) yield return transicao.Clarear();

        Estado = EstadoJogo.Jogando;
        Time.timeScale = 1f;
        AlterarTempo(bonusPorFase); // só agora, para o "+30s" aparecer no HUD com a tela visível
    }

    // Troca o mapa sem recarregar a cena: jogador, vida, tempo e HUD continuam os mesmos
    private void CarregarFase(int indice)
    {
        if (raizFase != null)
        {
            // Desativa na hora (o Destroy só acontece no fim do frame) para que os objetos
            // antigos não colidam nem entrem na contagem de itens da fase nova
            raizFase.gameObject.SetActive(false);
            Destroy(raizFase.gameObject);
        }

        indiceFase = indice;
        ConstrutorFase construtor = new ConstrutorFase(MapaFase.Fases[indice], MapaFase.Origem, MapaFase.TamanhoCelula);
        construtor.Construir();
        raizFase = construtor.Raiz;
        porta = construtor.Porta;

        // A fase é montada antes de contar os itens, porque ela pode mover, copiar ou remover itens
        ItensColetados = 0;
        PrepararItens();

        FaseIniciada?.Invoke(FaseAtual);
    }

    // Chamado pelos inimigos ao atingir o jogador
    public void PerderTempo(float segundos)
    {
        if (!JogoAtivo) return;

        AlterarTempo(-segundos);
        VerificarTempoEsgotado();
    }

    private void AlterarTempo(float variacao)
    {
        TempoRestante += variacao;
        TempoAlterado?.Invoke(variacao);
    }

    private void VerificarTempoEsgotado()
    {
        if (TempoRestante > 0f) return;

        TempoRestante = 0f;
        TerminarPartida(MotivoFim.TempoEsgotado);
    }

    private void AoJogadorMorrer()
    {
        if (JogoAtivo) TerminarPartida(MotivoFim.SemVidas);
    }

    private void TerminarPartida(MotivoFim motivo)
    {
        Estado = EstadoJogo.Fim;
        Time.timeScale = 0f; // congela física e movimento de tudo

        ResultadoPartida resultado = new ResultadoPartida
        {
            Motivo = motivo,
            TempoFinal = TempoDecorrido,
            TempoRestante = TempoRestante,
            ItensColetados = ItensColetados,
            ItensTotal = ItensTotal,
            Fase = FaseAtual,
            TotalFases = TotalFases,
        };

        if (resultado.Venceu) RegistrarRecorde(resultado);

        PartidaTerminou?.Invoke(resultado);
    }

    // O recorde é o MENOR tempo para vencer. PlayerPrefs mantém o valor salvo mesmo fechando o jogo.
    private static void RegistrarRecorde(ResultadoPartida resultado)
    {
        float recorde = PlayerPrefs.GetFloat(ChaveRecorde, float.MaxValue);
        resultado.NovoRecorde = resultado.TempoFinal < recorde;

        if (resultado.NovoRecorde)
        {
            recorde = resultado.TempoFinal;
            PlayerPrefs.SetFloat(ChaveRecorde, recorde);
            PlayerPrefs.Save();
        }

        resultado.MelhorTempo = recorde;
    }

    private static void RecarregarCena(bool pularMenu)
    {
        GameManager.pularMenu = pularMenu;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Conta os itens da fase e garante que cada um saiba ser coletado
    // (os itens da cena vêm do prefab, que não tem o componente ItemColetavel)
    private void PrepararItens()
    {
        GameObject[] itens = ItensDaCena();
        foreach (GameObject item in itens) item.ObterOuAdicionar<ItemColetavel>();
        ItensTotal = itens.Length;
    }

    // Itens ativos na cena (objetos desativados não entram)
    private static GameObject[] ItensDaCena()
    {
        try
        {
            return GameObject.FindGameObjectsWithTag(TagColetavel);
        }
        catch (UnityException) // a Unity lança exceção se a tag não existir no projeto
        {
            Debug.LogWarning($"A tag '{TagColetavel}' não existe. Crie-a em Tags & Layers.");
            return new GameObject[0];
        }
    }
}
