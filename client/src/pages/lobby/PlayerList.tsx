import { FC } from "react";
import { LobbyInfo } from "../../models/Lobby";
import { useUser } from "../../userHooks";

export const PlayerList: FC<{
  lobbyInfo: LobbyInfo;
}> = ({ lobbyInfo }) => {
  const user = useUser();
  return (
    <div>
      <div className="fs-4 text-center">Other Players</div>
      <ul className="list-group">
        {Object.entries(lobbyInfo.players)
          .filter(([player, _ /*ship*/]) => player !== user?.preferred_username)
          .map(([player, _ /*ship*/]) => (
            <li key={player} className="list-group-item">
              <strong>{player}</strong>
            </li>
          ))}
      </ul>
    </div>
  );
};
