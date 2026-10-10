import { api } from "../api/game-api";
export default function TimeTravel({ onTick }: { onTick: () => void }) {
  return <div className="dev">{[8, 24, 49].map(h => <button key={h} onClick={() => api.dev.advance(h).then(onTick)}>+{h}h</button>)}</div>;
}
