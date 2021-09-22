const CopyPlugin = require("copy-webpack-plugin");
const path = require("path");
const fs = require("fs");
const HtmlWebpackPlugin = require("html-webpack-plugin");
const { CleanWebpackPlugin } = require("clean-webpack-plugin");
const appDirectory = fs.realpathSync(process.cwd());

module.exports = {
    target: "web",
    entry: path.resolve(appDirectory, "src/game.ts"), //path to the main .ts file
    output: {
        filename: "js/gamebundle.js",
        clean: true,
        path: path.resolve(appDirectory, "dist"),
        publicPath: '/',
    },
    resolve: {
        extensions: [".tsx", ".ts", ".js"],
    },
    devServer: {
        host: "0.0.0.0",
        port: 8080, //port that we're using for local host (localhost:8080)
        disableHostCheck: true,
        contentBase: path.resolve(appDirectory, "public"), //tells webpack to serve from the public folder
        publicPath: "/",
        hot: true,
    },
    module: {
        rules: [
            {
                test: /\.css$/i,
                use: ['style-loader', 'css-loader'],
            },
            {
                test: /\.tsx?$/,
                use: "ts-loader",
                exclude: /node_modules/,
            },
            {
                test: /\.png$/,
                type: 'asset',
                generator: {
                    filename: 'static/[hash][ext][query]'
                }
            }
        ],
    },
    plugins: [
        new HtmlWebpackPlugin({
            inject: true,
            template: path.resolve(appDirectory, "index.html"),
        }),
        new CopyPlugin({
            patterns: [
                { from: "public", to: "." },
            ],
        }),
        new CleanWebpackPlugin(),
    ],
    mode: "development",
};