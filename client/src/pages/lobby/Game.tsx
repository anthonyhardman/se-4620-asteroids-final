import { OrthographicCamera } from "@react-three/drei";
import { Canvas, useFrame } from "@react-three/fiber";
import { FC } from "react";
import { PlayerShip } from "../../models/Lobby";
import { BufferAttribute, BufferGeometry, Quaternion, Vector3 } from "three";

interface GameProps {
  players: PlayerShip[];
}

export const Game: FC<GameProps> = ({ players }) => {
  const Scene = () => {
    useFrame(() => {});

    const triangleVertices = new Float32Array([
      0, 0.5, 0, -0.5, -0.5, 0, 0.5, -0.5, 0,
    ]);

    const geometry = new BufferGeometry();
    geometry.setAttribute("position", new BufferAttribute(triangleVertices, 3));

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
        {players.map((player, index) => {
          const direction = new Vector3(
            player.direction.x,
            player.direction.y,
            0
          );
          const quat = new Quaternion().setFromUnitVectors(
            new Vector3(0, 1, 0),
            direction.normalize()
          );

          return (
            <mesh
              key={index}
              quaternion={quat}
              scale={[50, 50, 1]}
              position={[player.position.x, player.position.y, 0]}
            >
              <bufferGeometry attach="geometry">
                <bufferAttribute
                  attach={"attributes-position"}
                  count={triangleVertices.length / 3} // 3 vertices
                  array={triangleVertices}
                  itemSize={3} // 3 values (x, y, z) per vertex
                />
              </bufferGeometry>
              <meshBasicMaterial attach="material" color="blue" />
            </mesh>
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
