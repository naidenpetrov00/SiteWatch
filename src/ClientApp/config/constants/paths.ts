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
        createWithDetails: "/cameras/withDetails",
    },
};
