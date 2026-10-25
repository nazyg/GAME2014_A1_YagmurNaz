"""Procedural pixel-art generator for GAME2014 A1 (Cosmic Warden).
All art is original. Palette: Sweetie 16 (CC0)."""
import os, random, math
from PIL import Image

OUT = os.path.join(os.path.dirname(__file__), "out", "Sprites")
os.makedirs(OUT, exist_ok=True)
random.seed(2014)

P = {
    '.': None,
    'c': (26, 28, 44), 'B': (65, 166, 246), 'D': (59, 93, 201), 'C': (115, 239, 247),
    'W': (244, 244, 244), 'O': (239, 125, 87), 'Y': (255, 205, 117), 'R': (177, 62, 83),
    'r': (93, 39, 93), 'P': (93, 39, 93), 'p': (177, 62, 83), 'G': (56, 183, 100),
    'g': (37, 113, 121), 'S': (148, 176, 194), 's': (86, 108, 134), 'k': (51, 60, 87),
    'V': (140, 80, 200), 'v': (90, 50, 140),
}

def mirror(rows, odd=False):
    out = []
    for r in rows:
        right = r[:-1][::-1] if odd else r[::-1]
        out.append(r + right)
    return out

def img_from(rows, pal=P):
    h, w = len(rows), len(rows[0])
    for r in rows:
        assert len(r) == w, (r, w)
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    for y, r in enumerate(rows):
        for x, ch in enumerate(r):
            col = pal[ch]
            if col: im.putpixel((x, y), col + (255,))
    return im

def save(im, name):
    im.save(os.path.join(OUT, name + ".png"))

# ---------- Player ship (16x20, 3 thruster frames) ----------
ship = [
    ".......c", "......cW", "......cW", ".....cBW", ".....cBW", "....cBBC",
    "....cBBC", "...cBBBC", ".c.cBBBB", "cBccBBBB", "cBBBBBBB", "cBBBBBDD",
    "cBBccBDD", ".cc..cBB", "......cO", "........",
]
flames = [
    [".......Y", ".......O", "........", "........"],
    [".......Y", ".......Y", ".......O", "........"],
    ["......YY", "......OY", ".......O", ".......R"],
]
for i, f in enumerate(flames):
    save(img_from(mirror(ship + f)), f"player_{i}")

# ---------- Chaser enemy (14x12, wings flap) ----------
chaser = [
    ["c......", "Rc.....", "RRc..cc", "cRRccRR", ".cRRRRR", "..cRRRR",
     "..cRRWc", "..cRRRR", "...cRRR", "...cRRc", "....cR.", ".....c."],
    [".......", ".......", ".....cc", "c...cRR", "RcccRRR", "RRRRRRR",
     "cRRRRWc", ".ccRRRR", "...cRRR", "...cRRc", "....cR.", ".....c."],
]
for i, f in enumerate(chaser):
    save(img_from(mirror(f)), f"chaser_{i}")

# ---------- Shooter enemy (18x12, lights blink) ----------
shooter = ["......ccc", ".....cCCC", "....cCCWC", "....cCCCC", "..ccVVVVV", ".cVVVVVVV",
           "cVVYVVYVV", "cVVVVVVVV", ".cvvvvvvv", "..ccvvvcc", "....cOc..", ".....c..."]
save(img_from(mirror(shooter)), "shooter_0")
save(img_from(mirror([r.replace('Y', 'O') for r in shooter])), "shooter_1")

# ---------- Bullets ----------
save(img_from(["cWc", "CWC", "CWC", "CCC", "BCB", "BCB", ".B.", ".B."]), "bullet_player")
save(img_from([".cYc.", "cYWYc", "YWWWY", "cOYOc", ".cOc."]), "bullet_enemy")

# ---------- Explosion (24x24, 6 frames) ----------
def explosion(frame, n=6, size=24):
    im = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    cx = cy = size / 2 - 0.5
    t = frame / (n - 1)
    outer = 3 + t * 9
    inner = max(0, (t - 0.35) * 14)
    rnd = random.Random(frame)
    for y in range(size):
        for x in range(size):
            d = math.hypot(x - cx, y - cy) + rnd.uniform(-1.2, 1.2)
            if inner <= d <= outer:
                k = (d - inner) / max(0.01, outer - inner)
                if t > 0.8 and rnd.random() < 0.5:
                    continue
                col = P['W'] if (k < 0.3 and t < 0.4) else P['Y'] if k < 0.55 else P['O'] if k < 0.8 else P['R']
                if t > 0.6: col = P['s'] if k > 0.6 else P['O'] if k < 0.3 else P['R']
                im.putpixel((x, y), col + (255,))
    return im
for i in range(6):
    save(explosion(i), f"explosion_{i}")

# ---------- Coin (10x10, 4 frames spin) ----------
def coin(wid):
    im = Image.new("RGBA", (10, 10), (0, 0, 0, 0))
    cx, cy = 4.5, 4.5
    rx, ry = wid / 2, 4.6
    for y in range(10):
        for x in range(10):
            v = ((x - cx) / max(rx, 0.5)) ** 2 + ((y - cy) / ry) ** 2
            if v <= 1:
                col = P['c'] if v > 0.7 else P['Y'] if x - cx < 0.5 else P['O']
                if wid >= 8 and abs(x - cx) < 1 and abs(y - cy) < 2.5: col = P['W']
                im.putpixel((x, y), col + (255,))
    return im
