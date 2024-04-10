import { FC, useEffect, useRef } from "react";
import { PlayerShip } from "../../models/Lobby";
import { Quaternion, Vector3, Mesh, MeshPhongMaterial } from "three";
import { Text } from "@react-three/drei";

interface PlayerShipDisplayProps {
  player: PlayerShip;
  username: string;
  model: any;
}

export const PlayerShipDisplay: FC<PlayerShipDisplayProps> = ({
  player,
  username,
  model,
}) => {
  const direction = new Vector3(player.direction.x, player.direction.y, 0);
  const quat = new Quaternion().setFromUnitVectors(
    new Vector3(0, 1, 0),
    direction.normalize()
  );
  const healthPercentage = player.health / player.maxHealth;

  const spaceShipRef = useRef<Mesh>();

  useEffect(() => {
    if (!spaceShipRef.current) return;
    spaceShipRef.current.traverse((child: any) => {
      if (child instanceof Mesh) {
        child.material = new MeshPhongMaterial({ color: player.color })
      }
    });
  }, [player.color]);

  return (
    <group key={username} position={[player.position.x, player.position.y, 0]}>
      {/* <mesh geometry={shipGeometry} quaternion={quat} scale={[50, 50, 1]}>
        <meshBasicMaterial attach="material" color={player.color} />
      </mesh> */}
      <mesh position={[0, 70, 0]} scale={[healthPercentage, 0.5, 1]}>
        <planeGeometry args={[100, 10]} />
        <meshBasicMaterial attach="material" color="green" />
      </mesh>
      <primitive ref={spaceShipRef} object={model} scale={10} quaternion={quat} />
      <Text
        position={[0, 90, 0]}
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
};
