using UnityEngine;
using UnityEngine.UI;

// Interface do jogo (HUD), montada por código: cronômetro, fase e itens, corações, tempo restante,
// mensagem de porta aberta e tela final.
//
// Padrão Observer: o HUD apenas observa o GameManager e a Vida do jogador. Ele lê o estado
// a cada frame (valores que mudam sempre, como o cronômetro) e escuta eventos para o que
// acontece de vez em quando (bônus/penalidade, vida alterada, porta aberta e fim de partida).
// Nunca altera as regras do jogo.
public class GameHUD : MonoBehaviour
{
    private static readonly Color Vermelho = new Color(1f, 0.35f, 0.35f);
    private static readonly Color Verde = new Color(0.5f, 1f, 0.5f);
    private static readonly Color CoracaoCheio = new Color(0.95f, 0.15f, 0.25f);
    private static readonly Color CoracaoVazio = new Color(0.2f, 0.2f, 0.2f, 0.6f);
    private const float TamanhoCoracao = 52f;
    private const float EspacoCoracao = 60f;
    private const float DuracaoDestaque = 0.7f;
    private const float DuracaoMensagem = 3f;

    private GameManager jogo;
    private Vida vida;
    private Image[] coracoes;
    private GameObject canvas;
    private Text textoCronometro;
    private Text textoItens;
    private Text textoRestante;
    private Text textoMensagem;
    private Text textoFinal;
    private GameObject painelFinal;

    // Último bônus/penalidade, exibido por alguns instantes ao lado do tempo restante
    private float ultimaVariacao;
    private float destaqueAte;
    private float mensagemAte;

    void Start()
    {
        jogo = GetComponent<GameManager>();
        vida = jogo.VidaJogador; // pode ser null se a cena não tiver jogador
        CriarInterface();

        jogo.PartidaIniciada += Mostrar;
        jogo.TempoAlterado += AoAlterarTempo;
        jogo.ItensConcluidos += AoConcluirItens;
        jogo.PartidaTerminou += AoTerminarPartida;

        if (vida != null)
        {
            vida.VidaAlterada += AtualizarCoracoes;
            AtualizarCoracoes(vida.Atual, vida.Maxima); // desenho inicial: todos cheios
        }

        // Escondido enquanto o menu estiver aberto
        canvas.SetActive(jogo.Estado != EstadoJogo.Menu);
    }

    void OnDestroy()
    {
        // Sempre cancelar a inscrição em eventos para não deixar referências soltas
        if (jogo == null) return;
        jogo.PartidaIniciada -= Mostrar;
        jogo.TempoAlterado -= AoAlterarTempo;
        jogo.ItensConcluidos -= AoConcluirItens;
        jogo.PartidaTerminou -= AoTerminarPartida;
        if (vida != null) vida.VidaAlterada -= AtualizarCoracoes;
    }

    void Update()
    {
        if (!canvas.activeSelf) return;

        textoCronometro.text = $"Tempo: {Formatar(jogo.TempoDecorrido)}";
        textoItens.text = $"Fase {jogo.FaseAtual}/{jogo.TotalFases}    Itens: {jogo.ItensColetados}/{jogo.ItensTotal}";
        textoMensagem.enabled = Time.unscaledTime < mensagemAte && jogo.JogoAtivo;
        AtualizarTempoRestante();
    }

    private void AtualizarTempoRestante()
    {
        string texto = $"Restante: {jogo.TempoRestante:0.0}s";
        Color cor = Color.white;

        // unscaledTime: continua contando mesmo com o jogo congelado (timeScale = 0)
        if (Time.unscaledTime < destaqueAte)
        {
            texto += $"  {ultimaVariacao:+0;-0}s"; // mostra "+3s" ou "-5s"
            cor = ultimaVariacao > 0f ? Verde : Vermelho;
        }
        else if (jogo.JogoAtivo && jogo.TempoRestante <= jogo.tempoAlerta)
        {
            bool piscar = Mathf.Repeat(Time.unscaledTime, 0.5f) < 0.25f;
            cor = piscar ? Color.white : Vermelho;
        }

        textoRestante.text = texto;
        textoRestante.color = cor;
    }

    private void Mostrar()
    {
        canvas.SetActive(true);
    }

    // Os primeiros "atual" corações ficam cheios (vermelhos); o resto fica vazio (cinza)
    private void AtualizarCoracoes(int atual, int maxima)
    {
        for (int i = 0; i < coracoes.Length; i++)
        {
            coracoes[i].color = i < atual ? CoracaoCheio : CoracaoVazio;
        }
    }

