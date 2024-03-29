import { FC } from 'react';

export const PlayerList: FC<{
  lobbyId: number
}> = ({ lobbyId }) => {
  console.log(lobbyId)
  const players = [
    { name: 'Player 1', color: 'Blue', weapon: 'Laser' },
    { name: 'Player 2', color: 'Red', weapon: 'Missiles' },
    { name: 'Player 3', color: 'Green', weapon: 'Plasma Cannon' },
  ];

  return (
    <div>
      <div className='fs-4 text-center'>Other Players</div>
      <ul className="list-group">
        {players.map((player, index) => (
          <li key={index} className="list-group-item">
            <strong>{player.name}</strong>: Color - {player.color}, Weapon - {player.weapon}
          </li>
        ))}
      </ul>
    </div>
  );
};

