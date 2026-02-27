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
                "Allow access to Photos to organize snapshots into albums",
            NSPhotoLibraryAddUsageDescription: "Allow saving camera snapshots",
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
      permissions: [
        "READ_MEDIA_IMAGES",
        "READ_EXTERNAL_STORAGE",
        "WRITE_EXTERNAL_STORAGE",
      ],
    },
    plugins: [
      [
        "expo-screen-orientation",
        {
          initialOrientation: "PORTRAIT_UP",
        },
      ],
    ],
  },
};
