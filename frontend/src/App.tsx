import { lazy, Suspense } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import HealthCheck from './apis/HealthCheck';
import { ROUTES } from './constants';
import { ToastProvider } from './providers/ToastProvider';
import Login from './pages/Login';
import Register from './pages/Register';
import { AuthProvider } from './providers/AuthProvider';

const router = createBrowserRouter([
  {
    path: ROUTES.TEST,
    element: <HealthCheck />,
  },
  {
    index: true,
    path: ROUTES.LOGIN,
    element: <Login />,
  },
  {
    path: ROUTES.REGISTER,
    element: <Register />,
  }
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
        <Suspense fallback={<div>Loading Devtools...</div>}>
          <ReactQueryDevtools initialIsOpen={false} buttonPosition='bottom-right' />
        </Suspense>
        <ToastProvider>
            <AuthProvider>
                <RouterProvider router={router} />
            </AuthProvider>
        </ToastProvider>

    </QueryClientProvider>
  )
}

export default App