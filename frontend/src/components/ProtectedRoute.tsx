import { type ReactNode } from 'react';
import { Navigate, Outlet } from 'react-router-dom';

import { useAuth } from '../contexts/AuthContext';
import { ROUTES } from '../constants';

interface ProtectedRouteProps {
  children?: ReactNode;
  redirectPath?: string;
}

export const ProtectedRoute = ({ children, redirectPath = ROUTES.LOGIN }: ProtectedRouteProps) => {
  const { isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to={redirectPath} replace />;
  }

  return <>{children ?? <Outlet />}</>;
};