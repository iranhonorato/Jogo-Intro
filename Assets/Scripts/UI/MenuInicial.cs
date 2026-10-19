using UnityEngine;
using UnityEngine.UI;

// Menu inicial com duas telas: a principal (Iniciar Jogo / Regras) e a de regras.
//
// Padrão Observer: o menu escuta o evento PartidaIniciada do GameManager para se esconder.
// Ele não controla as regras do jogo; só pede ao GameManager para começar a partida.
public class MenuInicial : MonoBehaviour
{
    private GameManager jogo;
    private GameObject canvas;
    private GameObject telaPrincipal;
    private GameObject telaRegras;
    private Text textoRegras;
    private Button botaoIniciar;
    private Button botaoVoltar;

    void Start()
    {
        jogo = GetComponent<GameManager>();
        UIFactory.GarantirEventSystem();
        CriarInterface();

        jogo.PartidaIniciada += Esconder;

        // A partida pode já ter começado (ex.: reinício com R, que pula o menu)
        if (jogo.Estado == EstadoJogo.Menu) MostrarTelaPrincipal();
        else Esconder();
    }

    void OnDestroy()
    {
        if (jogo != null) jogo.PartidaIniciada -= Esconder;
    }

    // ---------- Ações dos botões ----------

    private void IniciarJogo()
    {
        jogo.IniciarPartida(); // dispara PartidaIniciada, que chama Esconder()
    }

    private void MostrarRegras()
    {
        textoRegras.text = MontarTextoRegras(); // montado na hora para refletir os valores atuais
        telaPrincipal.SetActive(false);
        telaRegras.SetActive(true);
        UIFactory.Selecionar(botaoVoltar);
    }

    private void MostrarTelaPrincipal()
    {
        canvas.SetActive(true);
        telaRegras.SetActive(false);
        telaPrincipal.SetActive(true);
        UIFactory.Selecionar(botaoIniciar);
    }

    private void Esconder()
    {
        canvas.SetActive(false);
    }

    // Usa os valores configurados no GameManager e nos inimigos, para as regras
    // continuarem corretas se alguém mudar os números no Inspector
    private string MontarTextoRegras()
    {
        Enemy inimigo = FindAnyObjectByType<Enemy>();
        string penalidade = inimigo != null && inimigo.penalidadeTempo > 0f
            ? $" e {inimigo.penalidadeTempo:0} segundos"
            : "";
        int vidas = jogo.VidaJogador != null ? jogo.VidaJogador.Maxima : 0;

        return
            "<b>Objetivo</b>\n" +
            $"Atravesse os {jogo.TotalFases} labirintos. Mova-se com W A S D ou com as setas.\n" +
            "Colete todos os itens amarelos da fase para abrir a <color=#55FF77>porta</color> e entre nela.\n\n" +
            "<b>Tempo</b>\n" +
            $"Você começa com {jogo.tempoLimite:0}s. Cada item dá +{jogo.bonusPorItem:0}s e cada nova fase, +{jogo.bonusPorFase:0}s.\n" +
            "Se o tempo chegar a zero, você perde.\n\n" +
            "<b>Vidas</b>\n" +
            $"Você tem {vidas} corações. Se acabarem, você perde.\n" +
            "Os <color=#FF4060>corações</color> espalhados pelo mapa recuperam 1 vida.\n\n" +
            "<b>Inimigos</b>\n" +
            $"Encostar em um inimigo tira 1 coração{penalidade} e empurra você.\n" +
            "Alguns ficam parados, outros patrulham, e outros perseguem você.\n" +
            "Quando um inimigo <color=#FF6666>muda de cor</color>, é porque viu você e começou a perseguir!";
    }

    // ---------- Montagem da interface ----------

    private void CriarInterface()
    {
        Transform raiz = UIFactory.CriarCanvas("MenuInicial", 200); // acima do HUD
        canvas = raiz.gameObject;

        // Tela principal
        telaPrincipal = UIFactory.CriarPainel(raiz, "TelaPrincipal", UIFactory.CorFundoTela);
        Transform principal = telaPrincipal.transform;

        Text titulo = UIFactory.CriarTexto(principal, "Titulo", new Vector2(0.5f, 0.5f), new Vector2(0, 260), TextAnchor.MiddleCenter, 120);
        titulo.text = "<b>Pac-Fox</b>";

        Text subtitulo = UIFactory.CriarTexto(principal, "Subtitulo", new Vector2(0.5f, 0.5f), new Vector2(0, 140), TextAnchor.MiddleCenter, 40);
        subtitulo.text = "Colete os itens, abra a porta e escape dos labirintos!";

        botaoIniciar = UIFactory.CriarBotao(principal, "Iniciar Jogo", -20, IniciarJogo);
        UIFactory.CriarBotao(principal, "Regras", -140, MostrarRegras);

        // Tela de regras
        telaRegras = UIFactory.CriarPainel(raiz, "TelaRegras", UIFactory.CorFundoTela);
        Transform regras = telaRegras.transform;

        Text tituloRegras = UIFactory.CriarTexto(regras, "Titulo", new Vector2(0.5f, 0.5f), new Vector2(0, 440), TextAnchor.MiddleCenter, 80);
        tituloRegras.text = "<b>REGRAS</b>";

        textoRegras = UIFactory.CriarTexto(regras, "Texto", new Vector2(0.5f, 0.5f), new Vector2(0, 20), TextAnchor.MiddleCenter, 31);

        botaoVoltar = UIFactory.CriarBotao(regras, "Voltar", -450, MostrarTelaPrincipal);
    }
}
