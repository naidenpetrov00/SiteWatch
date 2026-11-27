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

export type CameraStreamProfile = 'sub' | 'main';

export type PtzDirection = 'Up' | 'Down' | 'Left' | 'Right';

export interface PtzRelativeMovement {
  horizontal: number;
  vertical: number;
  zoom: number;
}

export type WhepViewerState =
  | { kind: 'preparing' }
  | { kind: 'connecting' }
  | { kind: 'live' }
  | { kind: 'failed'; message: string }
  | { kind: 'retrying' }
  | { kind: 'closed' };

export type WhepRecordingState =
  | { kind: 'idle' }
  | { kind: 'recording' }
  | { kind: 'finalizing' }
  | { kind: 'unsupported'; message: string };

export type WhepAudioState = 'unavailable' | 'available' | 'interrupted';
