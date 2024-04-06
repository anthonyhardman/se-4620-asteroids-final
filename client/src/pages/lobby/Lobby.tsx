import { useContext, useEffect} from "react";
import { useParams } from "react-router-dom";
import { PlayerList } from "./PlayerList";
import { useGetLobbyInfoQuery, useStartGameMutation } from "./lobbyHooks";
import { Spinner } from "../../components/Spinner";
import { LobbyState } from "../../models/Lobby";
import { SignalRContext } from "../../signalR/SignalRContext";

export const Lobby = () => {
  const signalRContext = useContext(SignalRContext);
  const lobbyId = useParams<{ id: string }>().id;
  const startGameMutation = useStartGameMutation();
  const lobbyInfoQuery = useGetLobbyInfoQuery(lobbyId);
  const lobbyInfo = lobbyInfoQuery.data;

  useEffect(() => {
    if (lobbyId && signalRContext?.isConnected) {
      signalRContext?.joinGroup(lobbyId);
      console.log("Joined group");
    } else {
      console.log("Connection not ready");
    }

    console.log("LobbyId", lobbyId);
    return () => {
      if (lobbyId && signalRContext?.isConnected) {
        signalRContext?.leaveGroup(lobbyId);
        console.log("Left group");
      }
    };
  }, [lobbyId, signalRContext]);

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
      return <div>Playing</div>;
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
