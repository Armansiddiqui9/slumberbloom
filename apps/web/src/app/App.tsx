import { useCallback, useEffect, useState } from "react";
import type { BlockView, Garden, Proof } from "@sb/contracts";
import { api } from "../api/game-api"; import TimeTravel from "../dev/TimeTravel";
const EMOJI: Record<string, string> = { seed: "🌰", sprout: "🌱", bud: "🌿", bloom: "🌻" };
const TABS = [["bed", "🌙", "Bedtime"], ["dew", "💧", "Morning Dew"], ["garden", "🪴", "Garden"], ["census", "🧭", "Census"]] as const;
const hm = (ms: number) => `${Math.floor(ms / 3.6e6)}h ${Math.floor((ms % 3.6e6) / 6e4)}m`;

export default function App() {
  const [tab, setTab] = useState<string>("bed"); const [g, setG] = useState<Garden>(); const [blocks, setBlocks] = useState<BlockView[]>([]);
  const [away, setAway] = useState(false); const [proof, setProof] = useState<Proof>();
  const refresh = useCallback(async () => { await api.openBlock(); setG(await api.getGarden()); setBlocks(await api.getBlocks()); }, []);
  useEffect(() => { refresh(); }, [refresh]);
  const dew = blocks.find(b => b.stage === "dew"), last = blocks.at(-1);
  const act = (f: () => Promise<unknown>) => async () => { await f(); await refresh(); };
  return <>
    <header><b>SlumberBloom</b><span className="pill">Era 0 · Genesis</span></header>
    <main className="shell">
      {tab === "bed" && <section className="card wide">
        <div className="eyebrow">Nocturnal vigil</div><h2>{away ? "Sleep well. Your garden is mining." : "Tuck in, and be patient."}</h2>
        <div className="well">Zero tracking · zero microphone · nothing runs while you're away. Tuck in, slide <b>+8h</b>, then open <b>Morning Dew</b>.</div>
        <button className="btn" onClick={async () => { await api.tuckIn(); setAway(true); }}>🌙 Tuck In Phone & Begin Slumber</button></section>}
      {tab === "dew" && <section className="card wide">
        <div className="eyebrow">While you were away</div>
        {dew ? <><h2>{hm(dew.ms)} of patience hashed into Night Block #{dew.height}</h2><p className="mono">{dew.hash}</p>
          <button className="btn amber" onClick={act(() => api.claimBlock(dew.hash, crypto.randomUUID()))}>💧 Claim Dew</button></>
          : <h2>Nothing yet. Dew forms after 30+ minutes away.</h2>}
        {blocks.some(b => b.stage === "gathered") && <button className="btn" onClick={act(api.settleDawn)}>☀️ Jump to Dawn: settle & mint Frost</button>}
        {last && last.stage !== "dew" && <div className="well"><div className="eyebrow">Block #{last.height} · {last.stage}</div>
          <button className="btn ghost" style={{ marginTop: 8 }} onClick={async () => setProof(await api.lantern(last.hash))}>🏮 Light the Lantern</button>
          {proof && <p className="mono">{proof.ok ? "✓ Verified" : "✗ Failed"} · Merkle depth {proof.depth} · root {proof.root.slice(0, 16)}…</p>}</div>}
      </section>}
      {tab === "garden" && g && <><section className="card"><div className="eyebrow">Ipomoea noctiflora</div><h2>Baby Moonflower</h2>
        <div className="plant">{EMOJI[g.stage]}</div>
        <div className="bar"><i style={{ width: g.growth + "%" }} /></div><span className="pill">{g.stage} · {g.wilt} · {g.growth}/100</span>
        <button className="btn" disabled={g.sun < 1} onClick={act(api.water)}>💧 Water (1 ☀️)</button></section>
        <section className="card"><div className="eyebrow">Ledger</div><h2>{g.bloom} Bloom</h2><div className="well">☀️ {g.sun} Sun · 🌙 {g.moon} Moon</div>
          <p style={{ margin: 0 }}>Plants never die. After 120h unwatered they rest in dormancy.</p></section></>}
      {tab === "census" && <section className="card wide"><div className="eyebrow">Census</div><h2>The ceiling falls out of the Charter.</h2>
        <div className="well">Night blocks: <b>{blocks.length}</b> · Frost: <b>{blocks.filter(b => b.stage === "frost").length}</b><br />World Simulator, Charter Lab and era engine come next (docs/ARCHITECTURE.md §7).</div></section>}
    </main>
    <TimeTravel onTick={refresh} />
    <nav>{TABS.map(([k, e, l]) => <button key={k} className={tab === k ? "on" : ""} onClick={() => { setTab(k); refresh(); }}><span>{e}</span>{l}</button>)}</nav>
  </>;
}
