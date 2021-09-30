import { Connection }  from './connection';
import { GameContainer }  from './gameContainer';
import platform from 'platform';
import { folder } from 'jszip';

export class Telemetry
{
    static lastSent = performance.now();
    static reportInterval = 60000;

    static shouldSend(): boolean
    {
        return (performance.now() - this.lastSent > this.reportInterval);
    }

    static send(container: GameContainer, connection: Connection)
    {
        const report = {
            glinfo: container.engine.getGlInfo(),
            glversion: container.engine.webGLVersion,
            fps: container.engine.getFps(),
            size: {w: container.engine.getRenderWidth(), h: container.engine.getRenderHeight()},
            hostInformation: container.engine.hostInformation,
            fullscreen: container.engine.isFullscreen,
            latency: connection.latency,
            minLatency: connection.minimumLatency,
            ripple: connection.ripple,
            CPU: connection.statViewCPUPerSecond,
            touchscreen: ('ontouchstart' in window || navigator.maxTouchPoints),
            platform: platform
        }

        window.fetch('https://registry.daud.io/api/v1/telemetry/', {
            method: 'POST',
            mode: 'cors', 
            cache: 'no-cache',
            headers: {
                'Content-Type': 'application/json'
            },
            redirect: 'follow',
            body: JSON.stringify(report)
        });
        this.lastSent = performance.now();

    }
}