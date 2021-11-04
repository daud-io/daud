import { NetWorldView } from "./daud-net/net-world-view";
import { NetPing } from "./daud-net/net-ping";
import { NetQuantum } from "./daud-net/net-quantum";
import { Builder, ByteBuffer } from "flatbuffers";
import { AllMessages } from "./daud-net/all-messages";

export class Pinger {
    connected = false;
    minimumLatency = -1;
    socket?: WebSocket;
    pingSent?: number;
    statPongCount = 0;
    builder: Builder = new Builder(1024);

    messageBuffers = {
        Quantum: new NetQuantum(),
        WorldView: new NetWorldView(),
        Ping: new NetPing(),
    };


    constructor() {
        setInterval(() => {
            if (this.connected) {
                this.sendPing();
            }
        }, 250);
    }

    disconnect(): void {
        if (this.socket) {
            this.socket.close();
        }
    }
    connect(worldKey: string): void {
        try
        {
            var url:URL;

            try
            {
                url = new URL(worldKey);
            }
            catch
            {
                url = new URL(`wss://${worldKey}`);
            }
            let apiURL = `${url.protocol}//${url.host}/api/v1/connect?world=${encodeURIComponent(url.pathname?.substr(1))}`;

            if (this.socket) {
                this.onClose();
                this.socket.onclose = null;
                this.socket.onmessage = null;
                this.socket.close();
            }

            this.socket = new WebSocket(apiURL);
            this.socket.binaryType = "arraybuffer";

            this.socket.onmessage = (event) => {
                this.onMessage(event);
            };

            this.socket.onopen = () => {
                this.onOpen();
            };
            this.socket.onclose = () => {
                this.onClose();
            };
        }
        catch(e)
        {
            console.log('bad connection string: ' + worldKey);
        }

    }

    sendPing(): void {
        this.builder.clear();

        NetPing.startNetPing(this.builder);
        this.pingSent = performance.now();

        NetPing.addTime(this.builder, this.pingSent);
        NetPing.addClienttime(this.builder, this.pingSent);
        NetPing.addBackgrounded(this.builder, true);
        NetPing.addBandwidththrottle(this.builder, 0);

        const ping = NetPing.endNetPing(this.builder);

        NetQuantum.startNetQuantum(this.builder);
        NetQuantum.addMessageType(this.builder, AllMessages.NetPing);
        NetQuantum.addMessage(this.builder, ping);
        const quantum = NetQuantum.endNetQuantum(this.builder);

        this.builder.finish(quantum);

        this.send(this.builder.asUint8Array());
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

    onClose(): void {
        this.connected = false;
    }

    handleNetPing(message: NetPing): void {
        this.statPongCount++;

        const latency = performance.now() - message.clienttime();
        if (latency < this.minimumLatency || this.minimumLatency == -1) {
            this.minimumLatency = latency;
        }
    }
    
    onMessage(event: MessageEvent): void {

        var byteArray = new Uint8Array(event.data);
        var buffer = new ByteBuffer(byteArray);
        const quantum = NetQuantum.getRootAsNetQuantum(buffer, this.messageBuffers.Quantum);
        const messageType = quantum.messageType();

        switch (messageType) {
            case AllMessages.NetPing:
                this.handleNetPing(quantum.message(this.messageBuffers.Ping));
                break;
        }
    }
}
