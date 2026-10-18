using UnityEngine;

// Padrão Strategy: cada classe abaixo é uma forma diferente de o inimigo se mover.
// O Enemy guarda uma dessas estratégias e chama Atualizar() a cada frame de física.
// Para criar um comportamento novo, basta implementar esta interface e
// registrá-lo em Enemy.CriarEstrategia(), sem mexer nos comportamentos existentes.
public interface IComportamentoInimigo
{
    void Atualizar(Enemy inimigo);
}

// Fica imóvel. Padrão Null Object: um comportamento "vazio" evita checagens de null no Enemy.
public class ComportamentoParado : IComportamentoInimigo
{
    public void Atualizar(Enemy inimigo) { }
}

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

// Vai sempre em direção ao jogador.
public class ComportamentoPerseguicao : IComportamentoInimigo
{
    public void Atualizar(Enemy inimigo)
    {
        if (inimigo.Alvo != null) inimigo.MoverPara(inimigo.Alvo.position);
    }
}

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
