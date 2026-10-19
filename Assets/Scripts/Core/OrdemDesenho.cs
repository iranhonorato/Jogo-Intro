// Ordem em que os sprites são desenhados (sortingOrder): números maiores ficam por cima.
// Centralizar aqui evita números soltos espalhados pelos scripts e deixa a hierarquia visual clara.
public static class OrdemDesenho
{
    public const int Fundo = -100;  // imagem ao redor da arena, atrás de tudo
    public const int Piso = -50;    // chão do labirinto
    public const int Cenario = 0;   // paredes e itens amarelos (padrão da Unity)
    public const int Itens = 5;     // corações coletáveis
    public const int Inimigos = 10;
    public const int Jogador = 20;  // sempre na frente de tudo
}
