import { FC } from "react";
import { LobbyInfo } from "../../models/Lobby";

export const PlayerList: FC<{
  lobbyInfo: LobbyInfo;
}> = ({ lobbyInfo }) => {
  return (
    <div>
      <div className="fs-4 text-center">Players</div>
      <ul className="list-group">
        {Object.entries(lobbyInfo.players)
          .map(([player, ship]) => (
            <li key={player} className="list-group-item">
              <div className="row w-100">
                <div className="col">{player}</div>
                <div className="col-auto">{ship.health}</div>
              </div>
            </li>
          ))}
      </ul>
    </div>
  );
};
