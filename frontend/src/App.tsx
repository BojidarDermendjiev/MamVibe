import {
  lazy,
  Suspense,
  Component,
  type ReactNode,
  type ErrorInfo,
} from "react";
import { Routes, Route } from "react-router-dom";
import { useAuth } from "./hooks/useAuth";
import { ThemeProvider } from "./contexts/ThemeContext";
import { CategoriesProvider } from "./contexts/CategoriesContext";
import { SignalRProvider } from "./contexts/SignalRContext";
import { BusinessHubProvider } from "./contexts/BusinessHubContext";
import { NotificationProvider } from "./contexts/NotificationContext";
import MainLayout from "./layouts/MainLayout";
import AuthLayout from "./layouts/AuthLayout";
import AdminLayout from "./layouts/AdminLayout";
import ProtectedRoute from "./components/common/ProtectedRoute";
import LoadingSpinner from "./components/common/LoadingSpinner";

const HomePage = lazy(() => import("./pages/HomePage"));
const ModernAuthPage = lazy(() => import("./pages/ModernAuthPage"));
const BrowseItemsPage = lazy(() => import("./pages/BrowseItemsPage"));
const ItemDetailPage = lazy(() => import("./pages/ItemDetailPage"));
const CreateItemPage = lazy(() => import("./pages/CreateItemPage"));
const EditItemPage = lazy(() => import("./pages/EditItemPage"));
const DashboardPage = lazy(() => import("./pages/DashboardPage"));
const ProfilePage = lazy(() => import("./pages/ProfilePage"));
const SettingsPage = lazy(() => import("./pages/SettingsPage"));
const ChatPage = lazy(() => import("./pages/ChatPage"));
const PaymentPage = lazy(() => import("./pages/PaymentPage"));
const PaymentSuccessPage = lazy(() => import("./pages/PaymentSuccessPage"));
const PaymentCancelPage = lazy(() => import("./pages/PaymentCancelPage"));
const AdminDashboardPage = lazy(
  () => import("./pages/admin/AdminDashboardPage"),
);
const AdminUsersPage = lazy(() => import("./pages/admin/AdminUsersPage"));
const AdminItemsPage = lazy(() => import("./pages/admin/AdminItemsPage"));
const FeedbackPage = lazy(() => import("./pages/FeedbackPage"));
const AdminShippingPage = lazy(() => import("./pages/admin/AdminShippingPage"));
const AdminCommunityPage = lazy(() => import("./pages/admin/AdminCommunityPage"));
const AdminAuditLogPage = lazy(() => import("./pages/admin/AdminAuditLogPage"));
const AdminReportsPage = lazy(() => import("./pages/admin/AdminReportsPage"));
const AdminAbuseSignalsPage = lazy(() => import("./pages/admin/AdminAbuseSignalsPage"));
const AdminAppealsPage = lazy(() => import("./pages/admin/AdminAppealsPage"));
const AdminBusinessProfilesPage = lazy(() => import("./pages/admin/AdminBusinessProfilesPage"));
const AdminBusinessListingsPage = lazy(() => import("./pages/admin/AdminBusinessListingsPage"));
const AdminBusinessReferralsPage = lazy(() => import("./pages/admin/AdminBusinessReferralsPage"));
const AdminBusinessRevenuePage = lazy(() => import("./pages/admin/AdminBusinessRevenuePage"));
const DoctorReviewsPage = lazy(() => import("./pages/DoctorReviewsPage"));
const ChildFriendlyPlacesPage = lazy(() => import("./pages/ChildFriendlyPlacesPage"));
const ShipmentDetailPage = lazy(() => import("./pages/ShipmentDetailPage"));
const ForgotPasswordPage = lazy(() => import("./pages/ForgotPasswordPage"));
const ResetPasswordPage = lazy(() => import("./pages/ResetPasswordPage"));
const DonationPage = lazy(() => import("./pages/DonationPage"));
const NotFoundPage = lazy(() => import("./pages/NotFoundPage"));
const PrivacyPolicyPage = lazy(() => import("./pages/PrivacyPolicyPage"));
const TermsPage = lazy(() => import("./pages/TermsPage"));
const CookiePolicyPage = lazy(() => import("./pages/CookiePolicyPage"));
const BundleDetailPage = lazy(() => import("./pages/BundleDetailPage"));
const BundlePaymentPage = lazy(() => import("./pages/BundlePaymentPage"));
const AboutPage = lazy(() => import("./pages/AboutPage"));
const FaqPage = lazy(() => import("./pages/FaqPage"));
const HowItWorksPage = lazy(() => import("./pages/HowItWorksPage"));
const CoachesBrowsePage = lazy(() => import("./pages/coaches/CoachesBrowsePage"));
const CoachDetailPage = lazy(() => import("./pages/coaches/CoachDetailPage"));
const VenuesBrowsePage = lazy(() => import("./pages/coaches/VenuesBrowsePage"));
const BusinessRegisterPage = lazy(() => import("./pages/business/BusinessRegisterPage"));
const BusinessListingFormPage = lazy(() => import("./pages/business/BusinessListingFormPage"));
const BusinessPlanPage = lazy(() => import("./pages/business/BusinessPlanPage"));
const BusinessDashboardPage = lazy(() => import("./pages/business/BusinessDashboardPage"));
const SubscriptionSuccessPage = lazy(() => import("./pages/business/SubscriptionSuccessPage"));
const PartnerLoginPage = lazy(() => import("./pages/PartnerLoginPage"));
const PartnerRegisterPage = lazy(() => import("./pages/PartnerRegisterPage"));
const RecommendCoachPage = lazy(() => import("./pages/coaches/RecommendCoachPage"));
const PromoterDashboardPage = lazy(() => import("./pages/promoter/PromoterDashboardPage"));

