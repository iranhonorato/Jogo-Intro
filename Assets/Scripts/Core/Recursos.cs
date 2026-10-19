using UnityEngine;

// Carrega prefabs e sprites da pasta Assets/Resources pelo nome.
//
// O GameManager e as fábricas são criados por código (não estão salvos na cena), então não
// recebem referências pelo Inspector. A pasta Resources permite que o código encontre os
// prefabs pelo nome: Recursos.Prefab("Coracao") carrega Assets/Resources/Prefabs/Coracao.prefab.
public static class Recursos
{
    public static GameObject Prefab(string nome) => Carregar<GameObject>("Prefabs/" + nome);

    public static Sprite Sprite(string nome) => Carregar<Sprite>("Sprites/" + nome);

    // Imagens de fundo (Assets/Resources/ConfiguracaoVisual.asset). Carregada uma vez e reaproveitada.
    public static ConfiguracaoVisual Visual
    {
        get
        {
            if (visual == null) visual = Carregar<ConfiguracaoVisual>("ConfiguracaoVisual");
            return visual;
        }
    }

    private static ConfiguracaoVisual visual;

    private static T Carregar<T>(string caminho) where T : Object
    {
        T recurso = Resources.Load<T>(caminho);
        if (recurso == null) Debug.LogError($"Recurso não encontrado: Assets/Resources/{caminho}");
        return recurso;
    }
}
