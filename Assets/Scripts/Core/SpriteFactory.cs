using UnityEngine;

// Padrão Factory: gera por código os sprites geométricos simples (quadrado e porta).
// O coração é um arquivo de imagem (Assets/Resources/Sprites/Coracao.png), usado pelo prefab e pelo HUD.
// Cada sprite é criado uma única vez e reaproveitado (cache), já que todos são iguais.
public static class SpriteFactory
{
    private static Sprite quadrado;
    private static Sprite porta;

    // Quadrado branco de 1x1 unidade (a cor vem do SpriteRenderer)
    public static Sprite Quadrado()
    {
        if (quadrado == null)
        {
            Texture2D textura = Texture2D.whiteTexture; // textura 4x4 que já vem na Unity
            quadrado = CriarSprite(textura);
        }
        return quadrado;
    }

    // Porta de 1x1 unidade em tons de cinza: moldura escura, miolo claro e maçaneta.
    // A cor final vem do SpriteRenderer (que "tinge" os tons de cinza).
    public static Sprite Porta()
    {
        if (porta == null)
        {
            const int tamanho = 64;
            const int moldura = 6;
            Texture2D textura = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false);
            textura.wrapMode = TextureWrapMode.Clamp;

            Color32 escuro = new Color32(110, 110, 110, 255);
            Color32 claro = new Color32(255, 255, 255, 255);
            Vector2 macaneta = new Vector2(tamanho * 0.72f, tamanho * 0.45f);

            Color32[] pixels = new Color32[tamanho * tamanho];
            for (int y = 0; y < tamanho; y++)
            {
                for (int x = 0; x < tamanho; x++)
                {
                    bool naMoldura = x < moldura || y < moldura || x >= tamanho - moldura || y >= tamanho - moldura;
                    bool naMacaneta = Vector2.Distance(new Vector2(x, y), macaneta) < 4f;
                    pixels[y * tamanho + x] = naMoldura || naMacaneta ? escuro : claro;
                }
            }

            textura.SetPixels32(pixels);
            textura.Apply();
            porta = CriarSprite(textura);
        }
        return porta;
    }

    // Sprite com o pivô no centro e tamanho de 1 unidade no mundo
    private static Sprite CriarSprite(Texture2D textura)
    {
        return Sprite.Create(textura, new Rect(0, 0, textura.width, textura.height),
                             new Vector2(0.5f, 0.5f), textura.width);
    }
}