class ErrorBoundary extends Component<
  { children: ReactNode },
  { hasError: boolean }
> {
  constructor(props: { children: ReactNode }) {
    super(props);
    this.state = { hasError: false };
  }
  static getDerivedStateFromError(_error: Error) {
    return { hasError: true };
  }
  componentDidCatch(error: Error, info: ErrorInfo) {
    if (import.meta.env.DEV) console.error("Uncaught error:", error, info);
  }
  render() {
    if (this.state.hasError) {
      return (
        <div className="min-h-screen flex items-center justify-center">
          <div className="text-center">
            <p className="text-lg font-semibold text-gray-700 dark:text-gray-300">
              Something went wrong.
            </p>
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
        <Route path="/login" element={<ModernAuthPage />} />
        <Route path="/register" element={<ModernAuthPage />} />

        {/* Partner (business) auth — dedicated visual track, same backend identity */}
        <Route path="/partner/login" element={<PartnerLoginPage />} />
        <Route path="/partner/register" element={<PartnerRegisterPage />} />
        <Route element={<AuthLayout />}>
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
            <Route path="/admin/community" element={<AdminCommunityPage />} />
            <Route path="/admin/audit-logs" element={<AdminAuditLogPage />} />
            <Route path="/admin/reports" element={<AdminReportsPage />} />
            <Route path="/admin/abuse-signals" element={<AdminAbuseSignalsPage />} />
            <Route path="/admin/appeals" element={<AdminAppealsPage />} />
            <Route path="/admin/business/profiles" element={<AdminBusinessProfilesPage />} />
            <Route path="/admin/business/listings" element={<AdminBusinessListingsPage />} />
            <Route path="/admin/business/referrals" element={<AdminBusinessReferralsPage />} />
            <Route path="/admin/business/revenue" element={<AdminBusinessRevenuePage />} />
          </Route>
        </Route>

        {/* Main layout routes */}
        <Route element={<MainLayout />}>
          <Route path="/" element={<HomePage />} />
          <Route path="/browse" element={<BrowseItemsPage />} />
          <Route path="/items/:id" element={<ItemDetailPage />} />
          <Route path="/bundles/:id" element={<BundleDetailPage />} />
          <Route path="/about" element={<AboutPage />} />
          <Route path="/faq" element={<FaqPage />} />
          <Route path="/how-it-works" element={<HowItWorksPage />} />
          <Route path="/privacy" element={<PrivacyPolicyPage />} />
          <Route path="/terms" element={<TermsPage />} />
          <Route path="/cookies" element={<CookiePolicyPage />} />
          <Route path="/donate" element={<DonationPage />} />
          <Route path="/doctor-reviews" element={<DoctorReviewsPage />} />
          <Route path="/child-friendly-places" element={<ChildFriendlyPlacesPage />} />
          <Route path="/coaches" element={<CoachesBrowsePage />} />
          <Route path="/coaches/:id" element={<CoachDetailPage />} />
          <Route path="/coaches/recommend" element={<RecommendCoachPage />} />
          <Route path="/venues" element={<VenuesBrowsePage />} />
          <Route path="/venues/:id" element={<CoachDetailPage />} />

          {/* Protected routes */}
          <Route element={<ProtectedRoute />}>
            <Route path="/business/register" element={<BusinessRegisterPage />} />
            <Route path="/business/listing/new" element={<BusinessListingFormPage />} />
            <Route path="/business/listing/edit" element={<BusinessListingFormPage />} />
            <Route path="/business/plan" element={<BusinessPlanPage />} />
            <Route path="/business/dashboard" element={<BusinessDashboardPage />} />
            <Route path="/business/subscription/success" element={<SubscriptionSuccessPage />} />
            <Route path="/promoter/dashboard" element={<PromoterDashboardPage />} />
            <Route path="/create" element={<CreateItemPage />} />
            <Route path="/items/:id/edit" element={<EditItemPage />} />
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/profile" element={<ProfilePage />} />
            <Route path="/settings" element={<SettingsPage />} />
            <Route path="/chat" element={<ChatPage />} />
            <Route path="/chat/:userId" element={<ChatPage />} />
            <Route path="/payment/:itemId" element={<PaymentPage />} />
            <Route path="/payment/bundle/:bundleId" element={<BundlePaymentPage />} />
            <Route path="/payment/success" element={<PaymentSuccessPage />} />
            <Route path="/payment/cancel" element={<PaymentCancelPage />} />
            <Route path="/feedback" element={<FeedbackPage />} />
            <Route
              path="/shipments/:shipmentId"
              element={<ShipmentDetailPage />}
            />
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
            <BusinessHubProvider>
              <NotificationProvider>
                <AppRoutes />
              </NotificationProvider>
            </BusinessHubProvider>
          </SignalRProvider>
        </CategoriesProvider>
      </ThemeProvider>
    </ErrorBoundary>
  );
}
