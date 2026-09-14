import "dotenv/config";

export default {
  expo: {
    name: "MyApp",
    slug: "myapp",
    orientation: "portrait",
    extra: {
      API_URL: process.env.API_URL,
    },
    ios: {
        bundleIdentifier: "com.naidenpetrov00.myapp",
        infoPlist: {
            NSPhotoLibraryUsageDescription:
                "Allow access to Photos to upload site images and videos",
            NSPhotoLibraryAddUsageDescription: "Allow saving camera snapshots",
            NSCameraUsageDescription:
                "Allow access to the camera to add site images and videos",
            NSMicrophoneUsageDescription:
                "Allow SiteWatch to use the microphone when recording issue videos",
            NSAppTransportSecurity: {
                NSAllowsArbitraryLoads: true,
                NSAllowsArbitraryLoadsInWebContent: true,
                NSAllowsLocalNetworking: true,
            },
            NSLocalNetworkUsageDescription:
                "Allow access to cameras and devices on your local network",
      },
    },
    android: {
      package: "com.naidenpetrov00.myapp",
      permissions: ["CAMERA", "RECORD_AUDIO"],
    },
    plugins: [
      [
        "expo-image-picker",
        {
          photosPermission: "Allow SiteWatch to access your photos and videos for issue attachments.",
          cameraPermission: "Allow SiteWatch to use the camera for issue attachments.",
          microphonePermission: "Allow SiteWatch to use the microphone when recording issue videos.",
        },
      ],
      "expo-video",
      [
        "expo-screen-orientation",
        {
          initialOrientation: "PORTRAIT_UP",
        },
      ],
    ],
  },
};
