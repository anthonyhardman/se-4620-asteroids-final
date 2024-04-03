import { Route, Routes } from "react-router-dom"
import { Toaster } from "react-hot-toast"
import { Home } from "./pages/home/Home"
import { NavBar } from "./components/NavBar"
import { Lobby } from "./pages/lobby/Lobby"
import { Shop } from "./pages/shop/Shop"
import { WebsocketAsteroidsContext, WebsocketAsteroidsProvider } from "./context/WebsocketAsteroidsContext"
import { useAuth } from "react-oidc-context"
import { useContext, useEffect } from "react"

function App() {
  const auth = useAuth()
  const context = useContext(WebsocketAsteroidsContext)
  
  useEffect(() => {
    if (auth.user) {
      console.log("Registered client with ID: ", auth.user.profile.sub)
      context.registerClient(auth.user.profile.preferred_username ?? "")
      console.log(auth.user.profile.preferred_username)
    }
  }, [auth.user, context])

  return (
    <>
      <Toaster />
      <div className="d-flex flex-column nav-flex">
        <WebsocketAsteroidsProvider>
          <NavBar />
          <div className="overflow-auto flex-grow-1 justify-content-between">
            <Routes>
              <Route path="/" element={<Home />} />
              <Route path="/lobby/:id" element={<Lobby />} />
              <Route path="/shop" element={<Shop />} />
            </Routes>
          </div>
        </WebsocketAsteroidsProvider>
      </div>
    </>
  )
}

export default App
