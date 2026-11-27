import { PtzDirection } from '../../models/camera-stream.models';

export type PtzSafetyState = 'clear' | 'active' | 'stopping' | 'stopFailed';

export type PtzControlState =
  | { kind: 'idle' }
  | { kind: 'arming'; direction: PtzDirection }
  | { kind: 'relative'; label: string }
  | { kind: 'starting'; direction: PtzDirection }
  | { kind: 'moving'; direction: PtzDirection }
  | { kind: 'stopping'; direction: PtzDirection }
  | { kind: 'stopFailed'; direction: PtzDirection };
