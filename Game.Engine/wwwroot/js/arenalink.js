import { Connection } from "./connection";

const arenaLinkInput = document.getElementById("arena-link-input");
const getUrl = window.location;
const baseUrl = getUrl.protocol + "//" + getUrl.host + "/" + getUrl.pathname.split('/')[1];
const chars = "0123456789abcdefghijklmnopqrstuwvxyzABCDEFGHIJKLMNOPQRSTUWVXYZ";
const base = chars.length;
const arenas = ["us.daud.io/default", "de.daud.io/default", "localhost:5000/default"];

export class ArenaLink {
    constructor() {
        this.generated = false;
    }
    
    generate(worldKey) {
        if (typeof(worldKey) != "undefined") {
            console.log("World key: " + worldKey);
            var d = new Date(),
                time = Math.floor(d.getTime() / 1000),
                arenaIndex = arenas.indexOf(worldKey);    
            if (arenaIndex !== -1) {
                var arenaLink = baseUrl + "#" + this.encode(time + "" + arenaIndex);
                arenaLinkInput.value = arenaLink;
                console.log("Arena link generated: " + arenaLink);
            } else {
                arenaLinkInput.value = getUrl;
            }
            this.generated = true;
        }
    }
    
    encode(num) {
        var encoded = '';
        while (num) {
            var remainder = num % base;
            num = Math.floor(num / base);
            encoded = chars[remainder].toString() + encoded;
        }
        return encoded;
    }
    
    decode(str) {
        var decoded = 0;
        while (str) {
            var index = chars.indexOf(str[0]);
            var power = str.length - 1;
            decoded += index * (Math.pow(base, power));
            str = str.substring(1);
        }
        return decoded;
    }
    
    copy() {
        // Select the text field
        arenaLinkInput.select();
        arenaLinkInput.setSelectionRange(0, 99999); // For mobile devices

        // Copy the text inside the text field
        document.execCommand("copy");
        
        // remove selection
        arenaLinkInput.setSelectionRange(0, 0);
        
        $("#arena-link-success").show();
        setTimeout(function(){
            $("#arena-link-success").fadeOut(1000);
        }, 3000);
    }
    
    getLinkFromURL() {
        var linkInURL;
        var actualWindow;

        if(this.iframeDetection())
        {
            console.log('Window is Iframed');
            actualWindow = parent.window;
        }
        else
        {
            console.log('No IFrame detected');
            actualWindow = window;
        }

        if(actualWindow.location.hash.length > 0)
        {
            console.log('Reading arena Link from URL');
            linkInURL = this.readArenaLinkFromURL(actualWindow.location.hash);

            console.log('Arena Link from URL: ' + linkInURL);
            console.log(this.decode(linkInURL));
        }

        return linkInURL;
    }
    
    readArenaLinkFromURL(hashUrl) {
        var arenaLink = hashUrl.substring(1, hashUrl.length);

        /*if(linkSemanticallyCorrect(arenaLink))
        {
            //Send arena link to maestro
            console.log("Link checks out!");*/
            return arenaLink;
        /*}
        else
        {
            console.log("This link doesn't check out!");
            return undefined;
        }*/
    }
    
    iframeDetection() {
        return window.self !== window.top;
    }
}