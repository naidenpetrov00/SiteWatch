import * as DocumentPicker from "expo-document-picker";
import * as ImagePicker from "expo-image-picker";

export type UploadSource = "camera" | "gallery" | "file";
export type PickerMediaKind = "image" | "video" | "media";

export type PickedUploadAsset = {
  uri: string;
  fileName?: string | null;
  mimeType?: string | null;
  fileSize?: number | null;
  mediaType?: "image" | "video";
};

export type PickUploadAssetsOptions = {
  source: UploadSource;
  mediaKind?: PickerMediaKind;
  documentPickerTypes: string | string[];
  multiple?: boolean;
  imageQuality?: number;
};

export type PickUploadAssetsResult =
  | { status: "cancelled" }
  | { status: "permission-denied" }
  | { status: "selected"; assets: PickedUploadAsset[] };

const mediaTypesFor = (kind: PickerMediaKind) =>
  kind === "image" ? ["images"] : kind === "video" ? ["videos"] : ["images", "videos"];

const fromImagePickerAsset = (asset: ImagePicker.ImagePickerAsset): PickedUploadAsset => ({
  uri: asset.uri,
  fileName: asset.fileName,
  mimeType: asset.mimeType,
  fileSize: asset.fileSize,
  mediaType: asset.type === "video" ? "video" : "image",
});

export const selectUploadAssets = async ({
  source,
  mediaKind = "image",
  documentPickerTypes,
  multiple = false,
  imageQuality,
}: PickUploadAssetsOptions): Promise<PickUploadAssetsResult> => {
  if (source === "file") {
    const result = await DocumentPicker.getDocumentAsync({
      type: documentPickerTypes,
      multiple,
      copyToCacheDirectory: true,
    });
    if (result.canceled) return { status: "cancelled" };
    return {
      status: "selected",
      assets: result.assets.map((asset) => ({
        uri: asset.uri,
        fileName: asset.name,
        mimeType: asset.mimeType,
        fileSize: asset.size,
      })),
    };
  }

  if (source === "camera") {
    const permission = await ImagePicker.requestCameraPermissionsAsync();
    if (!permission.granted) return { status: "permission-denied" };

    const result = await ImagePicker.launchCameraAsync({
      mediaTypes: mediaTypesFor(mediaKind),
      allowsEditing: false,
      quality: imageQuality,
    });
    if (result.canceled) return { status: "cancelled" };
    return { status: "selected", assets: [fromImagePickerAsset(result.assets[0])] };
  }

  const result = await ImagePicker.launchImageLibraryAsync({
    mediaTypes: mediaTypesFor(mediaKind),
    allowsEditing: false,
    allowsMultipleSelection: multiple,
    quality: imageQuality,
  });
  if (result.canceled) return { status: "cancelled" };
  return { status: "selected", assets: result.assets.map(fromImagePickerAsset) };
};
