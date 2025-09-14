import { lazy, Suspense } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import HealthCheck from './apis/HealthCheck';
import { ROUTES } from './constants';
import { ToastProvider } from './providers/ToastProvider';

const router = createBrowserRouter([
  {
    path: ROUTES.TEST,
    element: <HealthCheck />,
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
        <Suspense fallback={<div>Loading Devtools...</div>}>
          <ReactQueryDevtools initialIsOpen={false} buttonPosition='bottom-right' />
        </Suspense>
        <ToastProvider>
            <RouterProvider router={router} />      
        </ToastProvider>

    </QueryClientProvider>
  )
}

export default App