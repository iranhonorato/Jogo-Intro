using System;
using UnityEngine;

// Pontos de vida (corações) de um objeto.
//
// Fica em um componente separado do PlayerMovement (responsabilidade única), então
// pode ser reaproveitado em qualquer objeto que precise de vida.
// Padrão Observer: avisa por eventos quando a vida muda ou acaba. O HUD usa VidaAlterada
// para desenhar os corações e o GameManager usa Morreu para encerrar a partida.
public class Vida : MonoBehaviour
{
    public int vidaMaxima = 5;

    // (vida atual, vida máxima)
    public event Action<int, int> VidaAlterada;
    public event Action Morreu;

    public int Atual { get; private set; }
    public int Maxima => vidaMaxima;
    public bool Cheia => Atual >= vidaMaxima;
    public bool Morto => Atual <= 0;

    void Awake()
    {
        Atual = vidaMaxima;
    }

    public void Perder(int quantidade = 1)
    {
        if (Morto) return;

        Alterar(-quantidade);
        if (Morto) Morreu?.Invoke();
    }

    // Retorna false se não deu para curar (vida cheia ou já morto)
    public bool Curar(int quantidade = 1)
    {
        if (Morto || Cheia) return false;

        Alterar(quantidade);
        return true;
    }

    private void Alterar(int variacao)
    {
        Atual = Mathf.Clamp(Atual + variacao, 0, vidaMaxima);
        VidaAlterada?.Invoke(Atual, vidaMaxima);
    }
}
