import { Brand } from "../types";

export interface Camera {
  id: string;
  name: string;
  brand: Brand;
  username: string;
  password: string;
  ipAddress: string;
  port: string;
}
