import { useContext, useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { PlayerList } from './PlayerList';
import { WebsocketAsteroidsContext } from '../../context/WebsocketAsteroidsContext';
import toast from 'react-hot-toast';
import { useStartGameMutation } from './lobbyHooks';

export const Lobby = () => {
  const context = useContext(WebsocketAsteroidsContext);
  const lobbyId = useParams<{ id: string }>().id;
  const startGameMutation = useStartGameMutation()
  const [gameStarting, setGameStarting] = useState(false);
  const [countdown, setCountdown] = useState<number>(0);
  const [countdownInterval, setCountdownInterval] = useState(0);


  const reset = () => {
    setGameStarting(false);
    setCountdown(10);
    clearInterval(countdownInterval);
  };

  useEffect(() => {
    if (context.isConnected && lobbyId) {
      context.joinGroup(lobbyId)
      console.log("Joined group")
    } else {
      console.log("Connection not ready");
    }

    return () => {
      if (context.isConnected && lobbyId) {
        context.leaveGroup(lobbyId)
        console.log("Left group")
      }
    }
  }, [lobbyId, context, context.isConnected])

  useEffect(() => {
    if (context.startedAt) {
      setGameStarting(true);
      let timer = new Date().getUTCDate().valueOf() - new Date(context.startedAt).valueOf();
      setCountdownInterval(setInterval(() => {
        setCountdown(timer - 1);
        if (timer <= 1) {
          clearInterval(countdownInterval);
          toast.success('Game starts now!');
        }
        timer -= 1;
      }, 1000));
    }
  }, [context.startedAt, countdownInterval])

  if (!lobbyId) return <h3 className='text-center'>Unknown Lobby</h3>

  
  const startGame = () => {
    startGameMutation.mutate(lobbyId);
  };

  return (
    <div className="container mt-2 text-center">
      <h1>Waiting in Lobby</h1>
      {gameStarting ? (
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
          <div>The game has not started yet. Customize your ship before the game begins!</div>
          <div className='mt-2'>
            <button className="btn btn-success" onClick={startGame}>Start Game</button>
          </div >
        </>
      )}
      <div className='row my-3'>
        {/* <div className='col'>
          <div className='row text-start'>
            <div className='col'>
              <label htmlFor="shipColorSelect" className="form-label">Ship Color</label>
              <select id="shipColorSelect" className="form-select" value={shipColor} onChange={(e) => setShipColor(e.target.value)}>
                <option>Blue</option>
                <option>Red</option>
                <option>Green</option>
              </select>
            </div>
            <div className='col'>
              <label htmlFor="shipWeaponSelect" className="form-label">Ship Weapon</label>
              <select id="shipWeaponSelect" className="form-select" value={shipWeapon} onChange={(e) => setShipWeapon(e.target.value)}>
                <option>Laser</option>
                <option>Missiles</option>
                <option>Plasma Cannon</option>
              </select>
            </div>
          </div>
        </div> */}
        <div className='col-auto text-start'>
          <PlayerList lobbyId={lobbyId} />
        </div>
      </div>

    </div >
  );
};
