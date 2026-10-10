"""Wire the design mockups (design/mockups/*.html) into a navigable site under server/static/site."""
import re, pathlib
root = pathlib.Path(__file__).resolve().parents[1]; out = root/"server/static/site"; out.mkdir(parents=True, exist_ok=True)
ROUTES = {"bedtime-chamber":"/app/bedtime","morning-dew-harvest":"/app/morning-dew","conservatory-terrarium":"/app/garden","daylight-quest-log":"/app/garden",
  "gardener-folio":"/app/onboarding","night-blocks-ledger":"/app/morning-dew","diurnal-quest-map":"/app/garden","species-codex":"/app/garden",
  "download-app":"/app/onboarding","download":"/app/onboarding","your-garden":"/app/garden","sleep-garden":"/app/garden","explore":"/","web-demo":"/play/"}
# (file, desktop-twin, is_desktop_layout)
PAGES = {"landing-desktop":("landing-mobile",True),"landing-mobile":("landing-desktop",False),"garden-desktop":("garden-mobile",True),"garden-mobile":("garden-desktop",False),
         "onboarding":(None,False),"bedtime":(None,False),"morning-dew":(None,False)}
URL = {"landing-desktop":"/","landing-mobile":"/m","garden-desktop":"/app/garden-desktop","garden-mobile":"/app/garden-mobile"}
def flip(twin, desk):  # responsive hop between the mobile and desktop layouts of the same screen
    cmp = ">=900" if desk is False else "<900"
    return f'<script>if(innerWidth{cmp}&&!location.search.includes("stay"))location.replace("{URL[twin]}")</script>'
LIVE = '<script src="/static/live.js" defer></script>'
for name,(twin,desk) in PAGES.items():
    h = (root/f"design/mockups/{name}.html").read_text(encoding="utf-8")
    h = re.sub(r'(<a [^>]*?)data-path="([^"]+)"([^>]*?)href="#"', lambda m: f'{m[1]}data-path="{m[2]}"{m[3]}href="{ROUTES.get(m[2],"#")}"', h)
    h = re.sub(r'(<a [^>]*?)href="#"([^>]*?)data-path="([^"]+)"', lambda m: f'{m[1]}href="{ROUTES.get(m[3],"#")}"{m[2]}data-path="{m[3]}"', h)
    inj = (flip(twin, desk) if twin else "") + LIVE
    (out/f"{name}.html").write_text(h.replace("</head>", inj+"</head>", 1), encoding="utf-8")
print("site built:", sorted(p.name for p in out.iterdir()))
