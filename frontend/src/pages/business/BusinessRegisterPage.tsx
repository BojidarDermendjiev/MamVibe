import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { motion } from "framer-motion";
import {
  Sparkles,
  Loader2,
  Building2,
  User as UserIcon,
  ShieldCheck,
  ArrowRight,
} from "lucide-react";
import axios from "axios";
import { usePageSEO } from "@/hooks/useSEO";
import BusinessPolicyModal from "@/components/business/BusinessPolicyModal";
import { businessApi } from "@/api/businessApi";
import { useAuthStore } from "@/store/authStore";
import { useBusinessStore } from "@/store/businessStore";
import { getVisitorId } from "@/lib/fingerprint";
import {
  BusinessCategory,
  ProfileKind,
  type BusinessErrorEnvelope,
  type BusinessPolicyDto,
} from "@/types/business";
import toast from "@/utils/toast";

type Step = "policy" | "form" | "success";

interface FormState {
  category: BusinessCategory;
  profileKind: ProfileKind;
  legalName: string;
  displayName: string;
  bio: string;
  contactEmail: string;
  contactPhone: string;
  website: string;
  city: string;
}

const EMPTY_FORM: FormState = {
  category: BusinessCategory.Coach,
  profileKind: ProfileKind.Coach,
  legalName: "",
  displayName: "",
  bio: "",
  contactEmail: "",
  contactPhone: "",
  website: "",
  city: "",
};

/**
 * Multi-step business registration: (1) policy acceptance, (2) profile form
 * with device-fingerprint capture, (3) success splash redirecting to the
 * dashboard. Existing profile detection short-circuits to step 3.
 */
