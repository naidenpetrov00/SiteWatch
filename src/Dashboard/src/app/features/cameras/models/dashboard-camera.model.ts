export interface DashboardCamera {
  id: string;
  numberId: number;
  name: string;
  brand: string;
  model: string;
  ipAddress: string | null;
  rtspPort: number | null;
  ptzPort: number | null;
  protocol: string;
  siteId: string | null;
  siteName: string | null;
  deleteAction?: null;
}
