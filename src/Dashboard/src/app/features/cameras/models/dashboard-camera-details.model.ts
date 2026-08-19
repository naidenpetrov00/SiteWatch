export interface DashboardCameraDetails {
  id: string;
  numberId: number;
  name: string;
  brand: string;
  model: string;
  username: string | null;
  password: string | null;
  ipAddress: string | null;
  rtspPort: number | null;
  ptzPort: number | null;
  siteId: string | null;
  siteName: string | null;
}
