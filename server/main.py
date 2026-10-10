"""SlumberBloom server: serves the website, app screens and the React play-demo on http://localhost:8000
Run: pip install -r requirements.txt && uvicorn main:app --reload --port 8000"""
import hashlib, json, time, pathlib
from fastapi import FastAPI, HTTPException
from fastapi.responses import FileResponse
from fastapi.staticfiles import StaticFiles

BASE = pathlib.Path(__file__).parent / "static"; DB = pathlib.Path(__file__).parent / "state.json"
MIN_MS, MAX_MS = 30*60_000, 10*3_600_000   # dust rule, interval cap (same constants as packages/engine)
app = FastAPI(title="SlumberBloom")

# ---- engine port (mirrors packages/engine: canonical JSON + RFC 6962 Merkle)
H = lambda s: hashlib.sha256(s.encode()).hexdigest()
canon = lambda v: json.dumps(v, sort_keys=True, separators=(",", ":"), ensure_ascii=False)
leaf, node = (lambda d: H("\x00"+d)), (lambda a, b: H("\x01"+a+b))
def levels(items):
    lv = [leaf(i) for i in items]; out = [lv]
    while len(lv) > 1:
        lv = [node(lv[i], lv[i+1]) if i+1 < len(lv) else lv[i] for i in range(0, len(lv), 2)]; out.append(lv)
    return out
def path(items, i):
    p = []
    for lv in levels(items)[:-1]:
        s = i ^ 1
        if s < len(lv): p.append((lv[s], s < i))
        i >>= 1
    return p

def load():
    s = {"offset": 0, "last_seen": None, "last_water": None, "growth": 10, "sun": 3, "moon": 3, "bloom": 0, "blocks": []}
    if DB.exists(): s.update(json.loads(DB.read_text()))
    return s
S = load()
now = lambda: int(time.time()*1000) + S["offset"]
def save(): DB.write_text(json.dumps(S))
def stage(g): return "bloom" if g >= 100 else "bud" if g >= 60 else "sprout" if g >= 25 else "seed"
def wilt(h): return "dormant" if h >= 120 else "brown" if h >= 48 else "droop" if h >= 24 else "healthy"
sealed = lambda: [b["hash"] for b in S["blocks"] if b["stage"] != "dew"]

@app.post("/api/tuck-in")
def tuck_in(): S["last_seen"] = now(); save(); return {"ok": True}
@app.get("/api/block")
def open_block():
    dew = next((b for b in S["blocks"] if b["stage"] == "dew"), None)
    if not dew and S["last_seen"] is not None and now() - S["last_seen"] >= MIN_MS:
        prev = S["blocks"][-1] if S["blocks"] else None
        b = {"height": (prev["height"] if prev else 0)+1, "prev": prev["hash"] if prev else "genesis", "start": S["last_seen"], "end": now(), "ms": min(now()-S["last_seen"], MAX_MS)}
        dew = {**b, "hash": H(canon(b)), "stage": "dew"}; S["blocks"].append(dew); S["last_seen"] = None; save()
    return {"block": dew}
@app.post("/api/blocks/{h}/claim")
def claim(h: str):
    b = next((x for x in S["blocks"] if x["hash"] == h), None)
    if not b: raise HTTPException(404)
    if b["stage"] == "dew": b["stage"] = "gathered"
    save(); return b
@app.post("/api/dawn")
def dawn():
    n = 0
    for b in S["blocks"]:
        if b["stage"] == "gathered": b["stage"] = "frost"; S["bloom"] += round(b["ms"]/3_600_000*10); n += 1
    save(); return {"settled": n, "bloom": S["bloom"]}
@app.get("/api/garden")
def garden():
    wl = S["last_water"] or now()
    return {"growth": S["growth"], "stage": stage(S["growth"]), "wilt": wilt((now()-wl)/3_600_000), "sun": S["sun"], "moon": S["moon"], "bloom": S["bloom"], "blocks": S["blocks"]}
@app.post("/api/garden/water")
def water():
    if S["sun"] > 0: S["sun"] -= 1; S["growth"] = min(100, S["growth"]+25); S["last_water"] = now(); save()
    return garden()
@app.get("/api/lantern/{h}")
def lantern(h: str):
    items = sealed(); root = levels(items)[-1][0] if items else H("")
    if h not in items: return {"ok": False, "root": root}
    i = items.index(h); p = path(items, i); x = leaf(h)
    for sib, left in p: x = node(sib, x) if left else node(x, sib)
    return {"ok": x == root, "root": root, "depth": len(p)}
@app.post("/api/dev/advance")
def advance(hours: float = 8): S["offset"] += int(hours*3_600_000); save(); return {"offset_h": S["offset"]/3_600_000}
@app.post("/api/dev/reset")
def reset(): DB.unlink(missing_ok=True); S.clear(); S.update(load()); return {"ok": True}

# ---- website + app screens
SITE = BASE / "site"
page = lambda n: FileResponse(SITE / f"{n}.html")
app.get("/")(lambda: page("landing-desktop"))
app.get("/m")(lambda: page("landing-mobile"))
for route, f in {"onboarding": "onboarding", "bedtime": "bedtime", "morning-dew": "morning-dew", "garden": "garden-desktop",
                 "garden-desktop": "garden-desktop", "garden-mobile": "garden-mobile"}.items():
    app.get(f"/app/{route}")(lambda f=f: page(f))
app.mount("/static", StaticFiles(directory=BASE), name="static")
app.mount("/play", StaticFiles(directory=BASE / "play", html=True), name="play")   # working React demo (time travel, Lantern)
