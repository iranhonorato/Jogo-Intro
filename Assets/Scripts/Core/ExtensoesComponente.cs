using UnityEngine;

// Métodos de extensão usados em vários scripts.
public static class ExtensoesComponente
{
    // Retorna o componente T do objeto; se ele não existir, adiciona e retorna o novo.
    // Usado para completar objetos da cena com os componentes que o jogo precisa.
    public static T ObterOuAdicionar<T>(this GameObject objeto) where T : Component
    {
        return objeto.TryGetComponent(out T componente) ? componente : objeto.AddComponent<T>();
    }
}
