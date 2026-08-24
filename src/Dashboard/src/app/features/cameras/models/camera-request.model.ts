export interface CameraRequest {
  name: string;
  brand: string;
  model: string;
  username: string | null;
  password: string | null;
  ipAddress: string | null;
  rtspPort: number;
  ptzPort: number;
  protocol: string;
  siteId: string;
}

export interface UpdateCameraRequest extends CameraRequest {
  id: string;
}
