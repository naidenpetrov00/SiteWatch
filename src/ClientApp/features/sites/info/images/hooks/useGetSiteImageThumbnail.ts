import type { QueryConfig } from "@/lib/react-query";

import { getSiteImage, useGetSiteImage } from "./useGetSiteImage";

type UseGetSiteImageThumbnailOptions = {
  imageId?: string;
  queryConfig?: QueryConfig<typeof getSiteImage>;
};

export const useGetSiteImageThumbnail = ({
  imageId,
  queryConfig: customQueryConfig,
}: UseGetSiteImageThumbnailOptions) => {
  return useGetSiteImage({
    imageId,
    queryKey: "site-image-thumbnail",
    queryConfig: customQueryConfig,
  });
};

export default useGetSiteImageThumbnail;
