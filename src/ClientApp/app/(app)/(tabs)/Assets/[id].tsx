import { useLocalSearchParams } from "expo-router";

import AssetDetailsScreen from "@/features/assets/screens/AssetDetailsScreen";

export default function AssetDetailsRoute() {
  const { id } = useLocalSearchParams<{ id: string }>();
  return <AssetDetailsScreen assetId={typeof id === "string" ? id : ""} />;
}
