import { lazy, Suspense, Component, type ReactNode, type ErrorInfo } from "react";
import { Routes, Route } from "react-router-dom";
import { useAuth } from "./hooks/useAuth";
import { ThemeProvider } from "./contexts/ThemeContext";
import { CategoriesProvider } from "./contexts/CategoriesContext";
import { SignalRProvider } from "./contexts/SignalRContext";
import { NotificationProvider } from "./contexts/NotificationContext";
import MainLayout from "./layouts/MainLayout";
import AuthLayout from "./layouts/AuthLayout";
import AdminLayout from "./layouts/AdminLayout";
import ProtectedRoute from "./components/common/ProtectedRoute";
import LoadingSpinner from "./components/common/LoadingSpinner";

const HomePage = lazy(() => import("./pages/HomePage"));
const LoginPage = lazy(() => import("./pages/LoginPage"));
const RegisterPage = lazy(() => import("./pages/RegisterPage"));
const BrowseItemsPage = lazy(() => import("./pages/BrowseItemsPage"));
const ItemDetailPage = lazy(() => import("./pages/ItemDetailPage"));
const CreateItemPage = lazy(() => import("./pages/CreateItemPage"));
const EditItemPage = lazy(() => import("./pages/EditItemPage"));
const DashboardPage = lazy(() => import("./pages/DashboardPage"));
const ProfilePage = lazy(() => import("./pages/ProfilePage"));
const SettingsPage = lazy(() => import("./pages/SettingsPage"));
const ChatPage = lazy(() => import("./pages/ChatPage"));
const PaymentPage = lazy(() => import("./pages/PaymentPage"));
const PaymentItemCardPage = lazy(() => import("./pages/PaymentItemCardPage"));
const PaymentSuccessPage = lazy(() => import("./pages/PaymentSuccessPage"));
const PaymentCancelPage = lazy(() => import("./pages/PaymentCancelPage"));
const AdminDashboardPage = lazy(
  () => import("./pages/admin/AdminDashboardPage"),
);
const AdminUsersPage = lazy(() => import("./pages/admin/AdminUsersPage"));
const AdminItemsPage = lazy(() => import("./pages/admin/AdminItemsPage"));
const FeedbackPage = lazy(() => import("./pages/FeedbackPage"));
const AdminShippingPage = lazy(
  () => import("./pages/admin/AdminShippingPage"),
);
const ShipmentDetailPage = lazy(() => import("./pages/ShipmentDetailPage"));
const ForgotPasswordPage = lazy(() => import("./pages/ForgotPasswordPage"));
const ResetPasswordPage = lazy(() => import("./pages/ResetPasswordPage"));
const DonationPage = lazy(() => import("./pages/DonationPage"));
const DonationCardPage = lazy(() => import("./pages/DonationCardPage"));
const NotFoundPage = lazy(() => import("./pages/NotFoundPage"));

class ErrorBoundary extends Component<{ children: ReactNode }, { hasError: boolean }> {
  constructor(props: { children: ReactNode }) {
    super(props);
    this.state = { hasError: false };
  }
  static getDerivedStateFromError(_: Error) {
    return { hasError: true };
  }
  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error('Uncaught error:', error, info);
  }
  render() {
    if (this.state.hasError) {
      return (
        <div className="min-h-screen flex items-center justify-center">
          <div className="text-center">
            <p className="text-lg font-semibold text-gray-700 dark:text-gray-300">Something went wrong.</p>
            <button
              className="mt-4 px-4 py-2 bg-primary text-white rounded-lg"
              onClick={() => this.setState({ hasError: false })}
            >
              Try again
            </button>
          </div>
        </div>
      );
    }
    return this.props.children;
  }
}

const PageLoader = () => (
  <div className="min-h-screen flex items-center justify-center">
    <LoadingSpinner size="lg" />
  </div>
);

function AppRoutes() {
  useAuth();

  return (
    <Suspense fallback={<PageLoader />}>
      <Routes>
        {/* Auth routes */}
        <Route element={<AuthLayout />}>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/forgot-password" element={<ForgotPasswordPage />} />
          <Route path="/reset-password" element={<ResetPasswordPage />} />
        </Route>

        {/* Admin routes */}
        <Route element={<ProtectedRoute requiredRole="Admin" />}>
          <Route element={<AdminLayout />}>
            <Route path="/admin" element={<AdminDashboardPage />} />
            <Route path="/admin/users" element={<AdminUsersPage />} />
            <Route path="/admin/items" element={<AdminItemsPage />} />
            <Route path="/admin/shipping" element={<AdminShippingPage />} />
          </Route>
        </Route>

        {/* Main layout routes */}
        <Route element={<MainLayout />}>
          <Route path="/" element={<HomePage />} />
          <Route path="/browse" element={<BrowseItemsPage />} />
          <Route path="/items/:id" element={<ItemDetailPage />} />
          <Route path="/donate" element={<DonationPage />} />
          <Route path="/donate/card" element={<DonationCardPage />} />

          {/* Protected routes */}
          <Route element={<ProtectedRoute />}>
            <Route path="/create" element={<CreateItemPage />} />
            <Route path="/items/:id/edit" element={<EditItemPage />} />
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/profile" element={<ProfilePage />} />
            <Route path="/settings" element={<SettingsPage />} />
            <Route path="/chat" element={<ChatPage />} />
            <Route path="/chat/:userId" element={<ChatPage />} />
            <Route path="/payment/:itemId" element={<PaymentPage />} />
            <Route path="/payment/:itemId/card" element={<PaymentItemCardPage />} />
            <Route path="/payment/success" element={<PaymentSuccessPage />} />
            <Route path="/payment/cancel" element={<PaymentCancelPage />} />
            <Route path="/feedback" element={<FeedbackPage />} />
            <Route path="/shipments/:shipmentId" element={<ShipmentDetailPage />} />
          </Route>

          <Route path="*" element={<NotFoundPage />} />
        </Route>
      </Routes>
    </Suspense>
  );
}

export default function App() {
  return (
    <ErrorBoundary>
      <ThemeProvider>
        <CategoriesProvider>
          <SignalRProvider>
            <NotificationProvider>
              <AppRoutes />
            </NotificationProvider>
          </SignalRProvider>
        </CategoriesProvider>
      </ThemeProvider>
    </ErrorBoundary>
  );
}
