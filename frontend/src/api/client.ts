import axios, { AxiosHeaders } from "axios";

// Default to backend port 5154; override via VITE_API_BASE_URL if needed
const baseURL =
   import.meta.env.VITE_API_BASE_URL?.toString() ?? "https://gsmss.onrender.com";

let credentialHeaders: { email?: string; password?: string } = {};

export const setCredentialHeaders = (email?: string, password?: string) => {
  credentialHeaders = { email, password };
  console.log("[API Client] Credentials updated:", {
    email,
    hasPassword: !!password,
  });
};

const api = axios.create({
  baseURL,
});

api.interceptors.request.use((config) => {
  const headers = new AxiosHeaders(config.headers);

  // Skip auth headers for public endpoints (login and register)
  const isPublicEndpoint = config.url?.includes("/api/auth/login") || 
                          config.url?.includes("/api/auth/register");

  if (!isPublicEndpoint && credentialHeaders.email && credentialHeaders.password) {
    headers.set("X-User-Email", credentialHeaders.email);
    headers.set("X-User-Password", credentialHeaders.password);
    console.log("[API Client] Setting auth headers for:", config.url, {
      email: credentialHeaders.email,
      hasPassword: !!credentialHeaders.password,
    });
    // #region agent log
    fetch('http://127.0.0.1:7243/ingest/137748db-c585-413d-b907-ee6b0f0d331d',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'client.ts:28',message:'Setting auth headers',data:{url:config.url,email:credentialHeaders.email,hasPassword:!!credentialHeaders.password},timestamp:Date.now(),sessionId:'debug-session',runId:'run1',hypothesisId:'D'})}).catch(()=>{});
    // #endregion
  } else if (isPublicEndpoint) {
    console.log("[API Client] Skipping auth headers for public endpoint:", config.url);
  } else {
    console.warn("[API Client] No credentials available for:", config.url);
    // #region agent log
    fetch('http://127.0.0.1:7243/ingest/137748db-c585-413d-b907-ee6b0f0d331d',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'client.ts:36',message:'No credentials available',data:{url:config.url,hasEmail:!!credentialHeaders.email,hasPassword:!!credentialHeaders.password},timestamp:Date.now(),sessionId:'debug-session',runId:'run1',hypothesisId:'D'})}).catch(()=>{});
    // #endregion
  }

  config.headers = headers;

  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    // If we get a 401, the credentials might be invalid
    // Don't clear automatically - let the app handle it
    // This prevents infinite redirect loops
    if (error.response?.status === 401) {
      console.warn("401 Unauthorized - check if credentials are valid");
    }
    return Promise.reject(error);
  }
);

export default api;

