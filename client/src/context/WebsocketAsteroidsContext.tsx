import * as signalR from "@microsoft/signalr";
import { FC, ReactNode, createContext, useEffect, useRef, useState } from "react";

interface WebsocketAsteroidsContextType {
  joinGroup: (group: string) => void;
  leaveGroup: (group: string) => void;
  registerClient: (client: string) => void;
  isConnected: boolean;
}

export const WebsocketAsteroidsContext = createContext<WebsocketAsteroidsContextType>({
  joinGroup: () => { },
  leaveGroup: () => { },
  registerClient: () => { },
  isConnected: false
});

export const WebsocketAsteroidsProvider: FC<{
  children: ReactNode
}> = ({ children }) => {
  const [isConnected, setIsConnected] = useState(false);
  const connection = useRef<signalR.HubConnection | null>(null);
  const actionQueue = useRef<Array<() => void>>([]);

  useEffect(() => {
    console.log("Connecting to the WebSocket server...");

    if (window.location.hostname === 'localhost') {
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

    connection.current.start().then(() => {
      console.log("Connected to the WebSocket server.");
      setIsConnected(true);
      actionQueue.current.forEach(action => action());
      actionQueue.current = [];
    }).catch((error) => console.error("WebSocket Error: ", error));

    connection.current.onclose = () => {
      console.log("Disconnected from the server.");
    };

    return () => {
      connection.current?.stop().then(() => setIsConnected(false));
    };
  }, []);

  const executeOrQueueAction = (action: () => void) => {
    if (isConnected) {
      action();
    } else {
      actionQueue.current.push(action);
    }
  };

  const joinGroup = (group: string) => {
    executeOrQueueAction(() => connection.current?.invoke("JoinGroup", group)
      .catch((error) => console.error("Error joining group:", error)));
  };

  const leaveGroup = (group: string) => {
    if (isConnected && connection.current && connection.current.state === signalR.HubConnectionState.Connected) {
      connection.current.invoke("LeaveGroup", group).catch((error) => console.error("Error leaving group:", error));
    }
  };

  const registerClient = (client: string) => {
    executeOrQueueAction(() => connection.current?.invoke("RegisterClient", client).catch((error) => console.error("Error registering client:", error)))
  }

  return (
    <WebsocketAsteroidsContext.Provider value={{ joinGroup, leaveGroup, registerClient, isConnected }}>
      {children}
    </WebsocketAsteroidsContext.Provider>
  );
} 