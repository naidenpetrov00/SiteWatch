import { queryConfig, type QueryConfig } from "@/lib/react-query";

import { api } from "@/lib/api-client";
import { paths } from "@/config/constants/paths";
import { useQuery } from "@tanstack/react-query";
import { z } from "zod";

export const getSiteVideoSnapshotContextSchema = z.object({
  snapshotId: z.string().uuid("Invalid GUID format"),
});

export type GetSiteVideoSnapshotContext = z.infer<
  typeof getSiteVideoSnapshotContextSchema
>;

const blobToDataUrl = (blob: Blob) =>
  new Promise<string>((resolve, reject) => {
    const reader = new FileReader();
    reader.onerror = () => reject(new Error("FileReader failed"));
    reader.onloadend = () => resolve(reader.result as string);
    reader.readAsDataURL(blob);
  });

export const getSiteVideoSnapshot = async ({
  snapshotId,
}: GetSiteVideoSnapshotContext): Promise<string> => {
  const blob = await api.get<Blob, Blob>(paths.videos.getSnapshotById(snapshotId), {
    responseType: "blob",
  });

  return blobToDataUrl(blob);
};

type UseGetSiteVideoSnapshotOptions = {
  snapshotId?: string;
  queryKey?: string;
  queryConfig?: QueryConfig<typeof getSiteVideoSnapshot>;
};

export const useGetSiteVideoSnapshot = ({
  snapshotId,
  queryKey,
  queryConfig: customQueryConfig,
}: UseGetSiteVideoSnapshotOptions) => {
  return useQuery({
    queryKey: [queryKey ?? "video-snapshot", snapshotId],
    enabled: Boolean(snapshotId),
    queryFn: () => getSiteVideoSnapshot({ snapshotId: snapshotId! }),
    ...queryConfig,
    ...customQueryConfig,
  });
};

export default useGetSiteVideoSnapshot;
