import axios from 'axios';

// 📡 Central axios instance — Kept exactly as you requested to match your existing code paths!
const api = axios.create({
  baseURL: 'https://localhost:7130/api', 
});

/* ==========================================================================
   🚀 1. REQUEST INTERCEPTOR: Token Injection & Smart Content-Type Fallback
   ========================================================================== */
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    if (config.data && !(config.data instanceof FormData)) {
      config.headers['Content-Type'] = 'application/json';
    }

    return config;
  },
  (error) => Promise.reject(error)
);

/* ==========================================================================
   🔄 2. RESPONSE INTERCEPTOR: Dynamic 401 Interception & Silent Refresh Loop
   ========================================================================== */
api.interceptors.response.use(
  (response) => response, 
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true; 

      try {
        const expiredToken = localStorage.getItem('token');
        const refreshToken = localStorage.getItem('refreshToken');

        if (!expiredToken || !refreshToken) {
          handleLogout();
          return Promise.reject(error);
        }

        console.log('Access token expired. Triggering background token refresh handshake...');

        // 🎯 Note: Since your BaseURL doesn't have /v1, we make sure the standalone 
        // background call explicitly targets the complete URL path to match your controller route.
        const response = await axios.post('https://localhost:7130/api/v1/Account/Refresh', {
          token: expiredToken,
          refreshToken: refreshToken
        });

        const { jwtToken, refreshToken: newRefreshToken } = response.data;

        localStorage.setItem('token', jwtToken);
        localStorage.setItem('refreshToken', newRefreshToken);

        originalRequest.headers.Authorization = `Bearer ${jwtToken}`;

        return axios(originalRequest);

      } catch (refreshError) {
        console.error('Refresh token is dead or expired. Evicting session.', refreshError);
        handleLogout();
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export const handleLogout = () => {
  localStorage.removeItem('token');
  localStorage.removeItem('refreshToken');
  LocalStorage.removeItem('guest_cart');
  window.location.href = '/login';
};

export default api;