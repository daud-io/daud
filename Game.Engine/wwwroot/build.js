const { build } = require("esbuild");
const fs = require("fs-extra");
const chokidar = require("chokidar");

let prod = process.argv[2] == "production";
function runBuild() {
    build({
        stdio: "inherit",
        entryPoints: ["./src/game.ts"],
        outfile: "dist/main.js",
        sourcemap: "external",
        bundle: true,
        minifyWhitespace: prod,
        minifySyntax: prod,
        define: { global: "window" },
    }).then(() => {
        console.log("Wrote to dist/main.js");
    });
}

function copy() {
    if (!fs.existsSync("dist")) fs.mkdirSync("dist");
    fs.copy("public", "dist", (err) => {
        if (err) throw err;
        console.log("Wrote to dist/index.html");
    });
}

if (!prod) {
    chokidar.watch("src").on("change", runBuild);
    chokidar.watch("public").on("change", copy);
}

runBuild();
copy();
fs.copyFileSync("node_modules/emoji-mart/css/emoji-mart.css", "dist/emoji-mart.css");
