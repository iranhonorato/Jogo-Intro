using UnityEngine;

// Vai e volta entre o ponto inicial e um ponto final.
public class ComportamentoPatrulha : IComportamentoInimigo
{
    private const float DistanciaChegada = 0.05f;

    private readonly Vector2 pontoInicial;
    private readonly Vector2 pontoFinal;
    private bool indoParaFinal = true;

    public ComportamentoPatrulha(Vector2 inicio, Vector2 direcao, float distancia)
    {
        pontoInicial = inicio;
        pontoFinal = inicio + direcao.normalized * distancia;
    }

    public void Atualizar(Enemy inimigo)
    {
        Vector2 destino = indoParaFinal ? pontoFinal : pontoInicial;
        inimigo.MoverPara(destino);

        // Chegou ao destino: inverte o sentido
        if (Vector2.Distance(inimigo.Posicao, destino) < DistanciaChegada)
        {
            indoParaFinal = !indoParaFinal;
        }
    }
}
