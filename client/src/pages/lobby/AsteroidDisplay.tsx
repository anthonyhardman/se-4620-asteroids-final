import { FC, useMemo } from "react";
import { Vector3, Quaternion, BufferGeometry, BufferAttribute } from "three";
import { Asteroid } from "../../models/Lobby";

interface AsteroidDisplayProps {
  asteroid: Asteroid;
}

export const AsteroidDisplay: FC<AsteroidDisplayProps> = ({ asteroid }) => {
  const scaleFactor = 50; // Adjust this factor to scale the asteroid size up

  const asteroidGeometry = useMemo(() => {
    const geometry = new BufferGeometry();
    const vertices = new Float32Array([
      0, 1, 0,   // Tip of the asteroid
      -1, -1, 0, // Left base
      1, -1, 0   // Right base
    ]);
    geometry.setAttribute('position', new BufferAttribute(vertices, 3));
    return geometry;
  }, []);

  const direction = new Vector3(asteroid.direction.x, asteroid.direction.y, 0);
  const quat = new Quaternion().setFromUnitVectors(new Vector3(0, 1, 0), direction.normalize());
  const healthPercentage = asteroid.health / (asteroid.size * asteroid.size);
  const color = healthPercentage > 0.5 ? "green" : "red";

  return (
    <group position={[asteroid.position.x, asteroid.position.y, 0]} scale={[scaleFactor * asteroid.size, scaleFactor * asteroid.size, scaleFactor * asteroid.size]}>
      <mesh geometry={asteroidGeometry} quaternion={quat}>
        <meshBasicMaterial attach="material" color={color} />
      </mesh>
    </group>
  );
};
