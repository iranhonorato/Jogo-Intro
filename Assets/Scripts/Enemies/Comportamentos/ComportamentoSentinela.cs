using UnityEngine;

// Combina duas estratégias: patrulha até o jogador chegar perto e então persegue.
// Usa dois raios (detectar < desistir) para não ficar alternando sem parar
// quando o jogador está exatamente na borda do raio.
public class ComportamentoSentinela : IComportamentoInimigo
{
    private readonly IComportamentoInimigo patrulha;
    private readonly IComportamentoInimigo perseguicao = new ComportamentoPerseguicao();
    private readonly float raioDeteccao;
    private readonly float raioDesistir;
    private bool perseguindo;

    public ComportamentoSentinela(IComportamentoInimigo patrulha, float raioDeteccao, float raioDesistir)
    {
        this.patrulha = patrulha;
        this.raioDeteccao = raioDeteccao;
        this.raioDesistir = raioDesistir;
    }

    public void Atualizar(Enemy inimigo)
    {
        float distancia = inimigo.Alvo != null
            ? Vector2.Distance(inimigo.Posicao, inimigo.Alvo.position)
            : Mathf.Infinity;

        if (distancia <= raioDeteccao) perseguindo = true;
        else if (distancia > raioDesistir) perseguindo = false;

        inimigo.MostrarAlerta(perseguindo);
        (perseguindo ? perseguicao : patrulha).Atualizar(inimigo);
    }
}
