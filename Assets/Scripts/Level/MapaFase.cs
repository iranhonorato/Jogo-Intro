using UnityEngine;

// Desenho da fase em texto: cada caractere é uma célula de 0,5 x 0,5 unidade.
// A primeira linha do texto é a parte de CIMA do mapa.
//
// Legenda:
//   #  parede                 .  chão
//   P  início do jogador      I  item (triângulo amarelo)
//   C  coração (vida)         E  inimigo parado
//   H  patrulha horizontal    V  patrulha vertical
//   S  sentinela              F  perseguidor
//
// Patrulhas e sentinelas vão de uma parede até a outra no corredor em que estão.
// Para mudar a fase basta redesenhar o texto (todas as linhas com o mesmo tamanho).
public static class MapaFase
{
    public const float TamanhoCelula = 0.5f;

    // Canto inferior esquerdo da grade, alinhado com a parte de dentro das paredes externas da cena
    // (a área interna vai de x = -7 a 7 e de y = -4,1 a 3,9: 28 x 16 células)
    public static readonly Vector2 Origem = new Vector2(-7f, -4.1f);

    // Tamanho (em unidades) dos objetos que já existem na cena e são redimensionados pela fase
    public const float TamanhoJogador = 0.45f;
    public const float TamanhoItem = 0.35f;

    public static readonly string[] Layout =
    {
        ".........#........#.........",
        ".P.......#........#....I....",
        ".........#........#.........",
        "...####..#..#######E.####...",
        "...#.....#..#...I....#C.#...",
        "...#.C...#..#........#..#.V.",
        "...#..####..####..####..#...",
        "......#..............#......",
        "......#...H..........#......",
        "...####..####..####..#..#...",
        ".........#..#.....#..I..#...",
        "..I......#..#.....#.....#...",
        "####..####..#..####..####...",
        "......#........#............",
        "..C...#....I...#....S.....F.",
        "......#........#............",
    };
}
