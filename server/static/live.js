// Light wiring of the static mockups to the FastAPI game API.
const j=(u,m="GET")=>fetch(u,{method:m}).then(r=>r.json());
document.addEventListener("click",async e=>{
  if(e.target.closest("#tuckInButton")){await j("/api/tuck-in","POST");alert("Tucked in 🌙  Dev: POST /api/dev/advance?hours=8, then open Morning Dew.");}
});
addEventListener("DOMContentLoaded",async()=>{const b=await j("/api/block");if(b.block&&location.pathname.includes("morning"))console.log("Dew ready",b.block)});
