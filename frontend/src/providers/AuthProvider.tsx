import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import type { User } from '../types/User';

import { AuthContext } from '../contexts/AuthContext';

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

      // clear all persisted auth and other stored data
      localStorage.clear();

    }
  }, []);

  // Token refresh handler
  const handleTokenRefresh = useCallback(async (): Promise<boolean> => {
    if (isRefreshing) {
      return false;
    }

    setIsRefreshing(true);

    try {
      // prefer the state value (less likely to be stale), fall back to localStorage
      const rt = refreshToken ?? localStorage.getItem('refreshToken') ?? '';

      // guard: if there's no refresh token, bail and logout
      if (!rt) {
        console.warn('AuthProvider: no refresh token available for refresh call');
        await handleLogout();
        return false;
      }

      console.debug('AuthProvider: calling refresh with token length', rt.length);
      const response = await apiRefreshToken(rt);

      setToken(response.accessToken);
      setRefreshToken(response.refreshToken);

      // persist refreshed tokens
      localStorage.setItem('token', response.accessToken);
      localStorage.setItem('refreshToken', response.refreshToken);

      // update global header
      httpService.setGlobalHeader('Authorization', `Bearer ${response.accessToken}`);

      return true;
    } catch (error) {
      console.error('AuthProvider: Token refresh failed:', error);
      await handleLogout();
      return false;
    } finally {
      setIsRefreshing(false);
    }
  }, [isRefreshing, refreshToken, handleLogout]);

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

        // persist auth so getStoredAuthData and other code see it
        localStorage.setItem('token', t);
        localStorage.setItem('refreshToken', rt);
        localStorage.setItem('user', JSON.stringify(u));

        httpService.setGlobalHeader('Authorization', `Bearer ${t}`);
      },

      logout: handleLogout,
      refreshAuthToken: handleTokenRefresh,
    };
  }, [token, refreshToken, user, isRefreshing, handleLogout, handleTokenRefresh]);

  return <AuthContext.Provider value={contextValue}>{children}</AuthContext.Provider>;
}