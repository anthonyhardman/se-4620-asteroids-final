import { useContext, useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { PlayerList } from "./PlayerList";
import { useGetLobbyInfoQuery, useStartGameMutation } from "./lobbyHooks";
import { Spinner } from "../../components/Spinner";
import { InputState, LobbyState, RotationDirection } from "../../models/Lobby";
import { SignalRContext } from "../../signalR/SignalRContext";
import { Game } from "./Game";

export const Lobby = () => {
  const signalRContext = useContext(SignalRContext);
  const lobbyId = useParams<{ id: string }>().id;
  const startGameMutation = useStartGameMutation();
  const lobbyInfoQuery = useGetLobbyInfoQuery(lobbyId);
  const lobbyInfo = lobbyInfoQuery.data;
  const [keysPressed, setKeysPressed] = useState([] as string[])
  const [inputState, _] = useState<InputState>({
    thrusting: false,
    rotationDirection: RotationDirection.None,
    shootPressed: 0,
  });
  
  useEffect(() => {
    if (lobbyId && signalRContext?.isConnected) {
      signalRContext?.joinGroup(lobbyId);
      console.log("Joined group");
    } else {
      console.log("Connection not ready");
    }

    return () => {
      if (lobbyId && signalRContext?.isConnected) {
        signalRContext?.leaveGroup(lobbyId);
        console.log("Left group");
      }
    };
  }, [lobbyId, signalRContext]);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (keysPressed.includes(e.key)) return;
      setKeysPressed([...keysPressed, e.key]);

      if (e.key === "w") {
        inputState.thrusting = true;
      }

      if (e.key === "a") {
        inputState.rotationDirection = RotationDirection.Left;
      } else if (e.key === "d") {
        inputState.rotationDirection = RotationDirection.Right;
      }

      if (lobbyId && signalRContext?.isConnected) {
        signalRContext?.updatePlayerInput(lobbyId, inputState);
      }
    };

    const handleKeyUp = (e: KeyboardEvent) => {
      if (!keysPressed.includes(e.key)) return;
      setKeysPressed(keysPressed.filter((key) => key !== e.key));

      if (e.key === "w") {
        inputState.thrusting = false;
      }

      if (e.key === "a" || e.key === "d") {
        inputState.rotationDirection = RotationDirection.None;
      }

      if (lobbyId && signalRContext?.isConnected) {
        signalRContext?.updatePlayerInput(lobbyId, inputState);
      }
    };

    window.addEventListener("keydown", handleKeyDown);
    window.addEventListener("keyup", handleKeyUp);

    return () => {
      window.removeEventListener("keydown", handleKeyDown);
      window.removeEventListener("keyup", handleKeyUp);
    };
  }, [lobbyId, signalRContext, keysPressed, inputState]);

  if (lobbyInfoQuery.isLoading) return <Spinner />;
  if (lobbyInfoQuery.isError)
    return <h3 className="text-center">Error getting lobby</h3>;
  if (!lobbyId || !lobbyInfo)
    return <h3 className="text-center">Unknown Lobby</h3>;

  const startGame = () => {
    if (!lobbyId) return;
    startGameMutation.mutate(lobbyId);
  };

  const renderLobbyState = () => {
    if (lobbyInfo.state === LobbyState.Joining) {
      return (
        <>
          <h1>Waiting in Lobby</h1>
          <div>
            The game has not started yet. Customize your ship before the game
            begins!
          </div>
          <div className="mt-2">
            <button className="btn btn-success" onClick={startGame}>
              Start Game
            </button>
          </div>
        </>
      );
    } else if (lobbyInfo.state === LobbyState.Countdown) {
      return (
        <div>
          Game starting in {lobbyInfo.countdownTime} seconds. Customize your
          ship!
        </div>
      );
    } else if (lobbyInfo.state === LobbyState.Playing) {
      return (
        <Game
          players={Object.entries(lobbyInfo.players).map(([_, ship]) => {
            return ship;
          })}
        />
      );
    } else {
      return <div>Unknown state</div>;
    }
  };

  return (
    <div className="container mt-2 text-center">
      {renderLobbyState()}
      <div className="row my-3">
        <div className="col-auto text-start">
          {lobbyInfo && <PlayerList lobbyInfo={lobbyInfo} />}
        </div>
      </div>
    </div>
  );
};
