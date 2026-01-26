export enum ChannelType {
  Main = 0,
  Sub = 1,
}

export enum Brand {
  Unknown = "Unknown",
  Dahua = "Dahua",
}

export interface PlayerHandle {
  toggleRecording: () => Promise<void>;
}