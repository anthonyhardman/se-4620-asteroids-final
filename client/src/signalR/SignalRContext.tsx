import { HubConnection, HubConnectionBuilder, HubConnectionState } from "@microsoft/signalr";
import {
  FC,
  ReactNode,
  createContext,
  useEffect,
  useRef,
  useState,
} from "react";
import { InputState, LobbyInfo } from "../models/Lobby";
import { HomeKeys } from "../pages/home/homeHooks";
import { getQueryClient } from "../services/queryClient";
import { LobbyKeys } from "../pages/lobby/lobbyHooks";
import { useUser } from "../userHooks";

interface SignalRConnectionContextType {
  joinGroup: (group: string) => void;
  leaveGroup: (group: string) => void;
  isConnected: boolean;
  updatePlayerInput : (lobbyId: string, inputState: InputState) => void;
}

export const SignalRContext = createContext<
  SignalRConnectionContextType | undefined
>(undefined);

export const SignalRConnectionProvider: FC<{ children: ReactNode }> = ({
  children,
}) => {
  const [connection, setConnection] = useState<HubConnection>();
  const queryClient = getQueryClient();
  const queue = useRef<Array<() => void>>([]);
  const [isConnected, setIsConnected] = useState(false)
  const user = useUser();

  useEffect(() => {
    const createConnection = () =>
      new HubConnectionBuilder()
        .withUrl("/ws")
        .withAutomaticReconnect()
        .build();

    if (!connection) {
      const newConnection = createConnection();
      setConnection(newConnection);

      newConnection
        .start()
        .then(() => {
          console.log("Connection established");
          console.log(newConnection.state);
          setIsConnected(newConnection.state === HubConnectionState.Connected)
          queue.current.forEach((action) => action());
          queue.current = [];
        })
        .catch((error) =>
          console.error("Error establishing connection", error)
        );
    }

    connection?.on("LobbyCreated", (lobbies: LobbyInfo[]) => {
      queryClient.setQueryData(HomeKeys.lobbies, lobbies);
    });

    connection?.on("UpdateLobbyInfo", (lobby: LobbyInfo) => {
      queryClient.setQueryData(LobbyKeys.lobby(lobby.id), lobby);
    });

    connection?.onclose(() => {
      console.log("Connection closed");
      setIsConnected(false)
    });

    return () => {
      if (connection) {
        connection
          .stop()
          .then(() => {
            console.log("Connection stopped");
          })
          .catch((error) => {
            console.error("Error stopping connection", error);
          });
      }
    };
  }, [connection, queryClient]);

  const executeOrQueueAction = (action: () => void) => {
    console.log("Executing or queueing action");
    if (connection?.state === HubConnectionState.Connected) {
      action();
    } else {
      queue.current.push(action);
    }
  };

  const joinGroup = (group: string) => {
    executeOrQueueAction(() =>
      connection
        ?.invoke("JoinGroup", group)
        .then(() => console.log(`Joined group ${group}`))
        .catch((error) => console.error(`Error joining group ${group}`, error))
    );
  };

  const leaveGroup = (group: string) => {
    executeOrQueueAction(() =>
      connection
        ?.invoke("LeaveGroup", group)
        .then(() => console.log(`Left group ${group}`))
        .catch((error) => console.error(`Error leaving group ${group}`, error))
    );
  };

  const updatePlayerInput = (lobbyId: string, inputState: InputState) => {
    executeOrQueueAction(() =>
      connection
        ?.invoke("UpdatePlayerInputState", user?.preferred_username, lobbyId, inputState)
        .catch((error) => console.error(`Error updating player input state`, error)
    ));
  }

  return (
    <SignalRContext.Provider value={{ joinGroup, leaveGroup, isConnected, updatePlayerInput }}>
      {children}
    </SignalRContext.Provider>
  );
};
