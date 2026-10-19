// Dados enviados no evento PartidaTerminou, usados pela tela de fim de jogo
public class ResultadoPartida
{
    public MotivoFim Motivo;
    public bool Venceu => Motivo == MotivoFim.Vitoria;
    public float TempoFinal;
    public float TempoRestante;
    public int ItensColetados;
    public int ItensTotal;
    public int Fase;
    public int TotalFases;
    public float MelhorTempo;
    public bool NovoRecorde;
}
