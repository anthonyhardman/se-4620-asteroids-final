import { Link } from "react-router-dom";
import { useCreateLobbyMutation, useGetLobbiesQuery } from "./homeHooks";

export const Home = () => {
  const lobbiesQuery = useGetLobbiesQuery();
  const createLobbyMutation = useCreateLobbyMutation();
  const lobbies = lobbiesQuery.data ?? [];

  console.log(lobbies);

  return (
    <div className="container mt-2">
      <div className="row">
        <div className="col offset-2">
          <h1 className="text-center">Join Lobby</h1>
        </div>
        <div className="col-2 my-auto text-end">
          {/* Trying to create a lobby with the same user kind of breaks actor system
          because it tries to create a new actor with the same id. We'll need to figure 
          out how we'd like to handle this. */}
          <button
            className="btn btn-outline-primary"
            onClick={() => createLobbyMutation.mutate()}
          >
            Create
          </button>
        </div>
      </div>
      <div className="row">
        {lobbies.map((l, index) => (
          <div key={l.id} className="col-xl-3 col-12 col-md-6 col-lg-4">
            <div className="card shadow my-1">
              <div className="card-body text-center">
                <div className="card-title fs-4">Lobby {index + 1}</div>
                <div className="mb-2">{GetPlayerText(l.playerCount)}</div>
                {l.playerCount < 5 ? (
                  <Link
                    to={`/lobby/${l.id}`}
                    className="text-reset text-decoration-none btn btn-secondary w-100"
                  >
                    Join
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
