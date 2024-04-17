import { FC } from "react";
import { Vector3, Quaternion } from "three";
import { Bullet } from "../../models/Lobby";


interface BulletDisplayProps {
  bullet: Bullet;
  model: any;
}

export const BulletDisplay: FC<BulletDisplayProps> = ({ bullet, model }) => {
  const direction = new Vector3(bullet.direction.x, bullet.direction.y, 0);
  const quat = new Quaternion().setFromUnitVectors(new Vector3(0, 1, 0), direction.normalize());
  return (
    <group position={[bullet.position.x, bullet.position.y, 0]}>
      <primitive object={model} quaternion={quat} scale={5} />
    </group>
  );
};
