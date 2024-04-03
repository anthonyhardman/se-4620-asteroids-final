import { Route, Routes } from "react-router-dom"
import { Toaster } from "react-hot-toast"
import { Home } from "./pages/home/Home"
import { NavBar } from "./components/NavBar"
import { Lobby } from "./pages/lobby/Lobby"
import { Shop } from "./pages/shop/Shop"

function App() {
  return (
    <>
      <Toaster />
      <div className="d-flex flex-column nav-flex">
        <NavBar />
        <div className="overflow-auto flex-grow-1 justify-content-between">
          <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/lobby/:id" element={<Lobby />} />
            <Route path="/shop" element={<Shop />} />
          </Routes>
        </div>
      </div>
    </>
  )
}

export default App
