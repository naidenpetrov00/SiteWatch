import { CameraView, type BarcodeScanningResult, useCameraPermissions } from "expo-camera";
import { useIsFocused } from "@react-navigation/native";
import { useRouter } from "expo-router";
import { useCallback, useEffect, useRef, useState } from "react";
import {
  ActivityIndicator,
  Linking,
  Pressable,
  Text,
  View,
} from "react-native";

import { getAssetApiError, useDiscoverAssetByQr } from "../api";
import assetStyles from "../assets.styles";
import { isCanonicalQrIdentifier } from "../validation";
import { useColorPalette } from "@/hooks/useColorPalette";

export default function AssetScannerScreen() {
  const colors = useColorPalette();
  const router = useRouter();
  const isFocused = useIsFocused();
  const [permission, requestPermission] = useCameraPermissions();
  const discovery = useDiscoverAssetByQr();
  const [cameraAvailable, setCameraAvailable] = useState<boolean | null>(null);
  const [cameraActive, setCameraActive] = useState(false);
  const [permissionRequestPending, setPermissionRequestPending] = useState(false);
  const [scanLocked, setScanLocked] = useState(false);
  const [message, setMessage] = useState<string | null>(null);
  const mountedRef = useRef(true);
  const focusedRef = useRef(isFocused);
  const scanLockRef = useRef(false);

  focusedRef.current = isFocused;

  useEffect(() => {
    mountedRef.current = true;
    void CameraView.isAvailableAsync()
      .then((available) => {
        if (mountedRef.current) {
          setCameraAvailable(available);
          if (!available) {
            setMessage("A camera is not available on this device. Search for the asset manually.");
          }
        }
      })
      .catch(() => {
        if (mountedRef.current) {
          setCameraAvailable(false);
          setMessage("Camera availability could not be determined. Search for the asset manually.");
        }
      });

    return () => {
      mountedRef.current = false;
    };
  }, []);

  const unlock = useCallback(() => {
    scanLockRef.current = false;
    setScanLocked(false);
    setMessage(null);
    discovery.reset();
  }, [discovery]);

  const startCamera = useCallback(async () => {
    if (permissionRequestPending) return;
    setMessage(null);
    if (cameraAvailable === false) {
      setMessage("A camera is not available on this device. Search for the asset manually.");
      return;
    }

    if (permission?.granted) {
      unlock();
      setCameraActive(true);
      return;
    }

    if (permission && !permission.canAskAgain) {
      setMessage("Camera access is disabled. Enable it in Settings or search manually.");
      return;
    }

    setPermissionRequestPending(true);
    try {
      const nextPermission = await requestPermission();
      if (!mountedRef.current) return;
      if (nextPermission.granted) {
        unlock();
        setCameraActive(true);
        return;
      }

      setMessage(
        nextPermission.canAskAgain
          ? "Camera permission was denied. You can retry or search manually."
          : "Camera access is disabled. Enable it in Settings or search manually.",
      );
    } catch {
      if (mountedRef.current) {
        setMessage("Camera permission could not be requested. Try again or search manually.");
      }
    } finally {
      if (mountedRef.current) setPermissionRequestPending(false);
    }
  }, [cameraAvailable, permission, permissionRequestPending, requestPermission, unlock]);

  const handleBarcodeScanned = useCallback(
    async (result: BarcodeScanningResult) => {
      if (scanLockRef.current || result.type !== "qr") return;
      scanLockRef.current = true;
      setScanLocked(true);

      if (!isCanonicalQrIdentifier(result.data)) {
        setMessage("This QR code is not a valid SiteWatch asset identifier.");
        return;
      }

      setMessage("Looking up this asset…");
      try {
        const asset = await discovery.mutateAsync(result.data);
        if (mountedRef.current && focusedRef.current) {
          router.replace({ pathname: "/Assets/[id]", params: { id: asset.id } });
        }
      } catch (error) {
        if (mountedRef.current) {
          setMessage(getAssetApiError(error, "This asset is not available to you."));
        }
      }
    },
    [discovery, router],
  );

  const permanentlyDenied = Boolean(permission && !permission.granted && !permission.canAskAgain);
  const showCamera = cameraActive && permission?.granted && cameraAvailable !== false && isFocused;

  if (cameraAvailable === null) {
    return (
      <View style={[assetStyles.centerState, { backgroundColor: colors.background }]}>
        <ActivityIndicator color={colors.primary} size="large" />
        <Text style={[assetStyles.centerText, { color: colors.secondary }]}>Checking camera availability…</Text>
      </View>
    );
  }

  if (showCamera) {
    return (
      <View style={[assetStyles.screen, { backgroundColor: colors.background }]}>
        <CameraView
          barcodeScannerSettings={{ barcodeTypes: ["qr"] }}
          facing="back"
          onBarcodeScanned={scanLocked ? undefined : handleBarcodeScanned}
          onMountError={() => {
            setCameraActive(false);
            setMessage("The camera could not be started. Try again or search manually.");
          }}
          style={assetStyles.camera}
        >
          <View style={assetStyles.scannerFrame} />
        </CameraView>
        <View style={[assetStyles.cameraOverlay, { backgroundColor: `${colors.background}E6` }]}>
          <Text style={[assetStyles.centerText, { color: colors.text }]}>
            {message ?? "Place the asset QR code inside the frame."}
          </Text>
          {discovery.isPending ? <ActivityIndicator color={colors.primary} /> : null}
          <View style={assetStyles.buttonRow}>
            {scanLocked && !discovery.isPending ? (
              <Pressable
                accessibilityRole="button"
                onPress={unlock}
                style={({ pressed }) => [
                  assetStyles.button,
                  { backgroundColor: colors.primary, opacity: pressed ? 0.78 : 1 },
                ]}
              >
                <Text style={[assetStyles.buttonText, { color: colors.background }]}>Scan again</Text>
              </Pressable>
            ) : null}
            <Pressable
              accessibilityRole="button"
              onPress={() => router.push("/Assets/search")}
              style={({ pressed }) => [
                assetStyles.button,
                { backgroundColor: colors.secondary, opacity: pressed ? 0.78 : 1 },
              ]}
            >
              <Text style={[assetStyles.buttonText, { color: colors.background }]}>Search manually</Text>
            </Pressable>
            <Pressable
              accessibilityRole="button"
              onPress={() => router.back()}
              style={({ pressed }) => [
                assetStyles.button,
                { borderColor: colors.secondary, borderWidth: 1, opacity: pressed ? 0.78 : 1 },
              ]}
            >
              <Text style={[assetStyles.buttonText, { color: colors.text }]}>Cancel</Text>
            </Pressable>
          </View>
        </View>
      </View>
    );
  }

  return (
    <View style={[assetStyles.centerState, { backgroundColor: colors.background }]}>
      <Text style={[assetStyles.title, { color: colors.text }]}>Scan an asset</Text>
      <Text style={[assetStyles.centerText, { color: colors.secondary }]}>
        Camera access is requested only when you start scanning. You can search manually at any time.
      </Text>
      {message ? (
        <Text accessibilityRole="alert" style={[assetStyles.centerText, { color: colors.text }]}>{message}</Text>
      ) : null}
      <View style={assetStyles.buttonRow}>
        {cameraAvailable && !permanentlyDenied ? (
          <Pressable
            accessibilityRole="button"
            accessibilityState={{ busy: permissionRequestPending }}
            disabled={permissionRequestPending}
            onPress={() => void startCamera()}
            style={({ pressed }) => [
              assetStyles.button,
              {
                backgroundColor: colors.primary,
                opacity: permissionRequestPending ? 0.5 : pressed ? 0.78 : 1,
              },
            ]}
          >
            <Text style={[assetStyles.buttonText, { color: colors.background }]}>
              {permissionRequestPending
                ? "Requesting permission…"
                : permission?.granted
                  ? "Start scanning"
                  : "Allow camera and scan"}
            </Text>
          </Pressable>
        ) : null}
        {permanentlyDenied ? (
          <Pressable
            accessibilityRole="button"
            onPress={() => void Linking.openSettings()}
            style={({ pressed }) => [
              assetStyles.button,
              { borderColor: colors.primary, borderWidth: 1, opacity: pressed ? 0.78 : 1 },
            ]}
          >
            <Text style={[assetStyles.buttonText, { color: colors.primary }]}>Open Settings</Text>
          </Pressable>
        ) : null}
        <Pressable
          accessibilityRole="button"
          onPress={() => router.push("/Assets/search")}
          style={({ pressed }) => [
            assetStyles.button,
            { backgroundColor: colors.secondary, opacity: pressed ? 0.78 : 1 },
          ]}
        >
          <Text style={[assetStyles.buttonText, { color: colors.background }]}>Search manually</Text>
        </Pressable>
      </View>
    </View>
  );
}
