export const paths = {
    identity: {
        signUp: "/identity/signUp",
        signIn: "/identity/signIn",
        sendVerification: "/identity/sendVerification",
        verifyEmail: "/identity/verifyEmail",
        resendEmail: "/identity/sendVerification",
    },
    sites: {
        getByUserId: (userId: string) => `/site/sitesByUser/${userId}`,
    },
    cameras: {
        getBySiteId: (siteId: string) => `/camera/camerasBySite/${siteId}`,
        getById: (cameraId: string) => `/camera/cameraById/${cameraId}`,
    },
};
