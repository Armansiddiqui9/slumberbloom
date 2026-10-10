import { defineConfig } from "vite"; import react from "@vitejs/plugin-react"; import { fileURLToPath as f } from "node:url";
const p = (x: string) => f(new URL("../../packages/" + x, import.meta.url));
export default defineConfig({ base: "/play/", base: "/play/", plugins: [react()], resolve: { alias: { "@sb/engine": p("engine/src"), "@sb/contracts": p("contracts/src"), "@sb/adapters-local": p("adapters-local/src") } } });
