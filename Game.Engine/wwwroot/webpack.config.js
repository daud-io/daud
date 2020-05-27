const path = require("path");
const CopyPlugin = require("copy-webpack-plugin");
const webpack = require("webpack");
const TerserPlugin = require("terser-webpack-plugin");

module.exports = {
    entry: "./src/game.ts",
    mode: "development",
    devtool: "source-map",
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
    },
    resolve: {
        extensions: [".ts", ".js"],
    },
    output: {
        filename: "[name].js",
        path: path.resolve(__dirname, "dist"),
        pathinfo: false,
    },
    plugins: [new CopyPlugin({ patterns: [{ from: "public", to: "./" }] }), new webpack.ProvidePlugin({ PIXI: "pixi.js" })],
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
        fs: "empty",
    },
};
