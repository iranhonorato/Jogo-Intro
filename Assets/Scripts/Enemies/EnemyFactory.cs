using System.Collections.Generic;
using UnityEngine;

// Padrão Factory: cria os inimigos a partir dos prefabs em Assets/Resources/Prefabs.
// Cada símbolo do mapa corresponde a um prefab, já configurado com comportamento,
// velocidade, direção, raios e cor (ajustáveis no Inspector, sem mexer em código).
public static class EnemyFactory
{
    private static readonly Dictionary<char, string> PrefabPorSimbolo = new Dictionary<char, string>
    {
        { 'E', "InimigoParado" },
        { 'H', "InimigoPatrulhaHorizontal" },
        { 'V', "InimigoPatrulhaVertical" },
        { 'S', "InimigoSentinela" },
        { 'F', "InimigoPerseguidor" },
    };

    public static IEnumerable<char> Simbolos => PrefabPorSimbolo.Keys;

    // Configuração do prefab (sem criar nada), para saber como posicionar o inimigo antes de criá-lo
    public static Enemy Modelo(char simbolo)
    {
        GameObject prefab = Recursos.Prefab(PrefabPorSimbolo[simbolo]);
        return prefab != null ? prefab.GetComponent<Enemy>() : null;
    }

    public static Enemy Criar(char simbolo, Vector2 posicao)
    {
        GameObject prefab = Recursos.Prefab(PrefabPorSimbolo[simbolo]);
        if (prefab == null) return null;

        GameObject inimigo = Object.Instantiate(prefab, posicao, Quaternion.identity);
        inimigo.name = prefab.name; // sem o "(Clone)" no nome
        return inimigo.GetComponent<Enemy>();
    }
}
