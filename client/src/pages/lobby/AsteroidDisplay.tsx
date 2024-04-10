import { FC } from "react";
import { Vector3, Quaternion } from "three";
import { Asteroid } from "../../models/Lobby";

interface AsteroidDisplayProps {
  asteroid: Asteroid;
  model: any;
}

export const AsteroidDisplay: FC<AsteroidDisplayProps> = ({ asteroid, model }) => {
  const scaleFactor = 50; // Adjust this factor to scale the asteroid size up

  const direction = new Vector3(asteroid.direction.x, asteroid.direction.y, 0);
  const quat = new Quaternion().setFromUnitVectors(new Vector3(0, 1, 0), direction.normalize());

  return (
    <group position={[asteroid.position.x, asteroid.position.y, 0]} scale={[scaleFactor * asteroid.size, scaleFactor * asteroid.size, scaleFactor * asteroid.size]}>
      <primitive object={model} quaternion={quat} />
    </group>
  );
};
