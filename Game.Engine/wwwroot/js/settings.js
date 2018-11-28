import { sprites } from "./renderer";
import JSZip from "jszip";
var gear = document.getElementById("gear");
document.getElementById("settings").addEventListener("click", function() {
    gear.classList.remove("closed");
});
console.log(sprites);
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
                    console.log(element);
                    zip.file("daudmod/" + element + ".png")
                        .async("arraybuffer")
                        .then(function(ab) {
                            var arrayBufferView = new Uint8Array(ab);
                            var blob = new Blob([arrayBufferView], { type: "image/jpeg" });
                            var urlCreator = window.URL || window.webkitURL;
                            var url = urlCreator.createObjectURL(blob);
                            var img = new Image();
                            img.src = url;
                            img.onload = function() {
                                sprites[element].img = img;
                                if (info.scale[element]) {
                                    sprites[element].scale = info.scale[element];
                                    sprites[element].scaleToSize = true;
                                }
                            };
                        });
                });
                console.log(info);
            });
    }
    gear.classList.add("closed");
});
