using UnityEngine;

// Contrato para qualquer coisa que possa levar dano.
// Os inimigos dependem desta interface, e não de uma classe específica: assim, no futuro,
// outros objetos (ex.: um aliado ou uma caixa destrutível) podem levar dano sem mudar o Enemy.
public interface IDanificavel
{
    // direcao: de onde veio o golpe (usada para o empurrão).
    // Retorna false se o dano foi ignorado (ex.: jogador invencível).
    bool ReceberDano(Vector2 direcao);
}
