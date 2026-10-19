using UnityEngine;

// Reação do jogador ao dano: perde um coração, é empurrado e fica invencível
// (piscando) por um tempo, para não perder vários corações de uma vez.
//
// Implementa IDanificavel: os inimigos causam dano sem conhecer o jogador.
// Exige Vida e PlayerMovement, que a Unity adiciona junto se faltarem.
[RequireComponent(typeof(PlayerMovement), typeof(Vida))]
public class DanoJogador : MonoBehaviour, IDanificavel
{
    [Tooltip("Tempo invencível depois de levar dano")]
    public float tempoInvencivel = 1.5f;

    [Tooltip("Força do empurrão ao ser atingido por um inimigo")]
    public float forcaEmpurrao = 10f;

    private const float IntervaloPiscar = 0.2f;

    private PlayerMovement movimento;
    private Vida vida;
    private SpriteRenderer sr;
    private float invencivelAte;

    public bool Invencivel => Time.time < invencivelAte;

    void Awake()
    {
        movimento = GetComponent<PlayerMovement>();
        vida = GetComponent<Vida>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Pisca (liga/desliga o sprite) enquanto estiver invencível
        if (sr != null)
        {
            sr.enabled = !Invencivel || Mathf.Repeat(Time.time, IntervaloPiscar) < IntervaloPiscar / 2f;
        }
    }

    public bool ReceberDano(Vector2 direcao)
    {
        // Logo depois de levar dano o jogador fica invencível
        if (Invencivel || vida.Morto) return false;

        invencivelAte = Time.time + tempoInvencivel;

        // Empurra para longe do inimigo (para cima, se ele estiver exatamente em cima)
        movimento.Empurrar((direcao == Vector2.zero ? Vector2.up : direcao.normalized) * forcaEmpurrao);

        // Perde um coração (se for o último, a Vida avisa o GameManager pelo evento Morreu)
        vida.Perder(1);
        return true;
    }
}
