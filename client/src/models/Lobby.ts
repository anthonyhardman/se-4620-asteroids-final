export interface LobbyList {
  lobbies: LobbyInfo[];
}

export enum LobbyState {
  Joining,
  Playing,
  Stopped
}

export interface LobbyInfo {
  id: string;
  createdBy: string;
  playerCount: number;
  maxPlayers: number;
  state: LobbyState;
  players: { [username: string]: PlayerShip }
}

export interface PlayerShip {
  position: Vector;
  velocity: Vector;
  direction: Vector;
  inputState?: InputState;
}

export interface Vector {
  x: number;
  y: number;
}

export interface InputState {
  thrusting: boolean;
  rotationDirection: RotationDirection;
  shootPressed: number;
}

export enum RotationDirection {
  None,
  Left,
  Right
}


export interface CreateLobbyCommand {
  username: string;
} 