    private void AoAlterarTempo(float variacao)
    {
        ultimaVariacao = variacao;
        destaqueAte = Time.unscaledTime + DuracaoDestaque;
    }

    private void AoConcluirItens()
    {
        MostrarMensagem("A porta abriu! Encontre a saída.");
    }

    // Mensagem temporária no topo da tela
    private void MostrarMensagem(string mensagem)
    {
        textoMensagem.text = mensagem;
        mensagemAte = Time.unscaledTime + DuracaoMensagem;
    }

    private void AoTerminarPartida(ResultadoPartida r)
    {
        string texto;
        if (r.Motivo == MotivoFim.Vitoria)
        {
            string recorde = r.NovoRecorde
                ? "<color=#FFD700>NOVO RECORDE!</color>"
                : $"Melhor tempo: {Formatar(r.MelhorTempo)}";

            texto = "<color=#7CFC7C>VOCÊ VENCEU!</color>\n\n" +
                    $"Tempo final: {Formatar(r.TempoFinal)}\n" +
                    $"Tempo que sobrou: {r.TempoRestante:0.0}s\n" +
                    recorde + "\n";
        }
        else
        {
            string titulo = r.Motivo == MotivoFim.SemVidas ? "SEM VIDAS!" : "TEMPO ESGOTADO!";
            texto = $"<color=#FF6060>{titulo}</color>\n\n" +
                    $"Tempo final: {Formatar(r.TempoFinal)}\n" +
                    $"Fase: {r.Fase}/{r.TotalFases}\n" +
                    $"Itens coletados: {r.ItensColetados}/{r.ItensTotal}\n";
        }

        textoFinal.text = texto +
            "\n<size=36>Pressione R para jogar novamente\nPressione M para voltar ao menu</size>";
        painelFinal.SetActive(true);
    }

    // Formato mm:ss.cc (minutos, segundos e centésimos)
    private static string Formatar(float segundos)
    {
        int minutos = (int)(segundos / 60f);
        return $"{minutos:00}:{segundos % 60f:00.00}";
    }

    // ---------- Montagem da interface ----------

    private void CriarInterface()
    {
        Transform raiz = UIFactory.CriarCanvas("HUD", 100);
        canvas = raiz.gameObject;

        textoCronometro = UIFactory.CriarTexto(raiz, "Cronometro", new Vector2(0f, 1f), new Vector2(30, -20), TextAnchor.UpperLeft, 48);
        textoItens = UIFactory.CriarTexto(raiz, "Itens", new Vector2(0.5f, 1f), new Vector2(0, -20), TextAnchor.UpperCenter, 48);
        textoRestante = UIFactory.CriarTexto(raiz, "TempoRestante", new Vector2(1f, 1f), new Vector2(-30, -90), TextAnchor.UpperRight, 48);
        CriarCoracoes(raiz);

        textoMensagem = UIFactory.CriarTexto(raiz, "Mensagem", new Vector2(0.5f, 1f), new Vector2(0, -95), TextAnchor.UpperCenter, 44);
        textoMensagem.color = new Color(1f, 0.9f, 0.3f);
        textoMensagem.enabled = false;

        // Painel escuro que cobre a tela inteira no fim da partida
        painelFinal = UIFactory.CriarPainel(raiz, "PainelFinal", UIFactory.CorFundoTela);
        textoFinal = UIFactory.CriarTexto(painelFinal.transform, "TextoFinal", new Vector2(0.5f, 0.5f), Vector2.zero, TextAnchor.MiddleCenter, 64);
        painelFinal.SetActive(false);
    }

    // Uma fileira de corações no canto superior direito, alinhada pela direita
    private void CriarCoracoes(Transform raiz)
    {
        int quantidade = vida != null ? vida.Maxima : 0;
        coracoes = new Image[quantidade];

        Vector2 cantoSuperiorDireito = new Vector2(1f, 1f);
        Vector2 tamanho = new Vector2(TamanhoCoracao, TamanhoCoracao);

        for (int i = 0; i < quantidade; i++)
        {
            float x = -30f - (quantidade - 1 - i) * EspacoCoracao; // o último coração fica mais à direita
            coracoes[i] = UIFactory.CriarImagem(raiz, "Coracao " + (i + 1), Recursos.Sprite("Coracao"),
                                                cantoSuperiorDireito, new Vector2(x, -25f), tamanho);
        }
    }
}
