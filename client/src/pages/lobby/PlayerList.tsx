import { FC } from "react";
import { LobbyInfo } from "../../models/Lobby";

export const PlayerList: FC<{
  lobbyInfo: LobbyInfo;
}> = ({ lobbyInfo }) => {
  return (
    <div>
      <ul className="list-group">
        <li className="list-group-item bg-secondary-subtle">
          <div className="row w-100">
            <div className="col fw-bold">Player</div>
            <div className="col-2 text-center fw-bold">Health</div>
            <div className="col-2 text-center fw-bold">Points</div>
          </div>
        </li>
        {Object.entries(lobbyInfo.players)
          .sort(([playerA, shipA], [playerB, shipB]) => {
            if (shipA.health > shipB.health) return -1;
            if (shipA.health < shipB.health) return 1;
            return playerA.localeCompare(playerB);
          })
          .map(([player, ship]) => (
            <li key={player} className="list-group-item">
              <div className="row w-100">
                <div className="col">{player}</div>
                <div className="col-2 text-center">{ship.health}</div>
                <div className="col-2 text-center">{ship.points}</div>
              </div>
            </li>
          ))}

      </ul>
    </div>
  );
};
