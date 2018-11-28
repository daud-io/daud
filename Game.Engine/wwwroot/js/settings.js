import { sprites } from "./renderer";
import { img as background, setPattern } from "./background";

import JSZip from "jszip";
var gear = document.getElementById("gear");
document.getElementById("settings").addEventListener("click", function() {
    gear.classList.remove("closed");
});
document.getElementById("closes").addEventListener("click", async function() {
    var v = document.getElementById("mod").value;
    if (v) {
        var link = "https://dl.dropboxusercontent.com/s/" + v + "/daudmod.zip";
        var zip = await fetch(link)
            .then(function(response) {
                return response.blob();
            })
            .then(JSZip.loadAsync);
        zip.file("daudmod/info.json")
            .async("string")
            .then(function(text) {
                var info = JSON.parse(text);
                info.files.forEach(element => {
                    zip.file("daudmod/" + element[0] + ".png")
                        .async("arraybuffer")
                        .then(function(ab) {
                            var arrayBufferView = new Uint8Array(ab);
                            var blob = new Blob([arrayBufferView], { type: "image/jpeg" });
                            var urlCreator = window.URL || window.webkitURL;
                            var url = urlCreator.createObjectURL(blob);
                            if (element[0] == "bg") {
                                background.src = url;
                                background.onload = function() {
                                    setPattern();
                                };
                            } else {
                                sprites[element[0]].image.src = url;
                                if (element[1]) {
                                    sprites[element[0]].scale = element[1];
                                    sprites[element[0]].scaleToSize = !element[0].startsWith("ship");
                                }
                            }
                        });
                });
            });
    }
    gear.classList.add("closed");
});
