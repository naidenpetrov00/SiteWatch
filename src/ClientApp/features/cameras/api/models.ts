import { Brand } from "../types";

export interface Camera extends CameraAuth {
  id: string;
  name: string;
  brand: Brand;
  ipAddress: string;
  port: string;
}

export interface CameraAuth {
  username: string;
  password: string;
}
