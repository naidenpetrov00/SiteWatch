export interface DashboardSiteCamera {
  id: string;
  numberId: number;
  name: string;
  brand: string;
  model: string;
}

export interface CameraStreamAccessResponse {
  whepUrl: string;
  bearerTicket: string;
  connectionDeadline: string;
}

export type WhepViewerState =
  | { kind: 'preparing' }
  | { kind: 'connecting' }
  | { kind: 'live' }
  | { kind: 'failed'; message: string }
  | { kind: 'retrying' }
  | { kind: 'closed' };
