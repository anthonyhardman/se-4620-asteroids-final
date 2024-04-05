import * as signalR from "@microsoft/signalr";
import {
  FC,
  ReactNode,
  createContext,
  useEffect,
  useRef,
  useState,
} from "react";
import { useAuth } from "react-oidc-context";
import toast from "react-hot-toast";
import { getQueryClient } from "../services/queryClient";
import { HomeKeys } from "../pages/home/homeHooks";
import { LobbyInfo, LobbyState } from "../models/Lobby";

interface WebsocketAsteroidsContextType {
  joinGroup: (group: string) => void;
  leaveGroup: (group: string) => void;
  isConnected: boolean;
  startedAt?: Date;
  createLobby: () => void;
  lobbyInfo?: LobbyInfo;
  playing: boolean;
  startPlayingCountdown: (lobbyId: string) => void;
}

export const WebsocketAsteroidsContext =
  createContext<WebsocketAsteroidsContextType>({
    joinGroup: () => { },
    leaveGroup: () => { },
    isConnected: false,
    startedAt: undefined,
    createLobby: () => { },
    lobbyInfo: undefined,
    playing: false,
    startPlayingCountdown: () => { }
  });

export const WebsocketAsteroidsProvider: FC<{
  children: ReactNode;
}> = ({ children }) => {
  const auth = useAuth();
  const [isConnected, setIsConnected] = useState(false);
  const [startedAt, setStartedAt] = useState<Date>();
  const [lobbyInfo, setLobbyInfo] = useState<LobbyInfo>()
  const [playing, setPlaying] = useState(lobbyInfo?.state == LobbyState.Playing);
  const connection = useRef<signalR.HubConnection | null>(null);
  const actionQueue = useRef<Array<() => void>>([]);
  const queryClient = getQueryClient();

  useEffect(() => {
    console.log("Connecting to the WebSocket server...");

    if (window.location.hostname === "localhost") {
      const serverUrl = "http://localhost:8081/ws";
      connection.current = new signalR.HubConnectionBuilder()
        .withUrl(serverUrl)
        .build();
    } else {
      const serverUrl = "/ws";
      connection.current = new signalR.HubConnectionBuilder()
        .withUrl(serverUrl)
        .build();
    }

    connection.current
      .start()
      .then(() => {
        console.log("Connected to the WebSocket server.");
        setIsConnected(true);
        actionQueue.current.forEach((action) => action());
        actionQueue.current = [];
      })
      .catch((error) => console.error("WebSocket Error: ", error));

    connection.current.on("LobbyCreated", () => {
      queryClient.invalidateQueries({
        queryKey: HomeKeys.lobbies,
      });
    });

    connection.current.on("GameStarting", (startedAt: Date) => {
      console.log("Starting game")
      setStartedAt(startedAt);
    })

    connection.current.on("GameStarted", () => {
      console.log("Started game")
      setPlaying(true);
    })

    connection.current.on("UpdateLobbyInfo", (info: LobbyInfo) => {
      console.log("Recieved update")
      setLobbyInfo(info)
    })

    connection.current.onclose = () => {
      console.log("Disconnected from the server.");
    };

    return () => {
      connection.current?.stop().then(() => setIsConnected(false));
    };
  }, [queryClient]);


  const executeOrQueueAction = (action: () => void) => {
    if (isConnected) {
      action();
    } else {
      actionQueue.current.push(action);
    }
  };

  const joinGroup = (group: string) => {
    executeOrQueueAction(() =>
      connection.current
        ?.invoke("JoinGroup", group)
        .catch((error) => console.error("Error joining group:", error))
    );
  };

  const leaveGroup = (group: string) => {
    if (
      isConnected &&
      connection.current &&
      connection.current.state === signalR.HubConnectionState.Connected
    ) {
      connection.current
        .invoke("LeaveGroup", group)
        .catch((error) => console.error("Error leaving group:", error));
    }
  };

  const createLobby = () => {
    executeOrQueueAction(() =>
      connection.current
        ?.invoke("CreateLobby", auth.user?.profile.sub)
        .catch((error) => {
          console.error(error);
          toast.error("Error creating lobby");
        })
    );
  };

  const startPlayingCountdown = (lobbyId: string) => {
    executeOrQueueAction(() =>
      connection.current
        ?.invoke("GameStarting", lobbyId)
        .catch((error) => {
          console.error(error);
          toast.error("Error starting lobby");
        })
    );
  }

  return (
    <WebsocketAsteroidsContext.Provider
      value={{
        joinGroup,
        leaveGroup,
        isConnected,
        startedAt,
        createLobby,
        lobbyInfo,
        playing,
        startPlayingCountdown
      }}
    >
      {children}
    </WebsocketAsteroidsContext.Provider>
  );
};
