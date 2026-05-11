import { queryConfig, type QueryConfig } from "@/lib/react-query";

import { api } from "@/lib/api-client";
import { paths } from "@/config/constants/paths";
import { useQuery } from "@tanstack/react-query";
import { z } from "zod";

export const getSiteImageThumbnailContextSchema = z.object({
  imageId: z.string().uuid("Invalid GUID format"),
});

export type GetSiteImageThumbnailContext = z.infer<
  typeof getSiteImageThumbnailContextSchema
>;

const blobToDataUrl = (blob: Blob) =>
  new Promise<string>((resolve, reject) => {
    const reader = new FileReader();
    reader.onerror = () => reject(new Error("FileReader failed"));
    reader.onloadend = () => resolve(reader.result as string);
    reader.readAsDataURL(blob);
  });

export const getSiteImageThumbnail = async ({
  imageId,
}: GetSiteImageThumbnailContext): Promise<string> => {
  const blob = await api.get<Blob, Blob>(paths.images.getById(imageId), {
    responseType: "blob",
  });

  return blobToDataUrl(blob);
};

type UseGetSiteImageThumbnailOptions = {
  imageId?: string;
  queryConfig?: QueryConfig<typeof getSiteImageThumbnail>;
};

export const useGetSiteImageThumbnail = ({
  imageId,
  queryConfig: customQueryConfig,
}: UseGetSiteImageThumbnailOptions) => {
  return useQuery({
    queryKey: ["site-image-thumbnail", imageId],
    enabled: Boolean(imageId),
    queryFn: () => getSiteImageThumbnail({ imageId: imageId! }),
    ...queryConfig,
    ...customQueryConfig,
  });
};

export default useGetSiteImageThumbnail;
