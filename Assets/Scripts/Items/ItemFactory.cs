using UnityEngine;

// Padrão Factory: cria os itens (corações de vida e itens amarelos) a partir dos prefabs
// em Assets/Resources/Prefabs.
public static class ItemFactory
{
    public static GameObject CriarCoracao(Vector2 posicao) => Criar("Coracao", posicao);

    public static GameObject CriarItem(Vector2 posicao) => Criar("Item", posicao);

    private static GameObject Criar(string nomePrefab, Vector2 posicao)
    {
        GameObject prefab = Recursos.Prefab(nomePrefab);
        if (prefab == null) return null;

        GameObject objeto = Object.Instantiate(prefab, posicao, Quaternion.identity);
        objeto.name = prefab.name; // sem o "(Clone)" no nome
        return objeto;
    }
}
