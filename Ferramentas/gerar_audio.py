# Gera a música de fundo e os efeitos sonoros do jogo (arquivos WAV), sem bibliotecas externas.
#
# Uso (na pasta do projeto):  python Ferramentas/gerar_audio.py
# Saída: Assets/Resources/Audio/*.wav
#
# A música é um loop de 16 compassos (32 s a 120 BPM) em estilo chiptune tropical:
# marimba em arpejo, baixo em onda triangular, percussão e melodia em onda quadrada,
# sobre a progressão C - Am - F - G. Os efeitos são sons curtos sintetizados.
import math, random, struct, wave, os

TAXA = 22050                      # amostras por segundo (suficiente para som de 8 bits)
SAIDA = 'Assets/Resources/Audio'
random.seed(2026)


def freq(nota_midi):
    return 440.0 * 2 ** ((nota_midi - 69) / 12)


def envelope(t, duracao, ataque=0.005, soltura=0.03):
    # sobe e desce rápido nas pontas para não estalar
    if t < ataque:
        return t / ataque
    if t > duracao - soltura:
        return max(0.0, (duracao - t) / soltura)
    return 1.0


# ---------- instrumentos: valor da onda no instante t (segundos desde o início da nota) ----------
def quadrada(f, t, ciclo=0.25):
    return 1.0 if (f * t) % 1.0 < ciclo else -1.0


def triangular(f, t):
    return 4.0 * abs((f * t) % 1.0 - 0.5) - 1.0


def marimba(f, t):
    return math.sin(2 * math.pi * f * t) * math.exp(-t * 7) + 0.3 * math.sin(2 * math.pi * 4 * f * t) * math.exp(-t * 22)


def bumbo(t):
    f = 50 + 70 * math.exp(-t * 30)                  # frequência caindo: "tum"
    return math.sin(2 * math.pi * f * t) * math.exp(-t * 12)


def chimbal(t):
    return (random.random() * 2 - 1) * math.exp(-t * 45)


def misturar(buffer, inicio, duracao, gerador, volume):
    a = int(inicio * TAXA)
    for i in range(int(duracao * TAXA)):
        if a + i >= len(buffer):
            break
        t = i / TAXA
        buffer[a + i] += gerador(t) * envelope(t, duracao) * volume


def salvar(nome, buffer, pico=0.85):
    maior = max(abs(v) for v in buffer) or 1.0
    os.makedirs(SAIDA, exist_ok=True)
    with wave.open(os.path.join(SAIDA, nome + '.wav'), 'wb') as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(TAXA)
        w.writeframes(b''.join(struct.pack('<h', int(v / maior * pico * 32767)) for v in buffer))
    print(f'{nome}.wav: {len(buffer) / TAXA:.2f} s')


# ---------- música ----------
def musica():
    bpm = 120
    tempo = 60 / bpm                 # duração de uma batida (semínima)
    colcheia = tempo / 2
    compassos = 16
    buffer = [0.0] * int(compassos * 4 * tempo * TAXA)

    acordes = {  # notas do acorde (MIDI) e baixo
        'C': ([72, 76, 79], 48), 'Am': ([69, 72, 76], 45),
        'F': ([65, 69, 72], 41), 'G': ([67, 71, 74], 43),
    }
    progressao = ['C', 'Am', 'F', 'G'] * 4

    # melodia em colcheias: (nota, duração em colcheias); None = pausa
    frase_a = [[(76, 2), (79, 1), (81, 1), (79, 2), (76, 2)],
               [(72, 2), (76, 2), (74, 1), (72, 1), (69, 2)],
               [(69, 1), (72, 1), (74, 2), (77, 2), (76, 1), (74, 1)],
               [(74, 3), (71, 1), (74, 2), (79, 2)]]
    frase_b = [[(84, 2), (81, 1), (79, 1), (76, 2), (79, 2)],
               [(81, 3), (79, 1), (76, 2), (72, 2)],
               [(77, 2), (76, 1), (74, 1), (72, 2), (69, 2)],
               [(71, 2), (74, 2), (79, 4)]]
    final = frase_a[:3] + [[(74, 2), (76, 2), (72, 4)]]
    melodia = [None] * 4 + frase_a + frase_b + final   # 4 compassos de introdução sem melodia

    for c, nome in enumerate(progressao):
        notas, baixo = acordes[nome]
        inicio = c * 4 * tempo
        # marimba: arpejo do acorde, uma nota por colcheia
        arpejo = [notas[0], notas[1], notas[2], notas[0] + 12, notas[2], notas[1], notas[0], notas[1]]
        for k, n in enumerate(arpejo):
            misturar(buffer, inicio + k * colcheia, colcheia * 0.95, lambda t, f=freq(n): marimba(f, t), 0.20)
        # baixo: fundamental, fundamental, quinta, fundamental
        for k, n in enumerate([baixo, baixo, baixo + 7, baixo]):
            misturar(buffer, inicio + k * tempo, tempo * 0.9, lambda t, f=freq(n): triangular(f, t), 0.28)
        # percussão: chimbal no contratempo sempre; bumbo nos tempos 1 e 3 depois da introdução
        for k in range(8):
            if k % 2 == 1:
                misturar(buffer, inicio + k * colcheia, 0.08, chimbal, 0.07)
        if c >= 4:
            for k in (0, 2):
                misturar(buffer, inicio + k * tempo, 0.3, bumbo, 0.40)
        # melodia
        if melodia[c]:
            pos = 0
            for n, dur in melodia[c]:
                misturar(buffer, inicio + pos * colcheia, dur * colcheia * 0.92,
                         lambda t, f=freq(n): quadrada(f, t) * (0.85 + 0.15 * math.sin(2 * math.pi * 5 * t)), 0.10)
                pos += dur
    salvar('MusicaTropical', buffer, pico=0.8)


# ---------- efeitos ----------
def som_item():
    # "plim" de moeda: duas notas rápidas subindo
    buffer = [0.0] * int(0.25 * TAXA)
    misturar(buffer, 0.0, 0.07, lambda t: quadrada(freq(83), t, 0.5), 0.5)
    misturar(buffer, 0.06, 0.18, lambda t: quadrada(freq(88), t, 0.5) * math.exp(-t * 14), 0.5)
    salvar('SomItem', buffer)


def som_cura():
    # coração: arpejo subindo (dó, mi, sol, dó)
    buffer = [0.0] * int(0.4 * TAXA)
    for k, n in enumerate([84, 88, 91, 96]):
        dur = 0.22 if k == 3 else 0.07
        misturar(buffer, k * 0.06, dur, lambda t, f=freq(n): triangular(f, t) * math.exp(-t * 8), 0.6)
    salvar('SomCura', buffer)


def som_dano():
    # dano: tom grave descendo, misturado com ruído
    buffer = [0.0] * int(0.32 * TAXA)

    def golpe(t):
        f = 160 + 440 * math.exp(-t * 14)
        fase = 2 * math.pi * (160 * t + 440 * (1 - math.exp(-t * 14)) / 14)  # integral da frequência
        tom = 1.0 if math.sin(fase) > 0 else -1.0
        return (0.7 * tom + 0.3 * (random.random() * 2 - 1)) * math.exp(-t * 9)
    misturar(buffer, 0.0, 0.3, golpe, 0.6)
    salvar('SomDano', buffer)


if __name__ == '__main__':
    musica()
    som_item()
    som_cura()
    som_dano()
