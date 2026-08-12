import { useMutation, useMutationState, useQueryClient } from "@tanstack/react-query";

import { env } from "@/config/env";
import { paths } from "@/config/constants/paths";
import { useAuth } from "@/store/auth_context";
import { MAX_UPLOAD_BYTES } from "./constants";
import type { SiteFileIds } from "../files/types";
import type { SiteImageIds } from "../images/types";
import type { SiteVideoIds } from "../videos/types";
import type {
  PendingSiteMediaUpload,
  SiteMediaUploadKind,
  UploadSiteMediaRequest,
} from "./types";

type UploadSiteMediaResponse =
  | { kind: "image"; originalFileId: string; thumbnailFileId: string }
  | { kind: "video"; videoFileId: string; snapshotFileId: string; durationSeconds: number | null }
  | { kind: "file"; fileId: string };

const getUploadPath = (kind: SiteMediaUploadKind, siteId: string) => {
  switch (kind) {
    case "image":
      return paths.images.create(siteId);
    case "video":
      return paths.videos.create(siteId);
    case "file":
      return paths.files.create(siteId);
  }
};

const getUploadErrorMessage = async (
  response: Response,
  kind: SiteMediaUploadKind,
): Promise<string> => {
  if (response.status === 401) {
    return "Your session has expired. Sign in again and retry.";
  }

  if (response.status === 403) {
    return "Your account is not allowed to upload site content.";
  }

  if (response.status === 413) {
    const limit = Math.round(MAX_UPLOAD_BYTES[kind] / 1024 / 1024);
    return `The ${kind} file cannot exceed ${limit} MB.`;
  }

  const payload = (await response.json().catch(() => null)) as {
    detail?: string;
    errorMessage?: string;
    details?: { message?: string }[];
    errors?: Record<string, string[]>;
  } | null;
  const validationMessage = payload?.errors
    ? Object.values(payload.errors)
        .flat()
        .find((message) => message.trim().length > 0)
    : payload?.details?.find((detail) => detail.message?.trim().length)
        ?.message;

  return (
    validationMessage ??
    payload?.detail ??
    payload?.errorMessage ??
    `Unable to upload the ${kind}.`
  );
};

const queryKeyForKind = (kind: SiteMediaUploadKind) => {
  switch (kind) {
    case "image":
      return "site-image-ids";
    case "video":
      return "site-video-ids";
    case "file":
      return "site-file-ids";
  }
};

export const usePendingSiteMediaUploads = (
  kind: SiteMediaUploadKind,
  siteId?: string,
) => {
  const mutations = useMutationState({
    filters: { mutationKey: ["site-media", "upload"], status: "pending" },
    select: (mutation): PendingSiteMediaUpload => ({
      mutationId: mutation.mutationId,
      request: mutation.state.variables as UploadSiteMediaRequest,
    }),
  });

  return mutations.filter(
    ({ request }) => request.kind === kind && request.siteId === siteId,
  );
};

export const useUploadSiteMedia = () => {
  const { accessToken } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: ["site-media", "upload"],
    mutationFn: async ({ siteId, kind, asset, category, documentType }: UploadSiteMediaRequest): Promise<UploadSiteMediaResponse> => {
      if (!accessToken) {
        throw new Error("Authentication is required to upload site content.");
      }

      const formData = new FormData();
      formData.append(
        "file",
        {
          uri: asset.uri,
          name: asset.fileName,
          type: asset.contentType,
        } as unknown as Blob,
      );

      if (kind === "file") {
        formData.append("documentType", documentType ?? "");
      } else {
        formData.append("category", category ?? "");
      }

      let response: Response;
      try {
        response = await fetch(
          new URL(getUploadPath(kind, siteId), env.API_URL).toString(),
          {
            method: "POST",
            headers: { Authorization: `Bearer ${accessToken}` },
            body: formData,
          },
        );
      } catch {
        throw new Error("Unable to reach the server. Check your connection and retry.");
      }

      if (!response.ok) {
        throw new Error(await getUploadErrorMessage(response, kind));
      }

      const result = await response.json() as Record<string, unknown>;
      switch (kind) {
        case "image":
          return {
            kind,
            originalFileId: result.originalFileId as string,
            thumbnailFileId: result.thumbnailFileId as string,
          };
        case "video":
          return {
            kind,
            videoFileId: result.videoFileId as string,
            snapshotFileId: result.snapshotFileId as string,
            durationSeconds: result.durationSeconds as number | null,
          };
        case "file":
          return { kind, fileId: result.fileId as string };
      }
    },
    onSuccess: (response, request) => {
      if (response.kind === "image" && request.category) {
        queryClient.setQueryData<SiteImageIds[]>(
          ["site-image-ids", request.siteId],
          (items = []) =>
            items.some((item) => item.imageId === response.originalFileId)
              ? items
              : [
                  {
                    imageId: response.originalFileId,
                    thumbnailId: response.thumbnailFileId,
                    category: request.category,
                  },
                  ...items,
                ],
        );
      }

      if (response.kind === "video" && request.category) {
        queryClient.setQueryData<SiteVideoIds[]>(
          ["site-video-ids", request.siteId],
          (items = []) =>
            items.some((item) => item.videoId === response.videoFileId)
              ? items
              : [
                  {
                    videoId: response.videoFileId,
                    snapshotId: response.snapshotFileId,
                    durationSeconds: response.durationSeconds,
                    category: request.category,
                  },
                  ...items,
                ],
        );
      }

      if (response.kind === "file" && request.documentType) {
        queryClient.setQueryData<SiteFileIds[]>(
          ["site-file-ids", request.siteId],
          (items = []) =>
            items.some((item) => item.fileId === response.fileId)
              ? items
              : [
                  {
                    fileId: response.fileId,
                    fileName: request.asset.fileName,
                    contentType: request.asset.contentType,
                    documentType: request.documentType,
                  },
                  ...items,
                ],
        );
      }

      void queryClient.invalidateQueries({
        queryKey: [queryKeyForKind(request.kind), request.siteId],
      });
    },
  });
};
