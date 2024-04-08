import { OrthographicCamera, Text } from "@react-three/drei";
import { Canvas, useFrame } from "@react-three/fiber";
import { FC, useMemo } from "react";
import { BufferAttribute, BufferGeometry, Quaternion, Vector3 } from "three";
import { PlayerShip } from "../../models/Lobby";

interface GameProps {
  players: { [username: string]: PlayerShip };
}

export const Game: FC<GameProps> = ({ players }) => {
  const Scene = () => {
    useFrame(() => { });

    const shipGeometry = useMemo(() => {
      const geometry = new BufferGeometry();
      const vertices = new Float32Array([
        0, 0.5, 0,  // Tip of the ship
        -0.5, -0.5, 0,  // Left base
        0.5, -0.5, 0   // Right base
      ]);
      geometry.setAttribute('position', new BufferAttribute(vertices, 3));
      return geometry;
    }, []);

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
        <ambientLight intensity={0.5} />
        <pointLight decay={0.25} position={[0, 5, 3]} />
        {Object.entries(players).map(([username, player]) => {
          const direction = new Vector3(player.direction.x, player.direction.y, 0);
          const quat = new Quaternion().setFromUnitVectors(new Vector3(0, 1, 0), direction.normalize());
          const healthPercentage = player.health / player.maxHealth;

          return (
            <group key={username} position={[player.position.x, player.position.y, 0]}>
              <mesh
                geometry={shipGeometry}
                quaternion={quat}
                scale={[50, 50, 1]}
              >
                <meshBasicMaterial attach="material" color={player.color} />
              </mesh>
              <mesh
                position={[0, 30, 0]}
                scale={[healthPercentage, 0.5, 1]}
              >
                <planeGeometry args={[100, 10]} />
                <meshBasicMaterial attach="material" color="green" />
              </mesh>
              <Text
                position={[0, 60, 0]}
                fontSize={25}
                fontWeight="bold"
                color="#fff"
                anchorX="center"
                anchorY="middle"
              >
                {username}
              </Text>
            </group>
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
