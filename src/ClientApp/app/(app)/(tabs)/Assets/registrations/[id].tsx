import { useLocalSearchParams } from "expo-router";

import AssetRegistrationStatusScreen from "@/features/assets/screens/AssetRegistrationStatusScreen";

export default function AssetRegistrationStatusRoute() {
  const { id } = useLocalSearchParams<{ id: string }>();
  return <AssetRegistrationStatusScreen assetId={typeof id === "string" ? id : ""} />;
}
