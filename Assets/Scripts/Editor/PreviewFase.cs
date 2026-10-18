using UnityEditor;
using UnityEngine;

// Pré-visualização do labirinto na aba Scene, fora do Play.
//
// O labirinto só é montado de verdade ao dar Play (pelo ConstrutorFase). Este script de editor
// desenha o MapaFase por cima da cena para mostrar como a fase vai ficar, sem criar objetos.
// Fica numa pasta "Editor", então não vai para o jogo final.
// Liga/desliga pelo menu: Jogo Intro > Pré-visualizar labirinto.
[InitializeOnLoad]
public static class PreviewFase
{
    private const string Menu = "Jogo Intro/Pré-visualizar labirinto";
    private const string ChaveAtivo = "JogoIntro.PreviewFase";

    private static readonly Color CorParede = new Color(1f, 1f, 1f, 0.55f);
    private static readonly Color CorJogador = new Color(0.75f, 0.2f, 0.25f);
    private static readonly Color CorItem = new Color(1f, 0.9f, 0.2f);
    private static readonly Color CorCoracao = new Color(1f, 0.25f, 0.35f);
    private static readonly Color CorInimigo = new Color(0.55f, 0.2f, 0.8f);

    private static GUIStyle estiloRotulo;

    static PreviewFase()
    {
        SceneView.duringSceneGui += Desenhar;
    }

    private static bool Ativo
    {
        get => EditorPrefs.GetBool(ChaveAtivo, true);
        set => EditorPrefs.SetBool(ChaveAtivo, value);
    }

    [MenuItem(Menu)]
    private static void Alternar()
    {
        Ativo = !Ativo;
        SceneView.RepaintAll();
    }

    [MenuItem(Menu, true)]
    private static bool AtualizarMarcaDoMenu()
    {
        UnityEditor.Menu.SetChecked(Menu, Ativo);
        return true;
    }

    private static void Desenhar(SceneView cena)
    {
        // Durante o Play o labirinto real já está na cena
        if (!Ativo || EditorApplication.isPlayingOrWillChangePlaymode) return;

        string[] mapa = MapaFase.Layout;
        int linhas = mapa.Length;
        float celula = MapaFase.TamanhoCelula;
        Vector2 tamanho = new Vector2(celula, celula);

        for (int i = 0; i < linhas; i++)
        {
            for (int x = 0; x < mapa[i].Length; x++)
            {
                // A linha 0 do texto é a de cima do mapa
                Vector2 canto = MapaFase.Origem + new Vector2(x, linhas - 1 - i) * celula;
                Vector2 centro = canto + tamanho / 2f;
                char simbolo = mapa[i][x];

                switch (simbolo)
                {
                    case '#':
                        Handles.DrawSolidRectangleWithOutline(new Rect(canto, tamanho), CorParede, Color.clear);
                        break;
                    case 'P':
                        Marcador(centro, MapaFase.TamanhoJogador / 2f, CorJogador, "P");
                        break;
                    case 'I':
                        Marcador(centro, MapaFase.TamanhoItem / 2f, CorItem, "I");
                        break;
                    case 'C':
                        Marcador(centro, 0.2f, CorCoracao, "C");
                        break;
                    case 'E':
                    case 'H':
                    case 'V':
                    case 'S':
                    case 'F':
                        Marcador(centro, 0.2f, CorInimigo, simbolo.ToString());
                        break;
                }
            }
        }
    }

    // Círculo colorido com a letra do símbolo no meio
    private static void Marcador(Vector2 centro, float raio, Color cor, string letra)
    {
        Handles.color = cor;
        Handles.DrawSolidDisc(centro, Vector3.forward, raio);

        if (estiloRotulo == null)
        {
            estiloRotulo = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
            estiloRotulo.normal.textColor = Color.black;
        }
        Handles.Label(centro, letra, estiloRotulo);
    }
}
