using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Controla a mecânica de tempo do jogo:
// - Cronômetro com o tempo decorrido da partida
// - Contagem regressiva: se o tempo acabar, o jogador perde
// - Cada item coletado dá tempo bônus
// - Ao final, mostra o tempo final e o melhor tempo (recorde)
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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

    private const string ChaveRecorde = "MelhorTempo";

    private float tempoDecorrido;
    private float tempoRestante;
    private int itensTotal;
    private int itensColetados;
    private bool jogoAtivo;
    private float destaqueBonus;
    private float destaquePenalidade;
    private float ultimaPenalidade;

    private Font fonte;
    private Text textoCronometro;
    private Text textoRestante;
    private Text textoItens;
    private Text textoFinal;
    private GameObject painelFinal;

    // Cria o GameManager automaticamente em toda cena carregada, caso ele não exista
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Registrar()
    {
        SceneManager.sceneLoaded -= AoCarregarCena;
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
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        Time.timeScale = 1f;
        tempoRestante = tempoLimite;
        itensTotal = ContarItens();

        CriarInimigosPadrao();
        CriarInterface();
        jogoAtivo = true;
    }

    void Update()
    {
        if (jogoAtivo)
        {
            tempoDecorrido += Time.deltaTime;
            tempoRestante -= Time.deltaTime;

            if (tempoRestante <= 0f)
            {
                tempoRestante = 0f;
                FimDeJogo(false);
            }
        }
        else if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            Reiniciar();
        }

        if (destaqueBonus > 0f) destaqueBonus -= Time.unscaledDeltaTime;
        if (destaquePenalidade > 0f) destaquePenalidade -= Time.unscaledDeltaTime;

        AtualizarInterface();
    }

    // Chamado pelo Player ao coletar um item
    public void ItemColetado()
    {
        if (!jogoAtivo) return;

        itensColetados++;
        tempoRestante += bonusPorItem;
        destaqueBonus = 0.6f;
        destaquePenalidade = 0f;

        if (itensTotal > 0 && itensColetados >= itensTotal)
        {
            FimDeJogo(true);
        }
    }

    // Chamado pelos inimigos ao atingir o jogador
    public void PerderTempo(float segundos)
    {
        if (!jogoAtivo) return;

        tempoRestante -= segundos;
        ultimaPenalidade = segundos;
        destaquePenalidade = 0.8f;
        destaqueBonus = 0f;

        if (tempoRestante <= 0f)
        {
            tempoRestante = 0f;
            FimDeJogo(false);
        }
    }

    private void FimDeJogo(bool venceu)
    {
        jogoAtivo = false;
        Time.timeScale = 0f; // congela o jogo (física e movimento)

        string texto;
        if (venceu)
        {
            float recorde = PlayerPrefs.GetFloat(ChaveRecorde, float.MaxValue);
            bool novoRecorde = tempoDecorrido < recorde;
            if (novoRecorde)
            {
                recorde = tempoDecorrido;
                PlayerPrefs.SetFloat(ChaveRecorde, recorde);
                PlayerPrefs.Save();
            }

            texto = "<color=#7CFC7C>VOCÊ VENCEU!</color>\n\n" +
                    $"Tempo final: {Formatar(tempoDecorrido)}\n" +
                    $"Tempo que sobrou: {tempoRestante:0.0}s\n" +
                    (novoRecorde
                        ? "<color=#FFD700>NOVO RECORDE!</color>\n"
                        : $"Melhor tempo: {Formatar(recorde)}\n");
        }
        else
        {
            texto = "<color=#FF6060>TEMPO ESGOTADO!</color>\n\n" +
                    $"Tempo final: {Formatar(tempoDecorrido)}\n" +
                    $"Itens coletados: {itensColetados}/{itensTotal}\n";
        }

        textoFinal.text = texto + "\n<size=36>Pressione R para jogar novamente</size>";
        painelFinal.SetActive(true);
    }

    private void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private int ContarItens()
    {
        try
        {
            return GameObject.FindGameObjectsWithTag("Coletavel").Length;
        }
        catch (UnityException)
        {
            Debug.LogWarning("A tag 'Coletavel' não existe. Crie-a em Tags & Layers.");
            return 0;
        }
    }

    // Cria um inimigo de cada comportamento, posicionados dentro da arena
    private void CriarInimigosPadrao()
    {
        if (!criarInimigosAutomaticamente) return;
        if (FindAnyObjectByType<Enemy>() != null) return; // a cena já tem inimigos próprios

        CriarInimigo("Inimigo Parado", new Vector2(0f, 0.5f), Enemy.Comportamento.Parado, Vector2.zero, 0f, 0f);
        CriarInimigo("Inimigo Patrulha Horizontal", new Vector2(-5f, -2.5f), Enemy.Comportamento.Patrulha, Vector2.right, 10f, 3f);
        CriarInimigo("Inimigo Patrulha Vertical", new Vector2(3f, -3.2f), Enemy.Comportamento.Patrulha, Vector2.up, 6.4f, 2.5f);
        CriarInimigo("Inimigo Sentinela", new Vector2(5.5f, 2.5f), Enemy.Comportamento.Sentinela, Vector2.left, 4f, 2.5f);
        CriarInimigo("Inimigo Perseguidor", new Vector2(6f, -3.3f), Enemy.Comportamento.Perseguicao, Vector2.zero, 0f, 1.5f);
    }

    private void CriarInimigo(string nome, Vector2 posicao, Enemy.Comportamento comportamento,
                              Vector2 direcao, float distancia, float velocidade)
    {
        GameObject go = new GameObject(nome);
        go.transform.position = posicao;
        go.transform.localScale = Vector3.one * 0.7f;

        Enemy inimigo = go.AddComponent<Enemy>();
        inimigo.comportamento = comportamento;
        inimigo.direcaoPatrulha = direcao;
        inimigo.distanciaPatrulha = distancia;
        inimigo.velocidade = velocidade;
    }

    // Formato mm:ss.cc
    private static string Formatar(float segundos)
    {
        int minutos = (int)(segundos / 60f);
        float resto = segundos % 60f;
        return $"{minutos:00}:{resto:00.00}";
    }

    private void AtualizarInterface()
    {
        textoCronometro.text = $"Tempo: {Formatar(tempoDecorrido)}";

        if (destaquePenalidade > 0f)
        {
            textoRestante.text = $"Restante: {tempoRestante:0.0}s  -{ultimaPenalidade:0}s";
            textoRestante.color = new Color(1f, 0.35f, 0.35f);
        }
        else if (destaqueBonus > 0f)
        {
            textoRestante.text = $"Restante: {tempoRestante:0.0}s  +{bonusPorItem:0}s";
            textoRestante.color = new Color(0.5f, 1f, 0.5f);
        }
        else
        {
            textoRestante.text = $"Restante: {tempoRestante:0.0}s";
            bool alerta = tempoRestante <= tempoAlerta && jogoAtivo;
            // pisca em vermelho quando o tempo está acabando
            bool piscar = alerta && Mathf.Repeat(Time.unscaledTime, 0.5f) < 0.25f;
            textoRestante.color = alerta ? (piscar ? Color.white : new Color(1f, 0.35f, 0.35f)) : Color.white;
        }

        textoItens.text = $"Itens: {itensColetados}/{itensTotal}";
    }

    // ---------- Criação da interface (HUD) via código ----------

    private void CriarInterface()
    {
        fonte = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject canvasGO = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        Transform raiz = canvas.transform;
        textoCronometro = CriarTexto(raiz, "Cronometro", new Vector2(0f, 1f), new Vector2(30, -20), TextAnchor.UpperLeft, 48);
        textoItens = CriarTexto(raiz, "Itens", new Vector2(0.5f, 1f), new Vector2(0, -20), TextAnchor.UpperCenter, 48);
        textoRestante = CriarTexto(raiz, "TempoRestante", new Vector2(1f, 1f), new Vector2(-30, -20), TextAnchor.UpperRight, 48);

        // Painel escuro de fim de jogo
        painelFinal = new GameObject("PainelFinal", typeof(RectTransform), typeof(Image));
        RectTransform rtPainel = painelFinal.GetComponent<RectTransform>();
        rtPainel.SetParent(raiz, false);
        rtPainel.anchorMin = Vector2.zero;
        rtPainel.anchorMax = Vector2.one;
        rtPainel.offsetMin = Vector2.zero;
        rtPainel.offsetMax = Vector2.zero;
        painelFinal.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

        textoFinal = CriarTexto(painelFinal.transform, "TextoFinal", new Vector2(0.5f, 0.5f), Vector2.zero, TextAnchor.MiddleCenter, 64);
        textoFinal.rectTransform.sizeDelta = new Vector2(1400, 700);
        painelFinal.SetActive(false);
    }

    private Text CriarTexto(Transform pai, string nome, Vector2 ancora, Vector2 posicao, TextAnchor alinhamento, int tamanho)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform), typeof(Text));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(pai, false);
        rt.anchorMin = ancora;
        rt.anchorMax = ancora;
        rt.pivot = ancora;
        rt.anchoredPosition = posicao;
        rt.sizeDelta = new Vector2(700, 100);

        Text texto = go.GetComponent<Text>();
        texto.font = fonte;
        texto.fontSize = tamanho;
        texto.alignment = alinhamento;
        texto.color = Color.white;
        texto.horizontalOverflow = HorizontalWrapMode.Overflow;
        texto.verticalOverflow = VerticalWrapMode.Overflow;

        Outline contorno = go.AddComponent<Outline>();
        contorno.effectColor = new Color(0f, 0f, 0f, 0.8f);
        contorno.effectDistance = new Vector2(2, -2);

        return texto;
    }
}
