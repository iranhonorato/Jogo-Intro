using UnityEngine;

// Padrão Factory: gera por código os sprites simples do jogo, sem depender de arquivos de imagem.
// Cada sprite é criado uma única vez e reaproveitado (cache), já que todos são iguais.
public static class SpriteFactory
{
    private static Sprite quadrado;
    private static Sprite coracao;

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

    // Coração branco de 1x1 unidade (a cor vem do SpriteRenderer ou da Image)
    public static Sprite Coracao()
    {
        if (coracao == null)
        {
            const int tamanho = 64;
            Texture2D textura = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false);
            textura.wrapMode = TextureWrapMode.Clamp;

            Color32 cheio = new Color32(255, 255, 255, 255);
            Color32 vazio = new Color32(255, 255, 255, 0); // transparente
            Color32[] pixels = new Color32[tamanho * tamanho];

            for (int y = 0; y < tamanho; y++)
            {
                for (int x = 0; x < tamanho; x++)
                {
                    // Converte o pixel para coordenadas em torno de (0,0)
                    float px = (x + 0.5f) / tamanho * 2.6f - 1.3f;
                    float py = (y + 0.5f) / tamanho * 2.6f - 1.15f;

                    // Equação do coração: (x² + y² - 1)³ - x²·y³ <= 0 significa "dentro"
                    float a = px * px + py * py - 1f;
                    bool dentro = a * a * a - px * px * py * py * py <= 0f;

                    pixels[y * tamanho + x] = dentro ? cheio : vazio;
                }
            }

            textura.SetPixels32(pixels);
            textura.Apply();
            coracao = CriarSprite(textura);
        }
        return coracao;
    }

    // Sprite com o pivô no centro e tamanho de 1 unidade no mundo
    private static Sprite CriarSprite(Texture2D textura)
    {
        return Sprite.Create(textura, new Rect(0, 0, textura.width, textura.height),
                             new Vector2(0.5f, 0.5f), textura.width);
    }
}
