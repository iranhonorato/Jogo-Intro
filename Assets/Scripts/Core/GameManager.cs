using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Regras da partida: cronômetro, contagem regressiva, itens, vidas, vitória/derrota e recorde.
//
// Padrões de projeto:
// - Singleton: existe um único GameManager, acessível de qualquer script por GameManager.Instance.
// - Observer: em vez de mexer na tela diretamente, o GameManager dispara eventos
//   (TempoAlterado, PartidaTerminou). Quem quiser reagir (ex.: GameHUD) se inscreve neles.
//   Assim as regras do jogo não dependem da interface. O GameManager também é observador:
//   escuta o evento Morreu da Vida do jogador para encerrar a partida.
// - Máquina de estados: a partida está sempre em um estado (Menu, Jogando ou Fim),
//   e cada estado define o que o Update faz e quais ações são permitidas.
[RequireComponent(typeof(GameHUD), typeof(MenuInicial))]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public const string TagColetavel = "Coletavel";
    private const string ChaveRecorde = "MelhorTempo";

    [Header("Mecânica de Tempo")]
    [Tooltip("Tempo inicial (em segundos) para coletar todos os itens")]
    public float tempoLimite = 30f;


    [Tooltip("Segundos ganhos ao coletar cada item")]
    public float bonusPorItem = 3f;

    
    [Tooltip("Abaixo desse tempo restante o contador fica vermelho")]
    public float tempoAlerta = 5f;

    [Header("Inimigos")]
    [Tooltip("Cria um conjunto de inimigos automaticamente se a cena não tiver nenhum")]
    public bool criarInimigosAutomaticamente = true;

    [Header("Itens")]
    [Tooltip("Cria 3 corações de vida automaticamente se a cena não tiver nenhum")]
    public bool criarCoracoesAutomaticamente = true;

    // Eventos (Observer). O float é a variação do tempo: positivo = bônus, negativo = penalidade.
    public event Action PartidaIniciada;
    public event Action<float> TempoAlterado;
    public event Action<ResultadoPartida> PartidaTerminou;

    // Estado da partida: qualquer um pode ler, só o GameManager altera.
    public float TempoDecorrido { get; private set; }
    public float TempoRestante { get; private set; }
    public int ItensColetados { get; private set; }
    public int ItensTotal { get; private set; }
    public EstadoJogo Estado { get; private set; } = EstadoJogo.Menu;
    public bool JogoAtivo => Estado == EstadoJogo.Jogando;

    // Vida do jogador, exposta para o HUD desenhar os corações
    public Vida VidaJogador { get; private set; }

    // Ao reiniciar com R a cena é recarregada; este valor estático sobrevive à recarga
    // e faz a nova partida começar direto, sem passar pelo menu.
    private static bool pularMenu;

    // Garante que toda cena carregada tenha um GameManager, sem precisar colocá-lo à mão.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Registrar()
    {
        SceneManager.sceneLoaded -= AoCarregarCena; // evita inscrição dupla
        SceneManager.sceneLoaded += AoCarregarCena;
    }

    private static void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        if (FindAnyObjectByType<GameManager>() == null)
        {
            new GameObject("GameManager").AddComponent<GameManager>();
        }
    }

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
            VidaJogador = jogador.Vida;
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
        ItensTotal = ContarItens();

        if (criarInimigosAutomaticamente && FindAnyObjectByType<Enemy>() == null)
        {
            EnemyFactory.CriarConjuntoPadrao();
        }

        if (criarCoracoesAutomaticamente && FindAnyObjectByType<CoracaoColetavel>() == null)
        {
            ItemFactory.CriarCoracoesPadrao();
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

        if (ItensTotal > 0 && ItensColetados >= ItensTotal)
        {
            TerminarPartida(MotivoFim.Vitoria);
        }
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

    private static int ContarItens()
    {
        try
        {
            return GameObject.FindGameObjectsWithTag(TagColetavel).Length;
        }
        catch (UnityException) // a Unity lança exceção se a tag não existir no projeto
        {
            Debug.LogWarning($"A tag '{TagColetavel}' não existe. Crie-a em Tags & Layers.");
            return 0;
        }
    }
}

public enum EstadoJogo { Menu, Jogando, Fim }

public enum MotivoFim { Vitoria, TempoEsgotado, SemVidas }

// Dados enviados no evento PartidaTerminou
public class ResultadoPartida
{
    public MotivoFim Motivo;
    public bool Venceu => Motivo == MotivoFim.Vitoria;
    public float TempoFinal;
    public float TempoRestante;
    public int ItensColetados;
    public int ItensTotal;
    public float MelhorTempo;
    public bool NovoRecorde;
}
