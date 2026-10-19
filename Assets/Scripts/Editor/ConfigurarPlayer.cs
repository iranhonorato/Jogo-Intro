using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Ferramenta do editor: configura o Player da cena aberta com o sprite da raposinha
// (sprite, cor branca, sem rotação e collider do tamanho do desenho) e salva a cena.
//
// Roda sozinha uma única vez, logo depois de ser compilada; depois disso fica disponível
// no menu: Jogo Intro > Configurar Player com a raposinha.
// Fica numa pasta "Editor", então não vai para o jogo final.
[InitializeOnLoad]
public static class ConfigurarPlayer
{
    private const string CaminhoSprite = "Assets/sprites/player/raposinha_principal.png";
    private const string ChaveJaAplicado = "JogoIntro.ConfigurarPlayer.Raposinha";
    private const float RaioColisor = 0.42f; // um pouco menor que o desenho: hitbox mais justa

    static ConfigurarPlayer()
    {
        if (EditorPrefs.GetBool(ChaveJaAplicado, false)) return;

        // Espera o editor terminar de carregar antes de mexer na cena
        EditorApplication.delayCall += () =>
        {
            if (Aplicar()) EditorPrefs.SetBool(ChaveJaAplicado, true);
        };
    }

    [MenuItem("Jogo Intro/Configurar Player com a raposinha")]
    private static void AplicarPeloMenu()
    {
        Aplicar();
    }

    private static bool Aplicar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return false;

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CaminhoSprite);
        if (sprite == null)
        {
            Debug.LogWarning($"ConfigurarPlayer: sprite não encontrado em {CaminhoSprite}.");
            return false;
        }

        PlayerMovement jogador = Object.FindAnyObjectByType<PlayerMovement>();
        if (jogador == null || !jogador.TryGetComponent(out SpriteRenderer sr))
        {
            Debug.LogWarning("ConfigurarPlayer: nenhum Player com Sprite Renderer na cena aberta.");
            return false;
        }

        // Registra no Undo: dá para desfazer com Ctrl+Z
        List<Object> alterados = new List<Object> { sr, jogador.transform };
        jogador.TryGetComponent(out CircleCollider2D colisor);
        if (colisor != null) alterados.Add(colisor);
        Undo.RecordObjects(alterados.ToArray(), "Configurar Player com a raposinha");

        sr.sprite = sprite;
        sr.color = Color.white;                       // sem tinta: as cores vêm do desenho
        jogador.transform.rotation = Quaternion.identity; // a raposa fica em pé
        if (colisor != null) colisor.radius = RaioColisor;

        EditorSceneManager.MarkSceneDirty(jogador.gameObject.scene);
        EditorSceneManager.SaveScene(jogador.gameObject.scene);

        Selection.activeGameObject = jogador.gameObject; // mostra o Player no Inspector
        Debug.Log("ConfigurarPlayer: Player configurado com a raposinha e cena salva.");
        return true;
    }
}
