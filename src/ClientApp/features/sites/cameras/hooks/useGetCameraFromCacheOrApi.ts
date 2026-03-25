import { Camera } from "@/features/cameras/api/models";
import { useCameraById } from "@/features/cameras/api/get-camera-by-id";
import { useQueryClient } from "@tanstack/react-query";

const useGetCameraFromCacheOrApi = (
  siteId: string,
  cameraId: string
): Camera => {
  const queryClient = useQueryClient();
  const cameras = queryClient.getQueryData<Camera[]>(["cameras", siteId]);

  const cameraFromCache = cameras?.find((c) => c.id === cameraId);
  if (!cameraFromCache) {
    const { data } = useCameraById({ cameraId });
    return data!;
  }
  return cameraFromCache!;
};

export default useGetCameraFromCacheOrApi;
