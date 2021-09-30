import { NetPing } from "./daud-net/net-ping";
import { NetQuantum } from "./daud-net/net-quantum";
import { Builder, ByteBuffer } from "flatbuffers";
import { AllMessages } from "./daud-net/all-messages";

export class PingConnection {
    reloading = false;
    connected = false;
    latency = 0;
    minimumLatency = -1;
    socket?: WebSocket;
    pingSent?: number;
    pongCount: number = 0;

    disconnect(): void {
        if (this.socket) {
            this.socket.close();
        }
    }
    connect(connectString: string): void {
        let url: string = window.location.protocol === "http:" ? "wss:" : "wss:";
        let hostname = "daud.io";
        let worldKey = '';

        const worldKeyParse = connectString.match(/^(.*?)\/(.*)$/);
        if (worldKeyParse) {
            hostname = worldKeyParse[1];
            worldKey = worldKeyParse[2];
        }
        url += `//${hostname}/api/v1/connect?world=${encodeURIComponent(worldKey)}&`;

        this.minimumLatency = -1;

        if (this.socket) {
            this.socket.onclose = null;
            this.socket.close();
        }

        this.socket = new WebSocket(url);
        this.socket.binaryType = "arraybuffer";
        this.socket.onmessage = (event) => this.onMessage(event);
        this.socket.onopen = () => this.onOpen();
        this.socket.onclose = (event) => this.onClose(event);
    }

    sendPing(): void {
        const builder = new Builder(0);

        NetPing.startNetPing(builder);
        this.pingSent = performance.now();

        NetPing.addTime(builder, this.pingSent);
        NetPing.addClienttime(builder, this.pingSent);
        NetPing.addBandwidththrottle(builder, 0);

        const ping = NetPing.endNetPing(builder);

        NetQuantum.startNetQuantum(builder);
        NetQuantum.addMessageType(builder, AllMessages.NetPing);
        NetQuantum.addMessage(builder, ping);
        const quantum = NetQuantum.endNetQuantum(builder);

        builder.finish(quantum);

        this.send(builder.asUint8Array());
    }


    send(databuffer: Uint8Array): void {
        if (this.socket && this.socket.readyState === 1) {
            this.socket.send(databuffer);
        }
    }

    onOpen(): void {
        this.connected = true;
        this.sendPing();
    }

    onClose(event: CloseEvent): void {
        this.connected = false;
    }

    handleNetPing(message: NetPing): void {
        this.pongCount++;
        this.latency = performance.now() - message.clienttime();
        if (this.latency < this.minimumLatency || this.minimumLatency == -1) {
            this.minimumLatency = this.latency;
        }
        setTimeout(() => {
            if (this.connected) {
                this.sendPing();
            }
        }, 250);
    }

    onMessage(event: MessageEvent): void {
        const data = new Uint8Array(event.data);
        const buf = new ByteBuffer(data);
        const quantum = NetQuantum.getRootAsNetQuantum(buf);
        const messageType = quantum.messageType();

        switch (messageType) {
            case AllMessages.NetPing:
                this.handleNetPing(quantum.message(new NetPing()));
                break;
        }
    }
}