for i, w in enumerate([10, 6, 2, 6]):
    save(coin(w), f"coin_{i}")

# ---------- Star pickup (13x13, 2 frames twinkle) ----------
def star(bright):
    im = Image.new("RGBA", (13, 13), (0, 0, 0, 0))
    cx = cy = 6
    pts = []
    for k in range(10):
        ang = -math.pi / 2 + k * math.pi / 5
        r = 6.2 if k % 2 == 0 else 2.7
        pts.append((cx + r * math.cos(ang), cy + r * math.sin(ang)))
    def inside(px, py):
        c = False
        j = len(pts) - 1
        for i in range(len(pts)):
            xi, yi = pts[i]; xj, yj = pts[j]
            if ((yi > py) != (yj > py)) and (px < (xj - xi) * (py - yi) / (yj - yi + 1e-9) + xi):
                c = not c
            j = i
        return c
    for y in range(13):
        for x in range(13):
            if inside(x, y + 0.3):
                edge = not all(inside(x + dx, y + 0.3 + dy) for dx, dy in [(1, 0), (-1, 0), (0, 1), (0, -1)])
                col = P['O'] if edge else (P['W'] if bright and abs(x - cx) + abs(y - cy) < 2 else P['Y'])
                im.putpixel((x, y), col + (255,))
    return im
save(star(True), "star_0"); save(star(False), "star_1")

# ---------- Hearts ----------
heart = [".cc..", "cRRc.", "cWRRc", "cRRRR", ".cRRR", "..cRR", "...cR", "....c"]
save(img_from(mirror(heart, odd=True)), "heart")
save(img_from(mirror([r.replace('R', 'k').replace('W', 's') for r in heart], odd=True)), "heart_empty")

# ---------- Background starfield (208x288) ----------
W, H = 208, 288
bg = Image.new("RGBA", (W, H), (0, 0, 0, 255))
for y in range(H):
    for x in range(W):
        t = y / H
        base = (24, 25, 46)  # flat colour so the tile loops seamlessly
        bg.putpixel((x, y), base + (255,))
# nebula blobs
for _ in range(5):
    bx, by, br = random.randint(0, W), random.randint(0, H), random.randint(25, 55)
    tint = random.choice([(41, 54, 111), (70, 34, 80), (30, 60, 80)])
    for y in range(by - br, by + br):
        for x in range(bx - br, bx + br):
            d = math.hypot(x - bx, y - by) / br
            if d < 1 and random.random() < (1 - d) * 0.55:
                xx, yy = x % W, y % H  # wrap so the tile loops seamlessly
                r, g, b, a = bg.getpixel((xx, yy))
                bg.putpixel((xx, yy), (min(255, (r + tint[0]) // 2 + 6), min(255, (g + tint[1]) // 2 + 4), min(255, (b + tint[2]) // 2 + 8), 255))
for _ in range(260):
    x, y = random.randrange(W), random.randrange(H)
    c = random.choice([(86, 108, 134), (148, 176, 194), (148, 176, 194), (244, 244, 244), (115, 239, 247), (255, 205, 117)])
    bg.putpixel((x, y), c + (255,))
    if random.random() < 0.06:  # cross-shaped bright star
        for dx, dy in [(1, 0), (-1, 0), (0, 1), (0, -1)]:
            bg.putpixel(((x + dx) % W, (y + dy) % H), (148, 176, 194, 255))
save(bg, "background")

# ---------- UI: 9-slice button & panel ----------
def frame_img(w, h, fill, light, dark, border=(26, 28, 44)):
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    for y in range(h):
        for x in range(w):
            corner = (x in (0, w - 1)) and (y in (0, h - 1))
            if corner: continue
            if x in (0, w - 1) or y in (0, h - 1): col = border
            elif y == 1: col = light
            elif y >= h - 3: col = dark
            else: col = fill
            im.putpixel((x, y), col + (255,))
    return im
save(frame_img(24, 16, (59, 93, 201), (115, 239, 247), (41, 54, 111)), "ui_button")
save(frame_img(24, 24, (37, 40, 66), (86, 108, 134), (26, 28, 44), border=(148, 176, 194)), "ui_panel")
bar = Image.new("RGBA", (4, 4), (255, 255, 255, 255)); save(bar, "ui_white")

# ---------- Instruction icon: finger (12x16) ----------
finger = ["....cc......", "...cWWc.....", "...cWWc.....", "...cWWc.....", "...cWWccc...",
          "...cWWcWWcc.", ".cccWWcWWcWc", "cWWcWWWWWcWc", "cWWWWWWWWWWc", ".cWWWWWWWWWc",
          ".cWWWWWWWWWc", "..cWWWWWWWc.", "..cWWWWWWWc.", "...cWWWWWc..", "...cccccc...", "............"]
save(img_from(finger), "icon_finger")
print("sprites:", sorted(os.listdir(OUT)))
