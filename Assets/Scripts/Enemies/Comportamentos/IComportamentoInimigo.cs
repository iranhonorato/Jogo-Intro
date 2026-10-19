// Padrão Strategy: cada classe desta pasta é uma forma diferente de o inimigo se mover.
// O Enemy guarda uma dessas estratégias e chama Atualizar() a cada frame de física.
// Para criar um comportamento novo, basta implementar esta interface e
// registrá-lo em Enemy.CriarEstrategia(), sem mexer nos comportamentos existentes.
public interface IComportamentoInimigo
{
    void Atualizar(Enemy inimigo);
}
