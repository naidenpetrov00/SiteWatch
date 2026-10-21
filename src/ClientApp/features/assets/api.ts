import { isAxiosError } from "axios";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { paths } from "@/config/constants/paths";
import { api } from "@/lib/api-client";
import { useAuth } from "@/store/auth_context";
import type {
  MobileAssetDetails,
  MobileAssetListItem,
  MobileAssetRegistration,
  MobileAssetSite,
  PagedResponse,
  RegisterMobileAssetRequest,
} from "./types";

const authorizationHeaders = (accessToken: string) => ({
  Authorization: `Bearer ${accessToken}`,
});

const requireToken = (accessToken: string | null) => {
  if (!accessToken) throw new Error("Authentication is required for asset tracking.");
  return accessToken;
};

export const mobileAssetKeys = {
  all: ["mobile-assets"] as const,
  details: (id: string) => ["mobile-assets", "details", id] as const,
  registration: (id: string) => ["mobile-assets", "registration", id] as const,
  search: (search: string, pageIndex: number) =>
    ["mobile-assets", "search", search, pageIndex] as const,
  sites: (search: string) => ["mobile-assets", "sites", search] as const,
};

const discoverAssetByQr = async (qrIdentifier: string, accessToken: string) => {
  const response = await api.post(
    paths.mobileAssets.byQr,
    { qrIdentifier },
    { headers: authorizationHeaders(accessToken) },
  );
  return response as unknown as MobileAssetDetails;
};

const searchAssets = async (
  search: string,
  pageIndex: number,
  accessToken: string,
  signal: AbortSignal,
) => {
  const response = await api.get(paths.mobileAssets.search, {
    headers: authorizationHeaders(accessToken),
    params: { search, pageIndex, pageSize: 20 },
    signal,
  });
  return response as unknown as PagedResponse<MobileAssetListItem>;
};

const getAsset = async (id: string, accessToken: string) => {
  const response = await api.get(paths.mobileAssets.byId(id), {
    headers: authorizationHeaders(accessToken),
  });
  return response as unknown as MobileAssetDetails;
};

const getSites = async (
  search: string,
  accessToken: string,
  signal: AbortSignal,
) => {
  const response = await api.get(paths.mobileAssets.sites, {
    headers: authorizationHeaders(accessToken),
    params: search.trim() ? { search: search.trim() } : undefined,
    signal,
  });
  return response as unknown as MobileAssetSite[];
};

const registerAsset = async (
  request: RegisterMobileAssetRequest,
  accessToken: string,
) => {
  const response = await api.post(paths.mobileAssets.register, request, {
    headers: authorizationHeaders(accessToken),
  });
  return response as unknown as MobileAssetRegistration;
};

const getRegistration = async (id: string, accessToken: string) => {
  const response = await api.get(paths.mobileAssets.registration(id), {
    headers: authorizationHeaders(accessToken),
  });
  return response as unknown as MobileAssetRegistration;
};

export const useDiscoverAssetByQr = () => {
  const { accessToken } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: ["mobile-assets", "discover-qr"],
    mutationFn: (qrIdentifier: string) =>
      discoverAssetByQr(qrIdentifier, requireToken(accessToken)),
    onSuccess: (asset) => {
      queryClient.setQueryData(mobileAssetKeys.details(asset.id), asset);
    },
  });
};

export const useSearchMobileAssets = (search: string, pageIndex: number) => {
  const { accessToken } = useAuth();

  return useQuery({
    queryKey: mobileAssetKeys.search(search, pageIndex),
    queryFn: ({ signal }) =>
      searchAssets(search, pageIndex, requireToken(accessToken), signal),
    enabled: Boolean(accessToken && search.trim()),
  });
};

export const useMobileAsset = (id: string) => {
  const { accessToken } = useAuth();

  return useQuery({
    queryKey: mobileAssetKeys.details(id),
    queryFn: () => getAsset(id, requireToken(accessToken)),
    enabled: Boolean(accessToken && id),
  });
};

export const useMobileAssetSites = (search: string, enabled = true) => {
  const { accessToken } = useAuth();

  return useQuery({
    queryKey: mobileAssetKeys.sites(search),
    queryFn: ({ signal }) => getSites(search, requireToken(accessToken), signal),
    enabled: Boolean(accessToken && enabled),
  });
};

export const useRegisterMobileAsset = () => {
  const { accessToken } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: ["mobile-assets", "register"],
    mutationFn: (request: RegisterMobileAssetRequest) =>
      registerAsset(request, requireToken(accessToken)),
    onSuccess: (registration) => {
      queryClient.setQueryData(
        mobileAssetKeys.registration(registration.id),
        registration,
      );
    },
  });
};

export const useMobileAssetRegistration = (id: string) => {
  const { accessToken } = useAuth();

  return useQuery({
    queryKey: mobileAssetKeys.registration(id),
    queryFn: () => getRegistration(id, requireToken(accessToken)),
    enabled: Boolean(accessToken && id),
  });
};

export const getAssetApiStatus = (error: unknown) =>
  isAxiosError(error) ? error.response?.status : undefined;

export const getAssetApiError = (
  error: unknown,
  fallback = "The request could not be completed. Try again.",
) => {
  if (!isAxiosError(error)) {
    return error instanceof Error && error.message.trim() ? error.message : fallback;
  }

  if (!error.response) {
    return "The API could not be reached. Check your connection and try again.";
  }

  switch (error.response.status) {
    case 400: {
      const details = error.response.data?.details;
      if (Array.isArray(details) && details[0]?.message) return details[0].message as string;
      return "Review the supplied values and try again.";
    }
    case 401:
      return "Your session has expired. Sign in and try again.";
    case 403:
      return "Administrator or Worker access is required for asset tracking.";
    case 404:
      return "This resource is not available to you.";
    case 409:
      return error.response.data?.detail ?? "The registration conflicts with the current asset state.";
    default:
      return fallback;
  }
};
