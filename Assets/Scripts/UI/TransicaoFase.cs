using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Animação de troca de fase: a tela escurece, aparece um cartão com o nome da nova fase
// e a tela clareia já mostrando o mapa novo.
//
// Implementa ITransicaoFase (o GameManager só conhece a interface) e usa o padrão Observer
// para saber o texto do cartão: escuta o evento FaseIniciada do GameManager.
// Usa tempo "unscaled" porque o jogo fica pausado (timeScale = 0) durante a transição.
public class TransicaoFase : MonoBehaviour, ITransicaoFase
{
    private const float DuracaoEscurecer = 0.6f;
    private const float DuracaoTextoAparecer = 0.35f;
    private const float DuracaoCartao = 1.4f; // tempo com o cartão "Fase N" na tela
    private const float DuracaoClarear = 0.6f;

    private GameManager jogo;
    private GameObject canvas;
    private CanvasGroup fundo;  // controla a transparência da tela inteira
    private CanvasGroup textos; // controla só o cartão com o nome da fase
    private Text titulo;
    private Text subtitulo;

    void Start()
    {
        jogo = GetComponent<GameManager>();
        CriarInterface();
        jogo.FaseIniciada += AoIniciarFase;
    }

    void OnDestroy()
    {
        if (jogo != null) jogo.FaseIniciada -= AoIniciarFase;
    }

    public IEnumerator Escurecer()
    {
        textos.alpha = 0f; // o cartão só aparece depois que o mapa novo estiver pronto
        canvas.SetActive(true);
        yield return Animar(fundo, 0f, 1f, DuracaoEscurecer);
    }

    public IEnumerator Clarear()
    {
        yield return Animar(textos, 0f, 1f, DuracaoTextoAparecer);
        yield return new WaitForSecondsRealtime(DuracaoCartao);
        yield return Animar(fundo, 1f, 0f, DuracaoClarear);
        canvas.SetActive(false);
    }

    // O mapa novo foi montado (com a tela já escura): prepara o texto do cartão
    private void AoIniciarFase(int fase)
    {
        bool ultima = fase == jogo.TotalFases;
        titulo.text = $"<b>FASE {fase}</b>";
        subtitulo.text = (ultima ? "A última! Encontre a saída.\n" : "") +
                         $"<color=#7CFC7C>+{jogo.bonusPorFase:0}s de tempo</color>";
    }

    // Muda a transparência de 'de' para 'para' suavemente (SmoothStep acelera e desacelera)
    private static IEnumerator Animar(CanvasGroup grupo, float de, float para, float duracao)
    {
        for (float t = 0f; t < duracao; t += Time.unscaledDeltaTime)
        {
            grupo.alpha = Mathf.SmoothStep(de, para, t / duracao);
            yield return null; // espera o próximo frame
        }
        grupo.alpha = para;
    }

    // ---------- Montagem da interface ----------

    private void CriarInterface()
    {
        Transform raiz = UIFactory.CriarCanvas("Transicao", 150); // acima do HUD, abaixo do menu
        canvas = raiz.gameObject;

        GameObject painel = UIFactory.CriarPainel(raiz, "Fundo", Color.black);
        fundo = painel.AddComponent<CanvasGroup>();

        GameObject cartao = new GameObject("Cartao", typeof(RectTransform));
        RectTransform rt = cartao.GetComponent<RectTransform>();
        rt.SetParent(painel.transform, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        textos = cartao.AddComponent<CanvasGroup>();

        titulo = UIFactory.CriarTexto(cartao.transform, "Titulo", new Vector2(0.5f, 0.5f), new Vector2(0, 60), TextAnchor.MiddleCenter, 110);
        subtitulo = UIFactory.CriarTexto(cartao.transform, "Subtitulo", new Vector2(0.5f, 0.5f), new Vector2(0, -80), TextAnchor.MiddleCenter, 44);

        canvas.SetActive(false);
    }
}
