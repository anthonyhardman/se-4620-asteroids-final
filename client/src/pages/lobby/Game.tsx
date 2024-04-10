import { OrthographicCamera, useGLTF } from "@react-three/drei";
import { Canvas, useFrame } from "@react-three/fiber";
import { FC } from "react";
import { LobbyInfo } from "../../models/Lobby";
import { PlayerShipDisplay } from "./PlayerShipDisplay";
import { AsteroidDisplay } from "./AsteroidDisplay";
import shipModelPath from "../../3dModels/Fighter_01.glb?url";

interface GameProps {
  lobbyInfo: LobbyInfo;
}

export const Game: FC<GameProps> = ({ lobbyInfo }) => {
  const { players, asteroids } = lobbyInfo;

  const Scene = () => {
    useFrame(() => {});
    const { scene } = useGLTF(shipModelPath, true);

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
        <pointLight decay={0.0} position={[-1000, 125, 400]} />
        {Object.entries(players).map(([username, player]) => {
          const clonedScene = scene.clone(true);

          return (
            <PlayerShipDisplay
              key={username}
              player={player}
              username={username}
              model={clonedScene}
            />
          );
        })}
        {asteroids.map((asteroid, index) => (
          <AsteroidDisplay key={index} asteroid={asteroid} />
        ))}
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
