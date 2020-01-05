module.exports = {
    paths: {
        public: "dist"
    },
    files: {
        javascripts: {
            joinTo: {
                "app.js": /^app/,
                "vendor.js": /^(?!app)/
            }
        },
        stylesheets: { joinTo: "game.css" }
    },
    npm: {
        enabled: true,
        styles: {
            "emoji-mart": ["css/emoji-mart.css"]
        }
    },
    plugins: {
        babel: {
            presets: [
                [
                    "@babel/preset-env",
                    {
                        targets: {
                            browsers: ["last 2 versions"]
                        }
                    }
                ]
            ]
        },
        eslint: {
            pattern: /^app\/.*\.[jt]sx?$/,
            formatter: ""
        }
    }
};
