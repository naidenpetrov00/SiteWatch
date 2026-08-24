import * as MediaLibrary from "expo-media-library";

import {z} from "zod";

export const ptzDirectionSchema = z.enum(["Up", "Down", "Left", "Right"]);

export type PtzDirection = z.infer<typeof ptzDirectionSchema>;

export const blobToDataUrl = (blob: Blob) =>
    new Promise<string>((resolve, reject) => {
        const reader = new FileReader();
        reader.onerror = () => reject(new Error("FileReader failed"));
        reader.onloadend = () => resolve(reader.result as string);
        reader.readAsDataURL(blob);
    });

export async function saveSnapshot(snapshotUri: string) {
    const asset = await MediaLibrary.createAssetAsync(snapshotUri);

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
