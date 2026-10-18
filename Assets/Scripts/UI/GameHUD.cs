using UnityEngine;
using UnityEngine.UI;

// Interface do jogo (HUD), montada por código: cronômetro, itens, tempo restante e tela final.
//
// Padrão Observer: o HUD apenas observa o GameManager. Ele lê o estado a cada frame
// (valores que mudam sempre, como o cronômetro) e escuta eventos para o que acontece
// de vez em quando (bônus/penalidade e fim de partida). Nunca altera as regras do jogo.
public class GameHUD : MonoBehaviour
{
    private static readonly Color Vermelho = new Color(1f, 0.35f, 0.35f);
    private static readonly Color Verde = new Color(0.5f, 1f, 0.5f);
    private const float DuracaoDestaque = 0.7f;

    private GameManager jogo;
    private GameObject canvas;
    private Text textoCronometro;
    private Text textoItens;
    private Text textoRestante;
    private Text textoFinal;
    private GameObject painelFinal;

    // Último bônus/penalidade, exibido por alguns instantes ao lado do tempo restante
    private float ultimaVariacao;
    private float destaqueAte;

    void Start()
    {
        jogo = GetComponent<GameManager>();
        CriarInterface();

        jogo.PartidaIniciada += Mostrar;
        jogo.TempoAlterado += AoAlterarTempo;
        jogo.PartidaTerminou += AoTerminarPartida;

        // Escondido enquanto o menu estiver aberto
        canvas.SetActive(jogo.Estado != EstadoJogo.Menu);
    }

    void OnDestroy()
    {
        // Sempre cancelar a inscrição em eventos para não deixar referências soltas
        if (jogo == null) return;
        jogo.PartidaIniciada -= Mostrar;
        jogo.TempoAlterado -= AoAlterarTempo;
        jogo.PartidaTerminou -= AoTerminarPartida;
    }

    void Update()
    {
        if (!canvas.activeSelf) return;

        textoCronometro.text = $"Tempo: {Formatar(jogo.TempoDecorrido)}";
        textoItens.text = $"Itens: {jogo.ItensColetados}/{jogo.ItensTotal}";
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

    private void AoAlterarTempo(float variacao)
    {
        ultimaVariacao = variacao;
        destaqueAte = Time.unscaledTime + DuracaoDestaque;
    }

    private void AoTerminarPartida(ResultadoPartida r)
    {
        string texto;
        if (r.Venceu)
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
            texto = "<color=#FF6060>TEMPO ESGOTADO!</color>\n\n" +
                    $"Tempo final: {Formatar(r.TempoFinal)}\n" +
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
        textoRestante = UIFactory.CriarTexto(raiz, "TempoRestante", new Vector2(1f, 1f), new Vector2(-30, -20), TextAnchor.UpperRight, 48);

        // Painel escuro que cobre a tela inteira no fim da partida
        painelFinal = UIFactory.CriarPainel(raiz, "PainelFinal", new Color(0f, 0f, 0f, 0.75f));
        textoFinal = UIFactory.CriarTexto(painelFinal.transform, "TextoFinal", new Vector2(0.5f, 0.5f), Vector2.zero, TextAnchor.MiddleCenter, 64);
        painelFinal.SetActive(false);
    }
}
