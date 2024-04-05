import { useContext, useEffect, useRef, useState } from 'react';
import { useParams } from 'react-router-dom';
import { PlayerList } from './PlayerList';
import { WebsocketAsteroidsContext } from '../../context/WebsocketAsteroidsContext';
import toast from 'react-hot-toast';
import { useGetLobbyInfoQuery, useStartGameMutation } from './lobbyHooks';
import { Spinner } from '../../components/Spinner';
import { LobbyState } from '../../models/Lobby';

export const Lobby = () => {
  const context = useContext(WebsocketAsteroidsContext);
  const lobbyId = useParams<{ id: string }>().id;
  const startGameMutation = useStartGameMutation();
  const lobbyInfoQuery = useGetLobbyInfoQuery(lobbyId);
  const lobbyInfo = context.lobbyInfo && context.lobbyInfo.id === lobbyId
    ? context.lobbyInfo
    : lobbyInfoQuery.data
  const [gameStarting, setGameStarting] = useState(false);
  const [countdown, setCountdown] = useState(10);
  const countdownIntervalRef = useRef<number>();

  const reset = () => {
    setGameStarting(false);
    setCountdown(10);
    if (countdownIntervalRef.current) clearInterval(countdownIntervalRef.current);
  };

  useEffect(() => {
    if (context.isConnected && lobbyId) {
      context.joinGroup(lobbyId);
      console.log("Joined group");
    } else {
      console.log("Connection not ready");
    }

    return () => {
      if (context.isConnected && lobbyId) {
        context.leaveGroup(lobbyId);
        console.log("Left group");
      }
    };
  }, [lobbyId, context, context.isConnected]);

  useEffect(() => {
    if (context.startedAt) {
      setGameStarting(true);
      const start = new Date(context.startedAt).getTime();
      const now = Date.now();
      const diffInSeconds = Math.floor((now - start) / 1000);
      let timer = 10 - diffInSeconds;
      setCountdown(timer);

      countdownIntervalRef.current = setInterval(() => {
        if (timer > 0) {
          setCountdown(timer - 1);
          timer -= 1;
        } else {
          clearInterval(countdownIntervalRef.current);
          toast.success('Game starts now!');
          setGameStarting(false);
        }
      }, 1000);
    }

    return () => {
      if (countdownIntervalRef.current) clearInterval(countdownIntervalRef.current);
    };
  }, [context.startedAt]);

  if (lobbyInfoQuery.isLoading) return <Spinner />
  if (lobbyInfoQuery.isError) return <h3 className='text-center'>Error getting lobby</h3>
  if (!lobbyId || !lobbyInfo) return <h3 className='text-center'>Unknown Lobby</h3>

  const startGame = () => {
    startGameMutation.mutate(lobbyId);
  };
  return (
    <div className="container mt-2 text-center">
      <h1>Waiting in Lobby</h1>
      {(gameStarting) ? (
        <>
          <div className="alert alert-warning" role="alert">
            Game starting in {countdown} seconds. Complete your customizations!
          </div>
          <div className='mt-2'>
            <button className="btn btn-danger" onClick={() => reset()}>Cancel</button>
          </div>
        </>
      ) : (
        <>
          {lobbyInfo.state === LobbyState.Joining ? (
            <>
              <div>The game has not started yet. Customize your ship before the game begins!</div>
              <div className='mt-2'>
                <button className="btn btn-success" onClick={startGame}>Start Game</button>
              </div >
            </>
          ) : (
            <div>Playing</div>
          )}
        </>
      )}
      <div className='row my-3'>
        <div className='col-auto text-start'>
          {lobbyInfo && (
            <PlayerList lobbyInfo={lobbyInfo} />
          )}
        </div>
      </div>

    </div >
  );
};
