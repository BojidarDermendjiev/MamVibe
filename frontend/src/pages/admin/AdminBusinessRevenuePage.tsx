import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import {
  CircleDollarSign,
  Loader2,
  TrendingUp,
  AlertCircle,
  Users,
  LayoutGrid,
} from "lucide-react";
import { usePageSEO } from "@/hooks/useSEO";
import AdminBusinessTabs from "@/components/admin/AdminBusinessTabs";
import { adminBusinessApi, type BusinessRevenueDto } from "@/api/adminBusinessApi";

export default function AdminBusinessRevenuePage() {
  const { t } = useTranslation();
  usePageSEO({ title: "Admin · Business revenue", description: "Subscription KPI snapshot.", index: false });

  const [stats, setStats] = useState<BusinessRevenueDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;
    void (async () => {
      try {
        const result = await adminBusinessApi.revenue();
        if (!cancelled) setStats(result);
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  if (loading) {
    return (
      <div>
        <h1 className="text-2xl font-bold text-[#bdb9bc] mb-4">
          {t("admin.business.heading")}
        </h1>
        <AdminBusinessTabs />
        <div className="text-center py-20 text-gray-400">
          <Loader2 className="h-5 w-5 animate-spin mx-auto" />
        </div>
      </div>
    );
  }

  if (!stats) return null;

  const cards = [
    {
      icon: CircleDollarSign,
      label: t("admin.business.revenue.mrr"),
      value: `€${stats.monthlyRecurringRevenueEur.toFixed(2)}`,
      color: "bg-emerald-500/10 text-emerald-300",
    },
    {
      icon: Users,
      label: t("admin.business.revenue.activeSubs"),
      value: stats.activeSubscriptionCount,
      color: "bg-primary/15 text-primary",
    },
    {
      icon: TrendingUp,
      label: t("admin.business.revenue.trialing"),
      value: stats.trialingSubscriptionCount,
      color: "bg-blue-500/15 text-blue-300",
    },
    {
      icon: AlertCircle,
      label: t("admin.business.revenue.pastDue"),
      value: stats.pastDueSubscriptionCount,
      color: "bg-amber-500/15 text-amber-300",
    },
    {
      icon: TrendingUp,
      label: t("admin.business.revenue.canceled30"),
      value: stats.canceledLast30Days,
      color: "bg-red-500/15 text-red-300",
    },
    {
      icon: TrendingUp,
      label: t("admin.business.revenue.trialToPaid"),
      value: `${Math.round(stats.trialToPaidConversionRate * 100)}%`,
      color: "bg-mauve/15 text-mauve",
    },
    {
      icon: LayoutGrid,
      label: t("admin.business.revenue.totalListings"),
      value: stats.totalListings,
      color: "bg-lavender/20 text-primary",
    },
    {
      icon: AlertCircle,
      label: t("admin.business.revenue.pendingApproval"),
      value: stats.pendingApprovalListings,
      color: "bg-amber-500/15 text-amber-300",
    },
  ];

  return (
    <div>
      <h1 className="text-2xl font-bold text-[#bdb9bc] mb-4">
        {t("admin.business.heading")}
      </h1>
      <AdminBusinessTabs />

      <div className="grid grid-cols-2 lg:grid-cols-4 gap-3 mb-6">
        {cards.map((c, i) => (
          <div key={i} className="bg-[#2d2a42] border border-white/10 rounded-xl p-4">
            <div className={`w-9 h-9 rounded-lg ${c.color} flex items-center justify-center mb-2`}>
              <c.icon className="h-5 w-5" />
            </div>
            <p className="text-[10px] uppercase tracking-wide text-gray-400 font-semibold">
              {c.label}
            </p>
            <p className="text-xl font-bold text-white mt-0.5">{c.value}</p>
          </div>
        ))}
      </div>

      <div className="bg-[#2d2a42] border border-white/10 rounded-xl p-5">
        <h3 className="text-sm font-semibold text-white mb-3">
          {t("admin.business.revenue.byTierHeading")}
        </h3>
        {stats.byTier.length === 0 ? (
          <p className="text-sm text-gray-400">{t("admin.business.revenue.noSubs")}</p>
        ) : (
          <table className="w-full text-sm text-white">
            <thead className="text-xs uppercase tracking-wide text-gray-400 border-b border-white/10">
              <tr>
                <th className="px-3 py-2 text-left">{t("admin.business.revenue.tierName")}</th>
                <th className="px-3 py-2 text-right">{t("admin.business.revenue.activeCount")}</th>
                <th className="px-3 py-2 text-right">{t("admin.business.revenue.contribution")}</th>
              </tr>
            </thead>
            <tbody>
              {stats.byTier.map((t) => (
                <tr key={t.planCode} className="border-b border-white/5 last:border-0">
                  <td className="px-3 py-2 font-medium">{t.planCode}</td>
                  <td className="px-3 py-2 text-right">{t.activeCount}</td>
                  <td className="px-3 py-2 text-right text-emerald-300">
                    €{t.monthlyContributionEur.toFixed(2)}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
