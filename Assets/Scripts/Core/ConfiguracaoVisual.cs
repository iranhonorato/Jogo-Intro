using UnityEngine;

// Imagens de fundo do jogo e o quanto elas são escurecidas.
//
// É um ScriptableObject: um arquivo de dados do projeto (Assets/Resources/ConfiguracaoVisual.asset),
// editável no Inspector. As imagens são referenciadas diretamente, então podem ficar em qualquer
// pasta e ter qualquer nome. Para trocar um fundo, basta arrastar outra imagem para o campo.
[CreateAssetMenu(fileName = "ConfiguracaoVisual", menuName = "Jogo Intro/Configuração visual")]
public class ConfiguracaoVisual : ScriptableObject
{
    [Header("Telas")]
    public Sprite fundoMenu;
    public Sprite fundoVitoria;
    public Sprite fundoDerrota;

    [Header("Fases (na mesma ordem de MapaFase.Fases)")]
    public Sprite[] fundosFases;

    [Header("Contraste")]
    [Tooltip("Brilho das imagens atrás de textos (0 = preto, 1 = original). " +
             "Valores baixos diminuem o contraste e deixam o texto legível.")]
    [Range(0f, 1f)] public float brilhoFundoTelas = 0.35f;

    [Tooltip("Brilho da imagem atrás do labirinto (0 = preto, 1 = original)")]
    [Range(0f, 1f)] public float brilhoFundoFases = 0.45f;

    // null se a fase não tiver fundo configurado
    public Sprite FundoDaFase(int indice)
    {
        bool existe = fundosFases != null && indice >= 0 && indice < fundosFases.Length;
        return existe ? fundosFases[indice] : null;
    }

    // Multiplicar as cores por um cinza escurece a imagem e comprime a faixa de cores
    // (menos contraste), sem mudar o tom geral do desenho
    public static Color Tinta(float brilho) => new Color(brilho, brilho, brilho, 1f);
}
