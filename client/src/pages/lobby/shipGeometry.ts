import { BufferAttribute, BufferGeometry } from "three";



const shipGeometry = new BufferGeometry();
const vertices = new Float32Array([
  0, 0.5, 0,  // Tip of the ship
  -0.5, -0.5, 0,  // Left base
  0.5, -0.5, 0   // Right base
]);
shipGeometry.setAttribute('position', new BufferAttribute(vertices, 3));
export default shipGeometry;