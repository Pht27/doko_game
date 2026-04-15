import * as signalR from '@microsoft/signalr';

const BASE_URL = import.meta.env.VITE_API_URL ?? '';

export function createHubConnection(token: string): signalR.HubConnection {
  return new signalR.HubConnectionBuilder()
    .withUrl(`${BASE_URL}/hubs/game`, {
      accessTokenFactory: () => token,
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Warning)
    .build();
}

export async function joinGameGroup(
  connection: signalR.HubConnection,
  gameId: string,
): Promise<void> {
  await connection.invoke('JoinGame', gameId);
}
