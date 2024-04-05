import { Link, useNavigate } from "react-router-dom";
import { useCreateLobbyMutation, useGetLobbiesQuery } from "./homeHooks";
import { useUser } from "../../userHooks";

export const Home = () => {
  const user = useUser();
  const navigate = useNavigate();
  const lobbiesQuery = useGetLobbiesQuery();
  const createLobbyMutation = useCreateLobbyMutation();
  const lobbies = lobbiesQuery.data ?? [];

  const hasLobby = lobbies.filter(l => l.createdBy === user?.preferred_username).length > 0

  const createHandler = () => {
    createLobbyMutation.mutateAsync().then(id => navigate(`/lobby/${id}`));
  }

  return (
    <div className="container mt-2">
      <div className="row">
        <div className="col offset-2">
          <h1 className="text-center">Join Lobby</h1>
        </div>
        <div className="col-2 my-auto text-end">
          {!hasLobby && (
            <button
              className="btn btn-outline-primary"
              onClick={createHandler}
            >
              Create
            </button>
          )}
        </div>
      </div>
      <div className="row">
        {lobbies.map((l, index) => (
          <div key={l.id} className="col-xl-3 col-12 col-md-6 col-lg-4">
            <div className="card shadow my-1">
              <div className="card-body text-center">
                <div className="card-title fs-4">Lobby {index + 1}</div>
                <div className="mb-2">{GetPlayerText(l.playerCount)}</div>
                {l.playerCount < l.maxPlayers ? (
                  <Link
                    to={`/lobby/${l.id}`}
                    className="text-reset text-decoration-none btn btn-secondary w-100"
                  >
                    {l.createdBy = user?.preferred_username ? "Joined" : "Join"}
                  </Link>
                ) : (
                  <button className="btn btn-outline-secondary w-100" disabled>
                    Full
                  </button>
                )}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

function GetPlayerText(count: number) {
  if (count === 1) return "1 Player";
  return `${count} Players`;
}
