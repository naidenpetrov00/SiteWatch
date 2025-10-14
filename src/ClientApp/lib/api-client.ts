import Axios, { InternalAxiosRequestConfig } from "axios";

import { env } from "@/config/env";

export const api = Axios.create({
  baseURL: env.API_URL,
});

api.interceptors.response.use(
  (response) => {
    return response.data;
  },
  (error) => {
    const message = error.response?.data?.message || error.message;

    console.log(message);

    if (error.response?.status === 401) {
    }

    return Promise.reject(error);
  }
);
