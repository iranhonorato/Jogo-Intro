using UnityEngine;

// Desenho das fases em texto: cada caractere é uma célula de 0,5 x 0,5 unidade.
// A primeira linha do texto é a parte de CIMA do mapa.
//
// Legenda:
//   #  parede                 .  chão
//   P  início do jogador      I  item (triângulo amarelo)
//   C  coração (vida)         D  porta de saída
//   E  inimigo parado         H  patrulha horizontal
//   V  patrulha vertical      S  sentinela
//   F  perseguidor
//
// A porta abre quando todos os itens da fase são coletados e leva à próxima fase.
// A porta da última fase é a saída do jogo (vitória).
// Patrulhas e sentinelas vão de uma parede até a outra no corredor em que estão.
// Para criar uma fase nova, desenhe outro mapa e adicione-o em Fases (todas as linhas
// com o mesmo tamanho).
public static class MapaFase
{
    public const float TamanhoCelula = 0.5f;

    // Canto inferior esquerdo da grade, alinhado com a parte de dentro das paredes externas da cena
    // (a área interna vai de x = -7 a 7 e de y = -4,1 a 3,9: 28 x 16 células)
    public static readonly Vector2 Origem = new Vector2(-7f, -4.1f);

    // Tamanho (em unidades) dos objetos que já existem na cena e são redimensionados pela fase
    public const float TamanhoJogador = 0.55f;
    public const float TamanhoItem = 0.35f;

    // Fase 1: salas regulares, corredor central longo e a porta no canto inferior direito
    private static readonly string[] Fase1 =
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
        "..I......#..#.....#.....#..F",
        "####..####..#..####..####...",
        "......#........#............",
        "..C...#....I...#....S.......",
        "......#........#...........D",
    };

    // Fase 2: arena central aberta com uma sentinela, mais inimigos, e a porta de saída
    // no canto superior direito, num beco guardado por uma patrulha vertical
    private static readonly string[] Fase2 =
    {
        "...............#...C..#....D",
        ".I.............#.F....#.....",
        "..###########..####...#..###",
        "..#......#........#......#..",
        "..#..I...#........#....V.#.I",
        "..#......#..S.....#......#..",
        "######E..#........#####..#..",
        "..#...................#.....",
        "..#...H...............#.....",
        "..#..#####........#...####..",
        ".....#.C.#...I....#.........",
        ".....#...#........#.....I...",
        ".....#...#........#.........",
        "..####...####E.####...####..",
        ".P.............#..S......#..",
        "...............#....I....#.C",
    };

    // Ordem em que as fases são jogadas
    public static readonly string[][] Fases = { Fase1, Fase2 };
}
