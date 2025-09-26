import { lazy } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import HealthCheck from './apis/HealthCheck';
import { ROUTES } from './constants';
import { ToastProvider } from './providers/ToastProvider';
import Login from './pages/Login';
import Register from './pages/Register';
import { AuthProvider } from './providers/AuthProvider';
import { ProtectedRoute } from './components/ProtectedRoute';
import Home from './pages/Home';
import { AppLayout } from './layouts/AppLayout';
import AddTransaction from './pages/AddTransactionForm';

const router = createBrowserRouter([
  {
    path: ROUTES.TEST,
    element: <HealthCheck />,
  },
  {
    path: ROUTES.LOGIN,
    element: <Login />,
  },
  {
    path: ROUTES.REGISTER,
    element: <Register />,
  },
  {
    element: <ProtectedRoute />,
    children: [
        {
            element: <AppLayout />,
            children: [
                {
                    path: ROUTES.HOME,
                    element: <Home />,
              },
              {
                path: ROUTES.ADD_TRANSACTION,
                element: <AddTransaction />,
              },
            ],
        },
    ],
  },
]);

const ReactQueryDevtools = lazy(() =>
  import('@tanstack/react-query-devtools').then((module) => ({
    default: module.ReactQueryDevtools,
  }))
);

function App() {
  const querryClient = new QueryClient();

  return (
    <QueryClientProvider client={querryClient}>
        <ReactQueryDevtools initialIsOpen={false} buttonPosition='bottom-right' />
      <ToastProvider>
        <AuthProvider>
          <RouterProvider router={router} />
        </AuthProvider>
      </ToastProvider>
    </QueryClientProvider>
  );
}

export default App;