export const paths = {
    identity: {
        signUp: "/identity/signUp",
        signIn: "/identity/signIn",
        sendVerification: "/identity/sendVerification",
        verifyEmail: "/identity/verifyEmail",
        resendEmail: "/identity/sendVerification",
    },
    sites: {
        getByUserId: (userId: string) => `/sites/sitesByUser/${userId}`,
    },
    cameras: {
        getBySiteId: (siteId: string) => `/cameras/site/${siteId}/cameras`,
        getById: (cameraId: string) => `/cameras/${cameraId}`,
        getSnapshot: (cameraId: string) => `/cameras/${cameraId}/snapshot`,
        startPtzMovement: (cameraId: string) => `/cameras/${cameraId}/ptz/start`,
        stopPtzMovement: (cameraId: string) => `/cameras/${cameraId}/ptz/stop`,
        movePtzRelatively: (cameraId: string) => `/cameras/${cameraId}/ptz/relative`,
        createWithDetails: "/cameras/withDetails",
        create: "/cameras",
        delete: (cameraId: string) => `/cameras/${cameraId}`,
        moveToSite: (cameraId: string) => `/cameras/${cameraId}/site`,
    },
    images: {
        create: (siteId: string) => `/images/${siteId}`,
        getIdsBySiteId: (siteId: string) => `/images/images${siteId}`,
        getById: (imageId: string) => `/images/${imageId}`,
    },
    videos: {
        create: (siteId: string) => `/videos/${siteId}`,
        getIdsBySiteId: (siteId: string) => `/videos/site/${siteId}`,
        getById: (videoId: string) => `/videos/${videoId}`,
        getSnapshotById: (snapshotId: string) => `/videos/snapshot/${snapshotId}`,
    },
    files: {
        create: (siteId: string) => `/files/${siteId}`,
        getIdsBySiteId: (siteId: string) => `/files/files${siteId}`,
        getById: (fileId: string) => `/files/${fileId}`,
    },
    invoices: {
        getBySiteId: (siteId: string) => `/invoices/site/${siteId}`,
        createFromFile: (siteId: string) => `/invoices/site/${siteId}/file`,
        getFileAccess: (siteId: string, invoiceId: string) =>
            `/invoices/site/${siteId}/${invoiceId}/file-access`,
    },
};
