import { Brand, SupportedCameraBrand } from "../types";

export type CameraProtocol = "Http" | "Https";

export interface Camera extends CameraAuth {
  id: string;
  name: string;
  brand: Brand;
  ipAddress: string;
  port: number;
  protocol: CameraProtocol;
}

export interface CameraAuth {
  username: string;
  password: string;
}

export interface CameraUpsertRequest {
  name: string;
  brand: SupportedCameraBrand;
  model: string;
  username: string;
  password: string;
  ipAddress: string;
  rtspPort: number;
  ptzPort: number;
  protocol: CameraProtocol;
  siteId: string;
}

export interface CreateCameraResponse {
  id: string;
}
