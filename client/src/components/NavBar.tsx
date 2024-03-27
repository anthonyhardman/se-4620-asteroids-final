import { useAuth } from "react-oidc-context";
import { Link, useLocation } from "react-router-dom";

export const NavBar = () => {
  const location = useLocation();
  const isActive = (to: string) => {
    if (to === "/" && location.pathname === "/") {
      return true
    }
    if (to !== "/" && location.pathname.includes(to)) {
      return true
    }
  }

  const auth = useAuth();
  const logout = () => {
    auth.removeUser().then(() => auth.signoutRedirect());
  };
  const login = () => {
    auth.signinRedirect();
  };

  return (
    <nav className="navbar navbar-expand-lg navbar-dark bg-black shadow">
      <div className="container">
        <Link to="/" className="navbar-brand">
          Asteroids
        </Link>
        <button
          className="navbar-toggler"
          type="button"
          data-bs-toggle="collapse"
          data-bs-target="#navbarNav"
          aria-controls="navbarNav"
          aria-expanded="false"
          aria-label="Toggle navigation"
        >
          <span className="navbar-toggler-icon"></span>
        </button>
        <div className="collapse navbar-collapse" id="navbarNav">
          <ul className="navbar-nav ms-auto">
            <li className="nav-item">
              <Link to="/" className={`nav-link`}>
                <button className={`btn border-0 fs-5 ${isActive("/") && "active"}`}>
                  <i className="bi-house pe-1" />Home
                </button>
              </Link>
            </li>
            <li className="nav-item">
              <Link to="/shop" className={`nav-link`}>
                <button className={`btn border-0 fs-5 ${isActive("/") && "active"}`}>
                  <i className="bi-shop pe-1" />Shop
                </button>
              </Link>
            </li>
            <li className="nav-item my-auto ms-3">
              {auth.user ? (
                <button className="btn btn-outline-danger"
                  onClick={logout}>Logout</button>
              ) : (
                <button className="btn btn-outline-danger"
                  onClick={login}>Login</button>
              )}
            </li>
          </ul>
        </div>
      </div>
    </nav>
  );
};
