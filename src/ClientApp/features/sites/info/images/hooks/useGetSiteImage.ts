import { queryConfig, type QueryConfig } from "@/lib/react-query";

import { api } from "@/lib/api-client";
import { paths } from "@/config/constants/paths";
import { useQuery } from "@tanstack/react-query";
import { z } from "zod";

export const getSiteImageContextSchema = z.object({
  imageId: z.string().uuid("Invalid GUID format"),
});

export type GetSiteImageContext = z.infer<typeof getSiteImageContextSchema>;

const blobToDataUrl = (blob: Blob) =>
  new Promise<string>((resolve, reject) => {
    const reader = new FileReader();
    reader.onerror = () => reject(new Error("FileReader failed"));
    reader.onloadend = () => resolve(reader.result as string);
    reader.readAsDataURL(blob);
  });

export const getSiteImage = async ({
  imageId,
}: GetSiteImageContext): Promise<string> => {
  const blob = await api.get<Blob, Blob>(paths.images.getById(imageId), {
    responseType: "blob",
  });

  return blobToDataUrl(blob);
};

type UseGetSiteImageOptions = {
  imageId?: string;
  queryKey: string;
  queryConfig?: QueryConfig<typeof getSiteImage>;
};

export const useGetSiteImage = ({
  imageId,
  queryKey,
  queryConfig: customQueryConfig,
}: UseGetSiteImageOptions) => {
  return useQuery({
    queryKey: [queryKey, imageId],
    enabled: Boolean(imageId),
    queryFn: () => getSiteImage({ imageId: imageId! }),
    ...queryConfig,
    ...customQueryConfig,
  });
};

export default useGetSiteImage;
