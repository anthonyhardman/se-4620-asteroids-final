import { OrthographicCamera, useGLTF } from "@react-three/drei";
import { Canvas, useFrame } from "@react-three/fiber";
import { FC } from "react";
import { LobbyInfo } from "../../models/Lobby";
import { PlayerShipDisplay } from "./PlayerShipDisplay";
import { AsteroidDisplay } from "./AsteroidDisplay";
import shipModelPath from "../../3dModels/Fighter_01.glb?url";
import asteroidpModelPath from "../../3dModels/asteroid_1.glb?url";

interface GameProps {
  lobbyInfo: LobbyInfo;
}

export const Game: FC<GameProps> = ({ lobbyInfo }) => {
  const { players, asteroids } = lobbyInfo;

  const Scene = () => {
    useFrame(() => {});
    const { scene: shipModel } = useGLTF(shipModelPath, true);
    const { scene: asteroidModel } = useGLTF(asteroidpModelPath, true);
    const playersThatArentDead = Object.entries(players).filter(([_, player]) => player.health > 0);

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
        <ambientLight intensity={0.1} />
        <pointLight decay={0.0} position={[-1000, 125, 500]} />
        {playersThatArentDead.map(([username, player]) => {
          const clonedShipModel = shipModel.clone(true);

          return (
            <PlayerShipDisplay
              key={username}
              player={player}
              username={username}
              model={clonedShipModel}
            />
          );
        })}
        {asteroids.map((asteroid, index) => {
          const clonedAsteroidModel = asteroidModel.clone(true);
          return <AsteroidDisplay key={index} asteroid={asteroid} model={clonedAsteroidModel} />;
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
