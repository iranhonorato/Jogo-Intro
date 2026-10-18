using UnityEngine;

// Padrão Factory: concentra a criação dos itens (corações de vida e itens amarelos).
public static class ItemFactory
{
    private const float EscalaCoracao = 0.4f;
    private static readonly Color CorItem = new Color(1f, 0.9f, 0.2f);

    public static CoracaoColetavel CriarCoracao(Vector2 posicao)
    {
        GameObject go = new GameObject("Coracao");
        go.transform.position = posicao;
        go.transform.localScale = Vector3.one * EscalaCoracao; // antes do AddComponent: o Awake guarda essa escala

        return go.AddComponent<CoracaoColetavel>();
    }

    // Item amarelo coletável. Se houver um modelo (um item da cena), faz uma cópia dele,
    // mantendo o mesmo visual; senão, monta um triângulo amarelo por código.
    public static GameObject CriarItem(Vector2 posicao, GameObject modelo)
    {
        if (modelo != null)
        {
            GameObject copia = Object.Instantiate(modelo, posicao, Quaternion.identity);
            copia.name = "Item";
            copia.SetActive(true); // o modelo guardado pelo GameManager fica desativado
            return copia;
        }

        GameObject go = new GameObject("Item", typeof(SpriteRenderer));
        go.transform.position = posicao;

        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Triangulo();
        sr.color = CorItem;
        sr.sortingOrder = OrdemDesenho.Cenario;
        // Adicionado depois do sprite para o collider pegar o formato do triângulo
        go.AddComponent<PolygonCollider2D>().isTrigger = true;

        try
        {
            go.tag = GameManager.TagColetavel; // é pela tag que o jogador reconhece o item
        }
        catch (UnityException)
        {
            Debug.LogWarning($"A tag '{GameManager.TagColetavel}' não existe. Crie-a em Tags & Layers.");
            Object.Destroy(go);
            return null;
        }
        return go;
    }
}
