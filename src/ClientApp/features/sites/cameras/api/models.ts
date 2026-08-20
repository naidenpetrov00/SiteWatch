import { Brand } from "../types";

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
