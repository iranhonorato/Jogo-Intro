using UnityEngine;
using UnityEngine.SceneManagement;

// Monta, em toda cena carregada, as peças que o jogo precisa e que não estão salvas na cena:
// - o GameManager e as telas (HUD, menu inicial e transição de fase);
// - o componente de dano do jogador (que traz junto a Vida).
//
// É o único lugar que junta as partes ("composition root"): ele conhece todo mundo,
// mas ninguém depende dele. Assim o GameManager não precisa conhecer as telas,
// e as telas só conhecem o GameManager.
public static class InicializadorJogo
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Registrar()
    {
        SceneManager.sceneLoaded -= Montar; // evita inscrição dupla
        SceneManager.sceneLoaded += Montar;
    }

    private static void Montar(Scene cena, LoadSceneMode modo)
    {
        // Jogador primeiro: o GameManager procura a Vida dele ao ser criado
        PlayerMovement jogador = Object.FindAnyObjectByType<PlayerMovement>();
        if (jogador != null) jogador.gameObject.ObterOuAdicionar<DanoJogador>();

        // Reaproveita um GameManager colocado na cena; se não houver, cria um
        GameManager existente = Object.FindAnyObjectByType<GameManager>();
        GameObject objeto = existente != null ? existente.gameObject : new GameObject("GameManager");
        objeto.ObterOuAdicionar<GameManager>();

        objeto.ObterOuAdicionar<GameHUD>();
        objeto.ObterOuAdicionar<MenuInicial>();
        objeto.ObterOuAdicionar<TransicaoFase>();
    }
}
