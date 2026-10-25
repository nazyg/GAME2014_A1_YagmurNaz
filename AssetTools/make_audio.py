"""Procedural chiptune SFX + music for GAME2014 A1 (all original)."""
import os, wave
import numpy as np

SR = 44100
OUT = os.path.join(os.path.dirname(__file__), "out", "Audio")
os.makedirs(os.path.join(OUT, "SFX"), exist_ok=True)
os.makedirs(os.path.join(OUT, "Music"), exist_ok=True)
rng = np.random.default_rng(2014)

def write(path, x, vol=0.8):
    x = np.asarray(x, dtype=np.float64)
    peak = np.max(np.abs(x)) or 1.0
    x = x / peak * vol
    data = (np.clip(x, -1, 1) * 32767).astype(np.int16)
    with wave.open(os.path.join(OUT, path), "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes(data.tobytes())

def t_(dur): return np.arange(int(SR * dur)) / SR

def osc(freq, dur, kind="square", duty=0.5):
    """freq can be scalar or array (sweep). Phase-accumulating oscillator."""
    n = int(SR * dur)
    f = np.full(n, freq, dtype=float) if np.isscalar(freq) else np.asarray(freq, float)[:n]
    ph = np.cumsum(f) / SR % 1.0
    if kind == "square": return np.where(ph < duty, 1.0, -1.0)
    if kind == "tri": return 4 * np.abs(ph - 0.5) - 1
    if kind == "saw": return 2 * ph - 1
    return np.sin(2 * np.pi * ph)

def env(n, a=0.005, r=None, curve=2.0):
    e = np.ones(n)
    na = max(1, int(a * SR)); e[:na] = np.linspace(0, 1, na)
    tail = np.linspace(1, 0, n - na) ** curve
    e[na:] = tail
    return e

def noise(dur): return rng.uniform(-1, 1, int(SR * dur))

def lowpass(x, k=0.1):
    y = np.zeros_like(x); acc = 0.0
    for i, v in enumerate(x):
        acc += k * (v - acc); y[i] = acc
    return y

def note(midi): return 440.0 * 2 ** ((midi - 69) / 12)

# ---------------- SFX ----------------
d = 0.14; s = osc(np.geomspace(1400, 380, int(SR * d)), d, "square", 0.25); write("SFX/sfx_shoot.wav", s * env(len(s)), 0.5)
d = 0.18; s = osc(np.geomspace(500, 160, int(SR * d)), d, "square", 0.5); write("SFX/sfx_enemy_shoot.wav", s * env(len(s)), 0.45)
d = 0.7; s = lowpass(noise(d), 0.12) * env(int(SR * d), curve=2.5) + 0.5 * osc(np.geomspace(120, 40, int(SR * d)), d, "tri") * env(int(SR * d), curve=3)
write("SFX/sfx_explosion.wav", s, 0.8)
d = 0.4; s = osc(np.geomspace(600, 90, int(SR * d)), d, "saw") * env(int(SR * d)) + 0.6 * lowpass(noise(d), 0.3) * env(int(SR * d), curve=4)
write("SFX/sfx_player_hit.wav", s, 0.8)
def arp(notes, step, kind="square", duty=0.5):
    out = []
    for m in notes:
        x = osc(note(m), step, kind, duty); out.append(x * env(len(x), curve=1.2))
    return np.concatenate(out)
write("SFX/sfx_coin.wav", arp([83, 88], 0.07, duty=0.25), 0.5)
write("SFX/sfx_star.wav", arp([72, 76, 79, 84, 88], 0.06, duty=0.25), 0.5)
write("SFX/sfx_heart.wav", arp([67, 71, 74, 79], 0.08, "tri"), 0.6)
write("SFX/sfx_click.wav", arp([84], 0.05, duty=0.5), 0.4)
write("SFX/sfx_back.wav", arp([79, 72], 0.05, duty=0.5), 0.4)
write("SFX/sfx_gameover.wav", arp([67, 63, 60, 55, 48], 0.18, "tri"), 0.7)

# ---------------- Music ----------------
def render_song(bpm, bars, chords, lead_fn, bass_kind="tri", drums=True, lead_duty=0.25, swing=0):
    beat = 60 / bpm; step = beat / 4  # 16th note grid
    steps = bars * 16
    n = int(round(steps * step * SR))
    mix = np.zeros(n)
    def add(sig, start_step, gain):
        i = int(round(start_step * step * SR)); j = min(n, i + len(sig))
        mix[i:j] += sig[:j - i] * gain
    for bar in range(bars):
        root = chords[bar % len(chords)]
        # bass: 8th notes, root/octave
        for k in range(8):
            m = root - 24 + (12 if k % 2 else 0)
            x = osc(note(m), step * 2 * 0.9, bass_kind); add(x * env(len(x), curve=0.8), bar * 16 + k * 2, 0.35)
        # pad arpeggio (triad) on 16ths, quiet
        triad = [root, root + (3 if root in MINOR else 4), root + 7]
        for k in range(16):
            m = triad[k % 3] + 12
            x = osc(note(m), step * 0.9, "square", 0.125); add(x * env(len(x), curve=2), bar * 16 + k, 0.06)
        # lead
        for (pos, m, length) in lead_fn(bar, root):
            x = osc(note(m), step * length * 0.92, "square", lead_duty)
            vib = 1.0
            add(x * env(len(x), a=0.01, curve=0.7) * vib, bar * 16 + pos, 0.16)
        if drums:
            for k in range(16):
                if k % 8 == 0:
                    x = osc(np.geomspace(160, 45, int(SR * 0.12)), 0.12, "sine"); add(x * env(len(x), curve=1.5), bar * 16 + k, 0.55)
                if k % 8 == 4:
                    x = lowpass(noise(0.12), 0.5); add(x * env(len(x), curve=2), bar * 16 + k, 0.25)
                if k % 2 == 0:
                    x = noise(0.03) ; add(x * env(len(x), curve=3), bar * 16 + k, 0.06)
    return mix

MINOR = {57, 62, 64, 69, 52, 50}
scale_a_minor = [57, 59, 60, 62, 64, 65, 67, 69, 71, 72, 74, 76]

def lead_menu(bar, root):
    pat = [[(0, 69, 3), (4, 72, 2), (6, 71, 2), (8, 69, 4), (12, 64, 4)],
           [(0, 65, 3), (4, 69, 2), (6, 72, 2), (8, 74, 6), (14, 72, 2)],
           [(0, 72, 3), (4, 76, 2), (6, 74, 2), (8, 72, 4), (12, 67, 4)],
           [(0, 71, 4), (4, 72, 2), (6, 74, 2), (8, 76, 8)]]
    return pat[bar % 4]

def lead_game(bar, root):
    pats = [[(0, 76, 2), (2, 76, 1), (3, 74, 1), (4, 72, 2), (6, 74, 2), (8, 76, 2), (10, 79, 2), (12, 76, 4)],
            [(0, 77, 2), (2, 76, 2), (4, 74, 2), (6, 72, 2), (8, 74, 4), (12, 71, 4)],
            [(0, 72, 2), (2, 72, 1), (3, 74, 1), (4, 76, 2), (6, 79, 2), (8, 81, 3), (11, 79, 1), (12, 76, 4)],
            [(0, 74, 2), (2, 76, 2), (4, 74, 2), (6, 71, 2), (8, 69, 8)]]
    return pats[bar % 4]

def lead_over(bar, root):
    pats = [[(0, 69, 6), (8, 67, 4), (12, 64, 4)], [(0, 65, 8), (8, 64, 8)],
            [(0, 62, 6), (8, 64, 4), (12, 65, 4)], [(0, 64, 16)]]
    return pats[bar % 4]

menu = render_song(100, 8, [57, 53, 60, 55], lead_menu, drums=True)
game = render_song(140, 8, [57, 53, 60, 55], lead_game, bass_kind="square", drums=True)
over = render_song(72, 4, [57, 53, 50, 52], lead_over, drums=False, lead_duty=0.5)
write("Music/music_menu.wav", menu, 0.6)
write("Music/music_game.wav", game, 0.6)
write("Music/music_gameover.wav", over, 0.6)
print("audio done", os.listdir(os.path.join(OUT, "SFX")), os.listdir(os.path.join(OUT, "Music")))
