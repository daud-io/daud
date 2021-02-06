const fs = require("fs-extra");
const chokidar = require("chokidar");

let prod = process.argv[2] == "production";

function copy() {
    if (!fs.existsSync("dist")) fs.mkdirSync("dist");
    fs.copy("public", "dist", (err) => {
        if (err) throw err;
        console.log("Copied public to dist");
    });
}
copy();

let build = require("esbuild").build({
    entryPoints: ["./src/game.ts"],
    outdir: "dist",
    sourcemap: !prod,
    bundle: true,
    minify: prod,
    format: "esm",
    splitting: true,
    target: ["safari12"],
    define: { global: "window", "process.env.NODE_ENV": prod ? '"prudocution"' : '"development"' },
    incremental: !prod,
});

function log() {
    console.log("Wrote to dist/game.js");
}

if (!prod) {
    build.then((x) => chokidar.watch("src").on("change", () => x.rebuild().then(log)));
    chokidar.watch("public").on("change", copy);
}

build.then(log);
