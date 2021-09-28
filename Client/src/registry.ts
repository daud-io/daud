
export type ServerWorld = {
    hook: any,
    worldKey: string,
    advertisedPlayers: number

    host: Host;
    isDefault: boolean;
};

export type Host = {
    worlds: ServerWorld[],
    url: string
}

export class Registry
{
    static server: string = 'https://registry.daud.io/api/v1/registry';
    static async query(): Promise<Host[]> {
        const fetched = await window.fetch(Registry.server, {
            method: "GET",
            headers: {
                "Content-Type": "application/json; charset=utf-8",
            },
        });
        const { response }: { response: Host[] } = await fetched.json();

        return response;
    }
}
