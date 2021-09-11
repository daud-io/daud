import { KeyboardEventTypes, KeyboardInfo } from "@babylonjs/core/Events";
import { EventState } from "@babylonjs/core/Misc";
import { GameContainer } from "./gameContainer";

export class ChatOverlay {
    container: GameContainer;
    chat: HTMLElement;
    messages = ["YES", "NO", "OOPS", "HI", "GO", "LAG", "HMM?", "STOP RUNNING", "GG", "LOL"];

    constructor(container: GameContainer) {
        this.container = container;

        this.chat = document.getElementById("chat")!;
        
        for (let i = 0; i < this.messages.length; i++) {
            this.chat.innerHTML += `<tr><td>${(i + 1) % 10}</td><td>${this.messages[i]}</td></tr>`;
        }

        //this.container.scene.onKeyboardObservable.add((kbInfo, eventState) => this.onKey(kbInfo));
    }

    onKey(kbInfo: KeyboardInfo)
    {
        switch (kbInfo.type) {
            case KeyboardEventTypes.KEYDOWN:
                const e = kbInfo.event;

                if (e.key == "t" && document.body.classList.contains("alive")) {
                    this.chat.classList.toggle("open");
                }
                if ((e.code.startsWith("Digit") || e.code.startsWith("Numpad")) && document.body.classList.contains("alive")) {
                    message.txt = this.messages[(Number(e.key) + 9) % 10] || "";
                    message.time = Date.now();
                    this.chat.classList.remove("open");
                }
                                
                break;
        }        
    }
}

export const message = {
    txt: "",
    time: Date.now(),
};

