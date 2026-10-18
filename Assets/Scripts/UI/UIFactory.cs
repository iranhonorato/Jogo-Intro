using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Padrão Factory: monta os elementos de interface (canvas, textos, painéis e botões) por código.
// Usada pelo GameHUD e pelo MenuInicial, para que os dois tenham o mesmo visual
// sem repetir o código de criação.
public static class UIFactory
{
    private static readonly Color CorBotao = new Color(0.18f, 0.45f, 0.48f);
    private static readonly Color CorBotaoDestaque = new Color(0.26f, 0.62f, 0.66f);

    private static Font fonte;
    private static Font Fonte
    {
        get
        {
            if (fonte == null) fonte = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return fonte;
        }
    }

    // Canvas que cobre a tela e se adapta ao tamanho dela (referência 1920x1080).
    // ordem: canvases com ordem maior são desenhados por cima.
    public static Transform CriarCanvas(string nome, int ordem)
    {
        GameObject go = new GameObject(nome, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = ordem;

        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        return go.transform;
    }

    // Painel que ocupa toda a área do pai, com uma cor de fundo
    public static GameObject CriarPainel(Transform pai, string nome, Color cor)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform), typeof(Image));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(pai, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = cor;
        return go;
    }

    // Texto ancorado em um ponto do pai (ex.: (0,1) = canto superior esquerdo, (0.5,0.5) = centro)
    public static Text CriarTexto(Transform pai, string nome, Vector2 ancora, Vector2 posicao,
                                  TextAnchor alinhamento, int tamanho)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform), typeof(Text));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(pai, false);
        rt.anchorMin = ancora;
        rt.anchorMax = ancora;
        rt.pivot = ancora;
        rt.anchoredPosition = posicao;

        Text texto = go.GetComponent<Text>();
        texto.font = Fonte;
        texto.fontSize = tamanho;
        texto.alignment = alinhamento;
        texto.color = Color.white;
        texto.horizontalOverflow = HorizontalWrapMode.Overflow; // texto não é cortado
        texto.verticalOverflow = VerticalWrapMode.Overflow;

        // Contorno preto para o texto ficar legível sobre qualquer fundo
        Outline contorno = go.AddComponent<Outline>();
        contorno.effectColor = new Color(0f, 0f, 0f, 0.8f);
        contorno.effectDistance = new Vector2(2, -2);

        return texto;
    }

    // Imagem ancorada em um ponto do pai, com contorno escuro (como os textos)
    public static Image CriarImagem(Transform pai, string nome, Sprite sprite, Vector2 ancora,
                                    Vector2 posicao, Vector2 tamanho)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform), typeof(Image));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(pai, false);
        rt.anchorMin = ancora;
        rt.anchorMax = ancora;
        rt.pivot = ancora;
        rt.anchoredPosition = posicao;
        rt.sizeDelta = tamanho;

        Image imagem = go.GetComponent<Image>();
        imagem.sprite = sprite;
        imagem.preserveAspect = true;
        imagem.raycastTarget = false; // só decorativa: não bloqueia cliques

        Outline contorno = go.AddComponent<Outline>();
        contorno.effectColor = new Color(0f, 0f, 0f, 0.8f);
        contorno.effectDistance = new Vector2(2, -2);

        return imagem;
    }

    // Botão centralizado horizontalmente; posicaoY é a distância vertical em relação ao centro
    public static Button CriarBotao(Transform pai, string rotulo, float posicaoY, UnityAction aoClicar)
    {
        GameObject go = new GameObject("Botao " + rotulo, typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(pai, false);
        rt.sizeDelta = new Vector2(460, 95);
        rt.anchoredPosition = new Vector2(0f, posicaoY);

        // Cores do botão: normal, com o mouse em cima/selecionado pelo teclado, e pressionado
        Button botao = go.GetComponent<Button>();
        go.GetComponent<Image>().color = Color.white; // a cor final vem do ColorBlock abaixo
        ColorBlock cores = botao.colors;
        cores.normalColor = CorBotao;
        cores.highlightedColor = CorBotaoDestaque;
        cores.selectedColor = CorBotaoDestaque;
        cores.pressedColor = CorBotao * 0.8f;
        botao.colors = cores;
        botao.onClick.AddListener(aoClicar);

        Text texto = CriarTexto(go.transform, "Texto", new Vector2(0.5f, 0.5f), Vector2.zero, TextAnchor.MiddleCenter, 44);
        texto.text = rotulo;

        return botao;
    }

    // Botões só recebem cliques se existir um EventSystem na cena.
    // O módulo do Input System (pacote novo) é usado porque o projeto não aceita o Input antigo.
    public static void GarantirEventSystem()
    {
        if (EventSystem.current != null) return;

        GameObject go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        go.GetComponent<InputSystemUIInputModule>().AssignDefaultActions(); // mouse, teclado e controle
    }

    // Seleciona um botão para que ele possa ser usado pelo teclado (setas + Enter)
    public static void Selecionar(Button botao)
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(botao.gameObject);
    }
}
