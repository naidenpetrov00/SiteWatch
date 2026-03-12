import {blobToDataUrl, buildSnapshotBaseUrl} from "../utils";

import DigestClient from "digest-fetch";
import {Platform} from "react-native";
import {QueryConfig} from "@/lib/react-query";
import {useQuery} from "@tanstack/react-query";
import {z} from "zod";

export const getCameraSnapshotSchema = z.object({
    ipAddress: z.string().min(1),
    port: z.number(),
    username: z.string().min(2),
    password: z.string().min(2),
    channel: z.number().int().optional(),
    type: z.number().int().optional(),
});

export type GetCameraSnapshotInput = z.infer<typeof getCameraSnapshotSchema>;

const SNAPSHOT_TIMEOUT_MS = 10_000;

export const getCameraSnapshot = async ({
                                            ipAddress,
                                            port,
                                            username,
                                            password,
                                            channel = 1,
                                            type = 0,
                                        }: GetCameraSnapshotInput): Promise<string> => {
    const client = new DigestClient(username, password);
    const query = new URLSearchParams({
        channel: String(channel),
        type: String(type),
    }).toString();
    const url = buildSnapshotBaseUrl(ipAddress, port, query);

    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), SNAPSHOT_TIMEOUT_MS);

    let response: Response;
    try {
        response = await client.fetch(url, {signal: controller.signal});
    } catch (error) {
        const errorMessage =
            error instanceof Error ? error.message : "Unknown network error";
        const isTimeout =
            error instanceof Error &&
            (error.name === "AbortError" || errorMessage.includes("aborted"));

        if (isTimeout) {
            throw new Error(
                `Snapshot request timed out after ${SNAPSHOT_TIMEOUT_MS}ms (${url}). On iOS this usually means local network access is blocked or the camera is unreachable.`,
            );
        }

        throw new Error(
            `Snapshot request failed on ${Platform.OS} (${url}): ${errorMessage}`,
        );
    } finally {
        clearTimeout(timeoutId);
    }

    if (!response.ok) {
        const responseText = await response.text().catch(() => "");
        const suffix = responseText ? `: ${responseText}` : "";
        throw new Error(
            `Snapshot request failed (${response.status} ${response.statusText})${suffix}`,
        );
    }

    const blob = await response.blob();
    return await blobToDataUrl(blob);
};

type UseGetCameraSnapshotOptions = {
    data: GetCameraSnapshotInput;
    queryConfig?: QueryConfig<typeof getCameraSnapshot>;
};

export const useGetCameraSnapshot = ({
                                         data,
                                         queryConfig,
                                     }: UseGetCameraSnapshotOptions) => {
    return useQuery({
        queryKey: ["camera-snapshot", data.ipAddress],
        queryFn: () => getCameraSnapshot(data),
        retry: false,
        refetchOnWindowFocus: false,
        staleTime: 5_000,
        gcTime: 20_000,
        ...queryConfig,
    });
};