export default function BusinessRegisterPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { isAuthenticated, user } = useAuthStore();
  const { setProfile } = useBusinessStore();

  const [step, setStep] = useState<Step>("policy");
  const [policy, setPolicy] = useState<BusinessPolicyDto | null>(null);
  const [policyModalOpen, setPolicyModalOpen] = useState(true);
  const [form, setForm] = useState<FormState>(() => ({
    ...EMPTY_FORM,
    contactEmail: user?.email ?? "",
  }));
  const [submitting, setSubmitting] = useState(false);
  const [fingerprintError, setFingerprintError] = useState<string | null>(null);
  const [serverError, setServerError] = useState<string | null>(null);

  usePageSEO({
    title: t("business.register.seoTitle") || "Register your business",
    description:
      t("business.register.seoDescription") ||
      "Start your 7-day trial and reach families in your city.",
    index: false,
  });

  // Detect existing profile so a returning user doesn't restart from step 1.
  useEffect(() => {
    if (!isAuthenticated) return;
    businessApi
      .getMyProfile()
      .then((profile) => {
        setProfile(profile);
        setStep("success");
        setPolicyModalOpen(false);
      })
      .catch((err) => {
        // 404 = no profile yet, anything else = leave the user on the wizard
        if (axios.isAxiosError(err) && err.response?.status !== 404) {
          // Surface a non-blocking warning; the wizard still works.
          console.warn("Failed to check existing business profile", err);
        }
      });
  }, [isAuthenticated, setProfile]);

  // Warm the fingerprint agent the moment the wizard mounts so the submit click
  // does not wait for the first agent load.
  useEffect(() => {
    getVisitorId().catch((err) => {
      console.warn("FingerprintJS preload failed", err);
    });
  }, []);

  if (!isAuthenticated) {
    return (
      <div className="min-h-[50vh] flex items-center justify-center px-4">
        <div className="text-center max-w-md">
          <p className="text-gray-700 dark:text-gray-300 mb-4">
            {t("business.register.loginRequired")}
          </p>
          <button
            onClick={() => navigate("/login?next=/business/register")}
            className="px-5 py-2.5 rounded-xl bg-primary text-white text-sm font-semibold hover:bg-primary/90 transition-colors"
          >
            {t("nav.login") || "Sign in"}
          </button>
        </div>
      </div>
    );
  }

  const handlePolicyAccept = async (accepted: BusinessPolicyDto) => {
    setPolicy(accepted);
    setPolicyModalOpen(false);
    setStep("form");
    // Don't write the acceptance row yet — it's persisted atomically with the profile
    // create call below. Recording it twice would double-count for evidence purposes.
  };

  const handlePolicyClose = () => {
    setPolicyModalOpen(false);
    // No policy acceptance — bounce to /coaches if they back out before form
    if (!policy) navigate("/coaches");
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!policy) {
      setPolicyModalOpen(true);
      return;
    }

    setSubmitting(true);
    setFingerprintError(null);
    setServerError(null);

    let visitorId: string;
    try {
      visitorId = await getVisitorId();
    } catch {
      setFingerprintError(t("business.register.fingerprintError"));
      setSubmitting(false);
      return;
    }

    try {
      const created = await businessApi.createProfile({
        category: form.category,
        profileKind: form.profileKind,
        legalName: form.legalName.trim(),
        displayName: form.displayName.trim(),
        bio: form.bio.trim() || undefined,
        contactEmail: form.contactEmail.trim(),
        contactPhone: form.contactPhone.trim() || undefined,
        website: form.website.trim() || undefined,
        city: form.city.trim(),
        policyVersionId: policy.id,
        fingerprintVisitorId: visitorId,
      });
      setProfile(created);
      setStep("success");
      toast.success(t("business.register.successToast") || "Profile created!");
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.data) {
        const envelope = err.response.data as BusinessErrorEnvelope;
        switch (envelope.code) {
          case "device_already_has_business":
            setServerError(t("business.register.errors.deviceAlreadyHasBusiness"));
            break;
          case "profile_already_exists":
            setServerError(t("business.register.errors.profileAlreadyExists"));
            break;
          case "fingerprint_missing":
            setFingerprintError(t("business.register.fingerprintError"));
            break;
          default:
            setServerError(envelope.error || t("business.register.errors.generic"));
        }
      } else {
        setServerError(t("business.register.errors.generic"));
      }
    } finally {
      setSubmitting(false);
    }
  };

  if (step === "success") {
    return (
      <div className="max-w-xl mx-auto px-4 py-16 text-center">
        <motion.div
          initial={{ scale: 0.9, opacity: 0 }}
          animate={{ scale: 1, opacity: 1 }}
          transition={{ duration: 0.3 }}
          className="inline-flex h-16 w-16 rounded-2xl bg-gradient-to-br from-primary/20 to-mauve/20 items-center justify-center mb-5"
        >
          <ShieldCheck className="h-8 w-8 text-primary" />
        </motion.div>
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">
          {t("business.register.successHeading")}
        </h1>
        <p className="text-gray-500 dark:text-gray-400 mb-6">
          {t("business.register.successBody")}
        </p>
        <button
          onClick={() => navigate("/business/plan")}
          className="inline-flex items-center gap-2 px-5 py-2.5 rounded-xl bg-primary text-white text-sm font-semibold hover:bg-primary/90 transition-colors"
        >
          {t("business.register.successCta")} <ArrowRight size={16} />
        </button>
      </div>
    );
  }

  return (
    <div className="max-w-2xl mx-auto px-4 py-10">
      <BusinessPolicyModal
        isOpen={policyModalOpen}
        onAccept={handlePolicyAccept}
        onClose={handlePolicyClose}
      />

      <motion.div
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.35 }}
      >
        <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-primary/10 text-primary text-xs font-semibold mb-3">
          <Sparkles size={13} />
          {t("business.register.trialBadge")}
        </div>
        <h1 className="text-2xl sm:text-3xl font-bold text-primary-dark dark:text-white mb-2">
          {t("business.register.heading")}
        </h1>
        <p className="text-gray-500 dark:text-gray-400 mb-6">
          {t("business.register.subtitle")}
        </p>
      </motion.div>

      {step === "form" && (
        <form
          onSubmit={handleSubmit}
          className="bg-white dark:bg-[#2d2a42] border border-lavender/30 dark:border-white/10 rounded-2xl shadow-sm p-6 space-y-5"
        >
          {/* Category — Coach vs Venue Advertiser */}
          <div>
            <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-2">
              {t("business.register.categoryLabel")}
            </label>
            <div className="grid grid-cols-2 gap-2">
              {[
                { value: BusinessCategory.Coach, icon: UserIcon, key: "categoryCoach" },
                { value: BusinessCategory.VenueAdvertiser, icon: Building2, key: "categoryVenue" },
              ].map(({ value, icon: Icon, key }) => {
                const active = form.category === value;
                return (
                  <button
                    key={value}
                    type="button"
                    onClick={() => setForm((f) => ({ ...f, category: value }))}
                    className={`flex items-center gap-2 px-4 py-3 rounded-xl border text-sm font-medium transition-all ${
                      active
                        ? "border-primary bg-primary/10 text-primary"
                        : "border-gray-200 dark:border-white/10 text-gray-700 dark:text-gray-300 hover:border-primary/40"
                    }`}
                  >
                    <Icon size={16} />
                    {t(`business.register.${key}`)}
                  </button>
                );
              })}
            </div>
          </div>

          {/* Profile kind toggle */}
          <div>
            <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-2">
              {t("business.register.kindLabel")}
            </label>
            <div className="grid grid-cols-2 gap-2">
              {[
                { value: ProfileKind.Coach, icon: UserIcon, key: "kindCoach" },
                { value: ProfileKind.Agency, icon: Building2, key: "kindAgency" },
              ].map(({ value, icon: Icon, key }) => {
                const active = form.profileKind === value;
                return (
                  <button
                    key={value}
                    type="button"
                    onClick={() => setForm((f) => ({ ...f, profileKind: value }))}
                    className={`flex items-center gap-2 px-4 py-3 rounded-xl border text-sm font-medium transition-all ${
                      active
                        ? "border-primary bg-primary/10 text-primary"
                        : "border-gray-200 dark:border-white/10 text-gray-700 dark:text-gray-300 hover:border-primary/40"
                    }`}
                  >
                    <Icon size={16} />
                    {t(`business.register.${key}`)}
                  </button>
                );
              })}
            </div>
          </div>

          <FieldRow
            label={t("business.register.displayNameLabel")}
            placeholder={t("business.register.displayNamePlaceholder")}
            value={form.displayName}
            onChange={(v) => setForm((f) => ({ ...f, displayName: v }))}
            maxLength={100}
            required
          />
          <FieldRow
            label={t("business.register.legalNameLabel")}
            placeholder={t("business.register.legalNamePlaceholder")}
            value={form.legalName}
            onChange={(v) => setForm((f) => ({ ...f, legalName: v }))}
            maxLength={200}
            required
          />

          <div>
            <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
              {t("business.register.bioLabel")}
            </label>
            <textarea
              maxLength={2000}
              rows={3}
              value={form.bio}
              onChange={(e) => setForm((f) => ({ ...f, bio: e.target.value }))}
              placeholder={t("business.register.bioPlaceholder")}
              className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40 resize-none"
            />
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <FieldRow
              label={t("business.register.emailLabel")}
              type="email"
              value={form.contactEmail}
              onChange={(v) => setForm((f) => ({ ...f, contactEmail: v }))}
              maxLength={254}
              required
            />
            <FieldRow
              label={t("business.register.phoneLabel")}
              value={form.contactPhone}
              onChange={(v) => setForm((f) => ({ ...f, contactPhone: v }))}
              maxLength={32}
              placeholder="+359 …"
            />
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <FieldRow
              label={t("business.register.cityLabel")}
              value={form.city}
              onChange={(v) => setForm((f) => ({ ...f, city: v }))}
              maxLength={100}
              required
            />
            <FieldRow
              label={t("business.register.websiteLabel")}
              value={form.website}
              onChange={(v) => setForm((f) => ({ ...f, website: v }))}
              maxLength={2048}
              placeholder="https://…"
            />
          </div>

          {fingerprintError && (
            <p className="text-xs text-amber-600 dark:text-amber-400 bg-amber-50 dark:bg-amber-500/10 rounded-lg px-3 py-2">
              {fingerprintError}
            </p>
          )}
          {serverError && (
            <p className="text-sm text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-500/10 rounded-lg px-3 py-2">
              {serverError}
            </p>
          )}

          <div className="flex flex-col sm:flex-row gap-3 pt-1">
            <button
              type="button"
              onClick={() => setPolicyModalOpen(true)}
              className="flex-1 px-4 py-2.5 rounded-xl border border-gray-200 dark:border-white/10 text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-white/5 transition-colors"
            >
              {t("business.register.reviewPolicy")}
            </button>
            <button
              type="submit"
              disabled={submitting}
              className="flex-1 inline-flex items-center justify-center gap-2 px-4 py-2.5 rounded-xl bg-primary text-white text-sm font-semibold shadow-sm hover:bg-primary/90 disabled:opacity-60 transition-colors"
            >
              {submitting ? (
                <Loader2 className="h-4 w-4 animate-spin" />
              ) : (
                <>
                  {t("business.register.submitButton")} <ArrowRight size={15} />
                </>
              )}
            </button>
          </div>
        </form>
      )}
    </div>
  );
}

interface FieldRowProps {
  label: string;
  value: string;
  onChange: (next: string) => void;
  placeholder?: string;
  maxLength?: number;
  required?: boolean;
  type?: string;
}

function FieldRow({
  label,
  value,
  onChange,
  placeholder,
  maxLength,
  required,
  type = "text",
}: FieldRowProps) {
  return (
    <div>
      <label className="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">
        {label}
        {required && <span className="text-red-500 ml-0.5">*</span>}
      </label>
      <input
        type={type}
        value={value}
        required={required}
        maxLength={maxLength}
        placeholder={placeholder}
        onChange={(e) => onChange(e.target.value)}
        className="w-full px-3 py-2 rounded-lg border border-gray-200 dark:border-white/10 bg-gray-50 dark:bg-white/5 text-sm text-gray-800 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-primary/40"
      />
    </div>
  );
}
