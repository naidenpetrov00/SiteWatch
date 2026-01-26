import * as FileSystem from "expo-file-system/legacy";
import * as MediaLibrary from "expo-media-library";

import { z } from "zod";

export const ptzDirectionSchema = z.enum(["Up", "Down", "Left", "Right"]);

export type PtzDirection = z.infer<typeof ptzDirectionSchema>;

export const buildPtzBaseUrl = (ipAddress: string, query: string) =>
  `http://${ipAddress}/cgi-bin/ptz.cgi?${query}`;

export const buildSnapshotBaseUrl = (ipAddress: string, query: string) =>
  `http://${ipAddress}/cgi-bin/snapshot.cgi?${query}`;

export const blobToDataUrl = (blob: Blob) =>
  new Promise<string>((resolve, reject) => {
    const reader = new FileReader();
    reader.onerror = () => reject(new Error("FileReader failed"));
    reader.onloadend = () => resolve(reader.result as string);
    reader.readAsDataURL(blob);
  });

export async function saveSnapshot(dataUrl: string, cameraId: string | number) {
  const match = dataUrl.match(/^data:([^;]+);base64,(.*)$/);
  if (!match) throw new Error("Invalid data URL");

  const base64 = match[2];

  const extension = base64.startsWith("/9j") ? "jpg" : "png";

  const fileUri = `${
    FileSystem.documentDirectory
  }snapshot-${cameraId}-${Date.now()}.${extension}`;

  await FileSystem.writeAsStringAsync(fileUri, base64, {
    encoding: "base64",
  });

  const asset = await MediaLibrary.createAssetAsync(fileUri);

  try {
    const albumName = "Sites";
    const album = await MediaLibrary.getAlbumAsync(albumName);

    if (album) {
      await MediaLibrary.addAssetsToAlbumAsync([asset], album, false);
    } else {
      await MediaLibrary.createAlbumAsync(albumName, asset, false);
    }
  } catch (e) {
    console.warn("Saved, but couldn't add to album:", e);
  }
}
