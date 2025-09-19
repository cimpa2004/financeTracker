import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import type { User } from '../types/User';

import { AuthContext } from '../contexts/authContext';

import { logout as apiLogout } from '../apis/Auth';
import { httpService } from '../services/httpService';
import { getStoredAuthData } from '../utils/Auth';
import { refreshToken as apiRefreshToken } from '../apis/Auth';

interface JWTPayload {
  iss: string;
  name: string;
  sub: string;
  emailVerified: boolean;
  exp: number;
  email: string;
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(() => {
    const storedData = getStoredAuthData();
    const initialToken = storedData?.token ?? null;
    return initialToken;
  });

  const [refreshToken, setRefreshToken] = useState<string | null>(() => {
    const storedData = getStoredAuthData();
    const initialRefreshToken = storedData?.refreshToken ?? null;
    return initialRefreshToken;
  });

  const [user, setUser] = useState<User | null>(() => {
    const storedData = getStoredAuthData();
    const initialUser = storedData?.user ?? null;

    return initialUser;
  });

  const [isRefreshing, setIsRefreshing] = useState(false);

  // Logout handler - clears all auth state
  const handleLogout = useCallback(async () => {
    try {
      await apiLogout();
    } catch (error) {
      console.warn('AuthProvider: API logout failed:', error);
    } finally {
      setToken(null);
      setRefreshToken(null);
      setUser(null);
      httpService.removeGlobalHeader('Authorization');
    }
  }, []);

  // Token refresh handler
  const handleTokenRefresh = useCallback(async (): Promise<boolean> => {
    if (isRefreshing) {
      return false;
    }

    setIsRefreshing(true);

    try {
      const response = await apiRefreshToken(localStorage.getItem('refreshToken') ?? '');

      setToken(response.accessToken);
      setRefreshToken(response.refreshToken);

      return true;
    } catch (error) {
      console.error('AuthProvider: Token refresh failed:', error);
      await handleLogout();
      return false;
    } finally {
      setIsRefreshing(false);
    }
  }, [isRefreshing, handleLogout]);

  // Safe JWT token parsing
  const parseTokenExpiry = useCallback((tokenString: string): number | null => {
    try {
      const parts = tokenString.split('.');
      if (parts.length !== 3) {
        return null;
      }

      const payload = JSON.parse(atob(parts[1])) as JWTPayload;
      if (typeof payload.exp !== 'number') {
        return null;
      }

      const expiryMs = payload.exp * 1000;
      return expiryMs;
    } catch (error) {
      console.error('AuthProvider: Error parsing JWT:', error);
      return null;
    }
  }, []);

  // Check if token needs refresh (5 minutes before expiry)
  const checkTokenExpiry = useCallback(
    (tokenString: string): boolean => {
      const exp = parseTokenExpiry(tokenString);
      if (!exp) {
        return true;
      }

      const now = Date.now();
      const timeUntilExpiry = exp - now;
      const fiveMinutesMs = 5 * 60 * 1000;
      const needsRefresh = timeUntilExpiry < fiveMinutesMs;

      return needsRefresh;
    },
    [parseTokenExpiry]
  );

  useEffect(() => {
    if (!token) {
      return;
    }

    if (checkTokenExpiry(token)) {
      void handleTokenRefresh();
      return;
    }

    // Set up periodic token expiry checks (every 10 minutes)
    const interval = setInterval(
      () => {
        if (token && checkTokenExpiry(token)) {
          void handleTokenRefresh();
        }
      },
      10 * 60 * 1000
    );

    return () => {
      clearInterval(interval);
    };
  }, [token, checkTokenExpiry, handleTokenRefresh]);

  useEffect(() => {
    if (token) {
      httpService.setGlobalHeader('Authorization', `Bearer ${token}`);
    } else {
      httpService.removeGlobalHeader('Authorization');
    }
  }, [token]);

  // Immediately set Authorization header if token exists on mount
  if (typeof window !== 'undefined') {
    const storedData = getStoredAuthData();
    if (storedData?.token) {
      httpService.setGlobalHeader('Authorization', `Bearer ${storedData.token}`);
    }
  }

  const contextValue = useMemo(() => {
    const isAuthenticated = Boolean(user && token);
    const isLoading = isRefreshing;

    return {
      user,
      token,
      refreshToken,
      isAuthenticated,
      isLoading,

      setAuthData: (t: string, rt: string, u: User) => {
        setToken(t);
        setRefreshToken(rt);
        setUser(u);

        httpService.setGlobalHeader('Authorization', `Bearer ${t}`);
      },

      logout: handleLogout,
      refreshAuthToken: handleTokenRefresh,
    };
  }, [token, refreshToken, user, isRefreshing, handleLogout, handleTokenRefresh]);

  return <AuthContext.Provider value={contextValue}>{children}</AuthContext.Provider>;
}