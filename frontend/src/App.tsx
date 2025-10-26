//import { lazy } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { RouterProvider } from 'react-router-dom';
import { ToastProvider } from './providers/ToastProvider';
import { AppThemeProvider } from './theme';
import { AuthProvider } from './providers/AuthProvider';
import { router } from './routes';

// const ReactQueryDevtools = lazy(() =>
//   import('@tanstack/react-query-devtools').then((module) => ({
//     default: module.ReactQueryDevtools,
//   }))
// );

function App() {
  const querryClient = new QueryClient();

  return (
    <QueryClientProvider client={querryClient}>
      {/* <ReactQueryDevtools initialIsOpen={false} buttonPosition='bottom-right' /> */}
      <AppThemeProvider>
        <ToastProvider>
          <AuthProvider>
            <RouterProvider router={router} />
          </AuthProvider>
        </ToastProvider>
      </AppThemeProvider>
    </QueryClientProvider>
  );
}

export default App;