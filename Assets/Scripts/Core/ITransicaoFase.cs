using System.Collections;

// Contrato da animação de troca de fase.
//
// O GameManager controla QUANDO cada etapa acontece (escurecer, trocar o mapa, clarear),
// mas não sabe COMO a tela é animada: isso fica com quem implementa esta interface
// (hoje, a TransicaoFase). Assim a animação pode ser trocada sem mexer nas regras do jogo.
// As etapas são corrotinas: o GameManager espera cada uma terminar antes de seguir.
public interface ITransicaoFase
{
    // Cobre a tela. Quando terminar, o mapa pode ser trocado sem o jogador ver.
    IEnumerator Escurecer();

    // Mostra a tela do mapa novo de volta.
    IEnumerator Clarear();
}
