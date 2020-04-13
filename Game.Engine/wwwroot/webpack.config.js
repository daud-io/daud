const path = require("path");
const CopyPlugin = require("copy-webpack-plugin");
const webpack = require("webpack");
const TerserPlugin = require("terser-webpack-plugin");

module.exports = {
    entry: "./src/game.ts",
    mode: "development",
    devtool: "cheap-source-map",
    module: {
        rules: [
            {
                test: /\.ts$/,
                use: "ts-loader",
                exclude: /node_modules/,
            },
            {
                test: /\.css$/,
                use: ["style-loader", "css-loader"],
            },
        ],
        noParse: /browserfs\.js/,
    },
    resolve: {
        extensions: [".ts", ".js"],
        alias: {
            fs: "browserfs/dist/shims/fs.js",
            buffer: "browserfs/dist/shims/buffer.js",
            path: "browserfs/dist/shims/path.js",
            processGlobal: "browserfs/dist/shims/process.js",
            bufferGlobal: "browserfs/dist/shims/bufferGlobal.js",
            bfsGlobal: require.resolve("browserfs"),
        },
    },
    output: {
        filename: "[name].js",
        path: path.resolve(__dirname, "dist"),
        pathinfo: false,
    },
    plugins: [new CopyPlugin([{ from: "public", to: "./" }]), new webpack.ProvidePlugin({ BrowserFS: "bfsGlobal", process: "processGlobal", Buffer: "bufferGlobal", PIXI: "pixi.js" })],
    optimization: {
        minimize: true,
        minimizer: [
            new TerserPlugin({
                cache: true,
                chunkFilter: (chunk) => {
                    // Only uglify the `vendor` chunk
                    return chunk.name === "vendor";
                },
                sourceMap: true,
            }),
        ],
        runtimeChunk: "single",
        namedChunks: false,
        namedModules: false,
        splitChunks: {
            hidePathInfo: true,
            cacheGroups: {
                vendor: {
                    test: /[\\/]node_modules[\\/]/,
                    chunks: "all",
                    name: "vendor",
                },
            },
        },
    },
    node: {
        process: false,
        Buffer: false,
    },
};
