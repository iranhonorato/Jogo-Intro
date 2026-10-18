using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Regras da partida: cronômetro, contagem regressiva, itens, vitória/derrota e recorde.
//
// Padrões de projeto:
// - Singleton: existe um único GameManager, acessível de qualquer script por GameManager.Instance.
// - Observer: em vez de mexer na tela diretamente, o GameManager dispara eventos
//   (TempoAlterado, PartidaTerminou). Quem quiser reagir (ex.: GameHUD) se inscreve neles.
//   Assim as regras do jogo não dependem da interface.
[RequireComponent(typeof(GameHUD))]
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

    // Eventos (Observer). O float é a variação do tempo: positivo = bônus, negativo = penalidade.
    public event Action<float> TempoAlterado;
    public event Action<ResultadoPartida> PartidaTerminou;

    // Estado da partida: qualquer um pode ler, só o GameManager altera.
    public float TempoDecorrido { get; private set; }
    public float TempoRestante { get; private set; }
    public int ItensColetados { get; private set; }
    public int ItensTotal { get; private set; }
    public bool JogoAtivo { get; private set; }

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

        Time.timeScale = 1f; // a partida anterior pode ter terminado com o jogo congelado
        TempoRestante = tempoLimite;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        ItensTotal = ContarItens();

        if (criarInimigosAutomaticamente && FindAnyObjectByType<Enemy>() == null)
        {
            EnemyFactory.CriarConjuntoPadrao();
        }

        JogoAtivo = true;
    }

    void Update()
    {
        if (JogoAtivo)
        {
            TempoDecorrido += Time.deltaTime;
            TempoRestante -= Time.deltaTime;
            VerificarTempoEsgotado();
        }
        else if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            Reiniciar();
        }
    }

    // Chamado pelo Player ao coletar um item
    public void ItemColetado()
    {
        if (!JogoAtivo) return;

        ItensColetados++;
        AlterarTempo(bonusPorItem);

        if (ItensTotal > 0 && ItensColetados >= ItensTotal)
        {
            TerminarPartida(true);
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
        TerminarPartida(false);
    }

    private void TerminarPartida(bool venceu)
    {
        JogoAtivo = false;
        Time.timeScale = 0f; // congela física e movimento de tudo

        ResultadoPartida resultado = new ResultadoPartida
        {
            Venceu = venceu,
            TempoFinal = TempoDecorrido,
            TempoRestante = TempoRestante,
            ItensColetados = ItensColetados,
            ItensTotal = ItensTotal,
        };

        if (venceu) RegistrarRecorde(resultado);

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

    private void Reiniciar()
    {
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

// Dados enviados no evento PartidaTerminou
public class ResultadoPartida
{
    public bool Venceu;
    public float TempoFinal;
    public float TempoRestante;
    public int ItensColetados;
    public int ItensTotal;
    public float MelhorTempo;
    public bool NovoRecorde;
}
