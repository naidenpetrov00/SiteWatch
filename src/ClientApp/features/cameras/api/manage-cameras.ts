import { useMutation, useQueryClient } from "@tanstack/react-query";

import { paths } from "@/config/constants/paths";
import { useAuth } from "@/store/auth_context";
import { CameraUpsertRequest, CreateCameraResponse } from "./models";
import { cameraApiFetch } from "./camera-api-fetch";

const createCamera = ({
  request,
  accessToken,
}: {
  request: CameraUpsertRequest;
  accessToken: string;
}): Promise<CreateCameraResponse> =>
  cameraApiFetch(paths.cameras.create, accessToken, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
  }).then(async (response) => await response.json() as CreateCameraResponse);

const moveCameraToSite = ({
  cameraId,
  siteId,
  accessToken,
}: {
  cameraId: string;
  siteId: string;
  accessToken: string;
}): Promise<void> =>
  cameraApiFetch(paths.cameras.moveToSite(cameraId), accessToken, {
    method: "PATCH",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ siteId }),
  }).then(() => undefined);

const deleteCamera = ({
  cameraId,
  accessToken,
}: {
  cameraId: string;
  accessToken: string;
}): Promise<void> =>
  cameraApiFetch(paths.cameras.delete(cameraId), accessToken, {
    method: "DELETE",
  }).then(() => undefined);

export const useCreateCamera = (siteId: string) => {
  const { accessToken } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CameraUpsertRequest) => {
      if (!accessToken) throw new Error("Authentication is required to add a camera.");
      return createCamera({ request, accessToken });
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["cameras", siteId] }),
  });
};

export const useMoveCameraToSite = (sourceSiteId: string) => {
  const { accessToken } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ cameraId, siteId }: { cameraId: string; siteId: string }) => {
      if (!accessToken) throw new Error("Authentication is required to move a camera.");
      return moveCameraToSite({ cameraId, siteId, accessToken });
    },
    onSuccess: (_, { cameraId, siteId }) => {
      queryClient.removeQueries({ queryKey: ["camera", cameraId] });
      return Promise.all([
        queryClient.invalidateQueries({ queryKey: ["cameras", sourceSiteId] }),
        queryClient.invalidateQueries({ queryKey: ["cameras", siteId] }),
      ]);
    },
  });
};

export const useDeleteCamera = (siteId: string) => {
  const { accessToken } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (cameraId: string) => {
      if (!accessToken) throw new Error("Authentication is required to delete a camera.");
      return deleteCamera({ cameraId, accessToken });
    },
    onSuccess: (_, cameraId) => {
      queryClient.removeQueries({ queryKey: ["camera", cameraId] });
      return queryClient.invalidateQueries({ queryKey: ["cameras", siteId] });
    },
  });
};
