import Axios, { InternalAxiosRequestConfig } from "axios";

import { env } from "@/config/env";

export const api = Axios.create({
  baseURL: env.API_URL,
});

api.interceptors.response.use(
  (response) => response.data,
  (error) => {

    if (error.response) {
      console.log("❌ API responded with error:");
      console.log("Status:", error.response.status);
      console.log("URL:", error.config?.baseURL + error.config?.url);
      console.log("Data:", error.response.data);
    } else if (error.request) {
      console.log("🚫 No response received (Network Error)");
      console.log("URL:", error.config?.baseURL + error.config?.url);
      console.log("Request:", error.request);
    } else {
      console.log("⚙️ Request setup error:", error.message);
    }

    return Promise.reject(error);
  }
);

