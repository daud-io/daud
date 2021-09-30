const hudh = document.getElementById("hud")!;

let latency: number;
let framesPerSecond: number;
let playerCount: number;
let spectatorCount: number;
let minlatency: number;
let viewCPU: number;

let originalTitle = window.document.title;

function update(): void {
    hudh.innerText = `fps: ${framesPerSecond || 0} - \
                          players: ${playerCount || 0} - \
                          spectators: ${spectatorCount || 0} - \
                          ping: ${latency || 0}`;
}

export function setPerf(l: number, ml: number, f: number, cpu: number): void {
    viewCPU = Math.floor(cpu * 10);
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
    window.document.title = playerCount > 0 ? `DAUD | (${playerCount})` : originalTitle;
}