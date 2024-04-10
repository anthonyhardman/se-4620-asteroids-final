import { OrthographicCamera } from "@react-three/drei";
import { Canvas, useFrame } from "@react-three/fiber";
import { FC } from "react";
import { PlayerShip } from "../../models/Lobby";
import { PlayerShipDisplay } from "./PlayerShip";

interface GameProps {
  players: { [username: string]: PlayerShip };
}

export const Game: FC<GameProps> = ({ players }) => {
  
  
  const Scene = () => {
    useFrame(() => {});

    return (
      <>
        <OrthographicCamera
          makeDefault
          position={[0, 0, 500]}
          left={-400 * 3}
          right={400 * 3}
          top={300 * 3}
          bottom={-300 * 3}
          near={0.1}
          far={1000}
        />
        <ambientLight intensity={0.0} />
        <pointLight decay={0.0} position={[-1000, 125, 200]} />
        {Object.entries(players).map(([username, player]) => {
          return (
           <PlayerShipDisplay key={username} player={player} username={username} />
          );
        })}
      </>
    );
  };

  return (
    <Canvas
      style={{ backgroundColor: "#000", width: "800px", height: "600px" }}
    >
      <Scene />
    </Canvas>
  );
};
