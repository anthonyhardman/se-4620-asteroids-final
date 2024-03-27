import { Route, Routes } from "react-router-dom"
// import { NavBar } from "./components/navBar/NavBar"
import { Toaster } from "react-hot-toast"
import { Home } from "./pages/home/Home"

function App() {

  return (
    <>
      <Toaster />
      <div className="d-flex flex-column nav-flex">
        {/* <NavBar /> */}
        <div className="overflow-auto flex-grow-1 justify-content-between">
          <Routes>
            <Route path="/" element={<Home />} />
          </Routes>
        </div>
      </div>
    </>
  )
}

export default App
