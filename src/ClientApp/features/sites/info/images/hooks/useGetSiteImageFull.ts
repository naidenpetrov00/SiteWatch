import type { QueryConfig } from "@/lib/react-query";

import { getSiteImage, useGetSiteImage } from "./useGetSiteImage";

type UseGetSiteImageFullOptions = {
  imageId?: string;
  queryConfig?: QueryConfig<typeof getSiteImage>;
};

export const useGetSiteImageFull = ({
  imageId,
  queryConfig: customQueryConfig,
}: UseGetSiteImageFullOptions) => {
  return useGetSiteImage({
    imageId,
    queryKey: "site-image-full",
    queryConfig: customQueryConfig,
  });
};

export default useGetSiteImageFull;
