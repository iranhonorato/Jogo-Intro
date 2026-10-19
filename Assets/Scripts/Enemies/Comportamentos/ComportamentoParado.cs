// Fica imóvel. Padrão Null Object: um comportamento "vazio" evita checagens de null no Enemy.
public class ComportamentoParado : IComportamentoInimigo
{
    public void Atualizar(Enemy inimigo) { }
}
