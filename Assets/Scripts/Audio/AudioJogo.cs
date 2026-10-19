using UnityEngine;

// Música de fundo e efeitos sonoros do jogo.
//
// Padrão Observer: o áudio só escuta os eventos do GameManager (item coletado) e da Vida
// do jogador (perdeu ou recuperou coração). Nenhum script de jogo sabe que existe som:
// para tirar ou trocar o áudio, basta mexer aqui.
public class AudioJogo : MonoBehaviour
{
    private const float VolumeMusica = 0.35f;
    private const float VolumeEfeitos = 0.8f;

    private GameManager jogo;
    private Vida vida;
    private AudioSource musica;
    private AudioSource efeitos;
    private AudioClip somItem;
    private AudioClip somCura;
    private AudioClip somDano;
    private int vidaAnterior;

    void Start()
    {
        jogo = GetComponent<GameManager>();
        vida = jogo.VidaJogador;

        // Os arquivos ficam em Assets/Resources/Audio (gerados por Ferramentas/gerar_audio.py)
        somItem = Recursos.Audio("SomItem");
        somCura = Recursos.Audio("SomCura");
        somDano = Recursos.Audio("SomDano");

        // Duas fontes: uma para a música em loop e outra para os efeitos,
        // que podem tocar vários ao mesmo tempo (PlayOneShot) sem cortar a música
        musica = gameObject.AddComponent<AudioSource>();
        musica.clip = Recursos.Audio("MusicaTropical");
        musica.loop = true;
        musica.volume = VolumeMusica;
        musica.playOnAwake = false;

        efeitos = gameObject.AddComponent<AudioSource>();
        efeitos.playOnAwake = false;

        // A música começa assim que o jogo abre (já no menu) e continua durante as fases.
        // O áudio não depende do Time.timeScale, então toca mesmo com o jogo pausado.
        if (musica.clip != null) musica.Play();

        jogo.ItemFoiColetado += AoColetarItem;
        if (vida != null)
        {
            vidaAnterior = vida.Atual;
            vida.VidaAlterada += AoMudarVida;
        }
    }

    void OnDestroy()
    {
        if (jogo != null) jogo.ItemFoiColetado -= AoColetarItem;
        if (vida != null) vida.VidaAlterada -= AoMudarVida;
    }

    private void AoColetarItem()
    {
        Tocar(somItem);
    }

    // Compara com a vida anterior: diminuiu = dano, aumentou = coração coletado
    private void AoMudarVida(int atual, int maxima)
    {
        if (atual < vidaAnterior) Tocar(somDano);
        else if (atual > vidaAnterior) Tocar(somCura);
        vidaAnterior = atual;
    }

    private void Tocar(AudioClip clip)
    {
        if (clip != null) efeitos.PlayOneShot(clip, VolumeEfeitos);
    }
}
