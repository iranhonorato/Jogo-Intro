using UnityEngine;
using UnityEngine.UI;

// Menu inicial com duas telas: a principal (Iniciar Jogo / Regras) e a de regras.
//
// Padrão Observer: o menu escuta o evento PartidaIniciada do GameManager para se esconder.
// Ele não controla as regras do jogo; só pede ao GameManager para começar a partida.
public class MenuInicial : MonoBehaviour
{
    private static readonly Color CorFundo = new Color(0.05f, 0.12f, 0.14f, 0.92f);

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
        string penalidade = inimigo != null ? $"{inimigo.penalidadeTempo:0} segundos" : "tempo";

        return
            "<b>Objetivo</b>\n" +
            "Colete todos os itens amarelos antes que o tempo acabe.\n\n" +
            "<b>Controles</b>\n" +
            "Mova-se com W A S D ou com as setas.\n\n" +
            "<b>Tempo</b>\n" +
            $"Você começa com {jogo.tempoLimite:0} segundos.\n" +
            $"Cada item coletado dá +{jogo.bonusPorItem:0} segundos.\n" +
            "Se o tempo chegar a zero, você perde.\n\n" +
            "<b>Inimigos</b>\n" +
            $"Encostar em um inimigo tira {penalidade} e empurra você.\n" +
            "Alguns ficam parados, outros patrulham, e outros perseguem você.\n" +
            "Os que ficam <color=#FF9900>laranja</color> viram você e começaram a perseguir!";
    }

    // ---------- Montagem da interface ----------

    private void CriarInterface()
    {
        Transform raiz = UIFactory.CriarCanvas("MenuInicial", 200); // acima do HUD
        canvas = raiz.gameObject;

        // Tela principal
        telaPrincipal = UIFactory.CriarPainel(raiz, "TelaPrincipal", CorFundo);
        Transform principal = telaPrincipal.transform;

        Text titulo = UIFactory.CriarTexto(principal, "Titulo", new Vector2(0.5f, 0.5f), new Vector2(0, 260), TextAnchor.MiddleCenter, 120);
        titulo.text = "<b>JOGO INTRO</b>";

        Text subtitulo = UIFactory.CriarTexto(principal, "Subtitulo", new Vector2(0.5f, 0.5f), new Vector2(0, 140), TextAnchor.MiddleCenter, 40);
        subtitulo.text = "Colete todos os itens antes que o tempo acabe!";

        botaoIniciar = UIFactory.CriarBotao(principal, "Iniciar Jogo", -20, IniciarJogo);
        UIFactory.CriarBotao(principal, "Regras", -140, MostrarRegras);

        // Tela de regras
        telaRegras = UIFactory.CriarPainel(raiz, "TelaRegras", CorFundo);
        Transform regras = telaRegras.transform;

        Text tituloRegras = UIFactory.CriarTexto(regras, "Titulo", new Vector2(0.5f, 0.5f), new Vector2(0, 400), TextAnchor.MiddleCenter, 80);
        tituloRegras.text = "<b>REGRAS</b>";

        textoRegras = UIFactory.CriarTexto(regras, "Texto", new Vector2(0.5f, 0.5f), new Vector2(0, 20), TextAnchor.MiddleCenter, 34);

        botaoVoltar = UIFactory.CriarBotao(regras, "Voltar", -400, MostrarTelaPrincipal);
    }
}
