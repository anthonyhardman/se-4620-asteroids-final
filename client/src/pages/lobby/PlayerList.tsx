import { FC } from 'react';
import { LobbyInfo } from '../../models/Lobby';

export const PlayerList: FC<{
  lobbyInfo: LobbyInfo
}> = ({ lobbyInfo }) => {
  console.log(lobbyInfo)
  return (
    <div>
      <div className='fs-4 text-center'>Other Players</div>
      <ul className="list-group">
        {/* {lobbyInfo.players.map((player, index) => (
          <li key={index} className="list-group-item">
            <strong>{player.name}</strong>: Color - {player.color}, Weapon - {player.weapon}
          </li>
        ))} */}
      </ul>
    </div>
  );
};

