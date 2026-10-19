using UnityEngine;

// Imagens e texturas do jogo e o quanto as imagens de fundo são escurecidas.
//
// É um ScriptableObject: um arquivo de dados do projeto (Assets/Resources/ConfiguracaoVisual.asset),
// editável no Inspector. As imagens são referenciadas diretamente, então podem ficar em qualquer
// pasta e ter qualquer nome. Para trocar uma imagem, basta arrastar outra para o campo.
[CreateAssetMenu(fileName = "ConfiguracaoVisual", menuName = "Jogo Intro/Configuração visual")]
public class ConfiguracaoVisual : ScriptableObject
{
    [Header("Telas")]
    public Sprite fundoMenu;
    public Sprite fundoVitoria;
    public Sprite fundoDerrota;

    [Header("Fases (na mesma ordem de MapaFase.Fases)")]
    public VisualFase[] fases;

    [Header("Contraste")]
    [Tooltip("Brilho das imagens atrás de textos (0 = preto, 1 = original). " +
             "Valores baixos diminuem o contraste e deixam o texto legível.")]
    [Range(0f, 1f)] public float brilhoFundoTelas = 0.35f;

    [Tooltip("Brilho da imagem ao redor do labirinto (0 = preto, 1 = original)")]
    [Range(0f, 1f)] public float brilhoFundoFases = 0.45f;

    [Tooltip("Brilho do piso do labirinto: um pouco mais escuro faz personagens e itens se destacarem")]
    [Range(0f, 1f)] public float brilhoPiso = 0.85f;

    // null se a fase não tiver visual configurado
    public VisualFase DaFase(int indice)
    {
        bool existe = fases != null && indice >= 0 && indice < fases.Length;
        return existe ? fases[indice] : null;
    }

    // Multiplicar as cores por um cinza escurece a imagem e comprime a faixa de cores
    // (menos contraste), sem mudar o tom geral do desenho
    public static Color Tinta(float brilho) => new Color(brilho, brilho, brilho, 1f);
}

// Visual de uma fase. Piso e parede são texturas pequenas, repetidas ao longo da área.
[System.Serializable]
public class VisualFase
{
    [Tooltip("Imagem ao redor da arena")]
    public Sprite fundo;

    [Tooltip("Textura repetida no chão da arena")]
    public Sprite piso;

    [Tooltip("Textura repetida nas paredes (a borda do sprite vira o contorno de cada parede)")]
    public Sprite parede;
}
