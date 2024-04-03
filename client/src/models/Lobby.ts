export interface LobbyList {
  lobbies: LobbyInfo[];
}

export interface LobbyInfo {
  id: string;
  createdBy: string;
  playerCount: number;
  maxPlayers: number;
}
