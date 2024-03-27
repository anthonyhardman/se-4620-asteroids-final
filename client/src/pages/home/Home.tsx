import { Link } from "react-router-dom"

export const Home = () => {
  const lobbies = [1, 2, 3, 4, 5, 6]
  return (
    <div className="container mt-2">
      <div className="row">
        <div className="col offset-2">
          <h1 className="text-center">Join Lobby</h1>
        </div>
        <div className="col-2 my-auto text-end">
          <button className="btn btn-outline-primary">Create</button>
        </div>
      </div>
      <div className="row">
        {lobbies.map((l) => (
          <div key={l} className="col-xl-3 col-12 col-md-6 col-lg-4">
            <div className="card shadow my-1">
              <div className="card-body text-center">
                <div className="card-title fs-4">Lobby {l}</div>
                <div className="mb-2">{GetPlayerText(l)}</div>
                {l < 5 ? (
                  <Link to={`/lobby/${l}`}
                    className="text-reset text-decoration-none btn btn-secondary w-100">
                    Join
                  </Link>
                ) : (
                  <button className="btn btn-outline-secondary w-100" disabled>Full</button>
                )}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}

function GetPlayerText(count: number) {
  if (count === 1) return "1 Player"
  return `${count} Players`
}