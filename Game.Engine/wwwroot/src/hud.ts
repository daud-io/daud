const hudh = document.getElementById("hud")!;

let latency: number;
let framesPerSecond: number;
let playerCount: number;
let spectatorCount: number;
let minlatency: number;

function update(): void {
    hudh.innerText = `fps: ${framesPerSecond || 0} - \
                          players: ${playerCount || 0} - \
                          spectators: ${spectatorCount || 0} - \
                          ping: ${latency || 0} min:${minlatency}`;
}
export function setPerf(l: number, ml:number, f: number): void {
    if (latency == Math.floor(l) && framesPerSecond == f) return;
    latency = Math.floor(l);
    minlatency = Math.floor(ml);
    framesPerSecond = f;
    update();
}
export function setSpectatorCount(s: number): void {
    if (spectatorCount == s) return;
    spectatorCount = s;
    update();
}
export function setPlayerCount(p: number): void {
    if (p == playerCount) return;
    playerCount = p;
    window.document.title = playerCount > 0 ? `Daud.io (${playerCount})` : "Daud.io";
}
