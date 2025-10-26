//import { lazy } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import HealthCheck from './apis/HealthCheck';
import { ROUTES } from './constants';
import { ToastProvider } from './providers/ToastProvider';
import { AppThemeProvider } from './theme';
import Login from './pages/Login';
import Register from './pages/Register';
import { AuthProvider } from './providers/AuthProvider';
import { ProtectedRoute } from './components/navigation/ProtectedRoute';
import Home from './pages/Home';
import { AppLayout } from './layouts/AppLayout';
import AddTransaction from './pages/AddTransactionForm';
import AddCategoryPage from './pages/AddCategoryPage';
import WelcomePage from './pages/WelcomePage';
import BudgetChartsPage from './pages/BudgetChartsPage';
import AddBudgetPage from './pages/AddBudgetPage';
import StatisticsPage from './pages/StatisticsPage';
import ReportsPage from './pages/ReportsPage';
import AllTransactions from './pages/AllTransactions';
import Profile from './pages/Profile';

const router = createBrowserRouter([
  {
    index: true,
    element: <WelcomePage />
  },
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
              {
                path: ROUTES.ADD_CATEGORY,
                element: <AddCategoryPage />,
              },
              {
                path: ROUTES.BUDGET_CHARTS,
                element: <BudgetChartsPage />,
              },
              {
                path: ROUTES.ADD_BUDGET,
                element: <AddBudgetPage />,
              },
              {
                path: ROUTES.STATISTICS,
                element: <StatisticsPage />,
              },
              {
                path: ROUTES.REPORTS,
                element: <ReportsPage />,
              },
              {
                path: ROUTES.ALL_TRANSACTIONS,
                element: <AllTransactions />,
              }
              ,
              {
                path: ROUTES.PROFILE,
                element: <Profile />,
              }
            ],
        },
    ],
  },
]);

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