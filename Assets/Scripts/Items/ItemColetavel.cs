using UnityEngine;

// Item amarelo: ao ser tocado pelo jogador, avisa o GameManager e desaparece.
// A coleta fica no próprio item (como no CoracaoColetavel), e não no jogador.
public class ItemColetavel : MonoBehaviour
{
    private bool coletado;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (coletado || !other.TryGetComponent(out PlayerMovement _)) return;

        // O Destroy só acontece no fim do frame: a marcação evita contar o item duas vezes
        coletado = true;

        if (GameManager.Instance != null) GameManager.Instance.ItemColetado();
        Destroy(gameObject);
    }
}
