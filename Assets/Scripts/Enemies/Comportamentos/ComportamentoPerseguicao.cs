// Vai sempre em direção ao jogador.
public class ComportamentoPerseguicao : IComportamentoInimigo
{
    public void Atualizar(Enemy inimigo)
    {
        if (inimigo.Alvo != null) inimigo.MoverPara(inimigo.Alvo.position);
    }
}
