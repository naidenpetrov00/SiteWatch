export interface DashboardCamera {
  id: string;
  numberId: number;
  name: string;
  brand: string;
  model: string;
  ipAddress: string | null;
  rtspPort: number | null;
  ptzPort: number | null;
  siteId: string | null;
  siteName: string | null;
  junk?: null;
}
