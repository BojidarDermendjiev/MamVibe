import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { CheckCircle2, Loader2, Phone, Mail, X, MessageCircle } from "lucide-react";
import { usePageSEO } from "@/hooks/useSEO";
import AdminBusinessTabs from "@/components/admin/AdminBusinessTabs";
import { adminBusinessApi, type CoachReferralAdminDto } from "@/api/adminBusinessApi";
import { CoachReferralStatus } from "@/types/business";
import toast from "@/utils/toast";

const PAGE_SIZE = 25;

const STATUS_COLOR: Record<CoachReferralStatus, string> = {
  [CoachReferralStatus.Submitted]: "bg-gray-500/20 text-gray-300",
  [CoachReferralStatus.Contacted]: "bg-amber-500/20 text-amber-300",
  [CoachReferralStatus.Onboarded]: "bg-emerald-500/20 text-emerald-300",
  [CoachReferralStatus.Rejected]: "bg-red-500/20 text-red-300",
};

export default function AdminBusinessReferralsPage() {
  const { t } = useTranslation();
  usePageSEO({ title: "Admin · Coach referrals", description: "Review coach referrals.", index: false });

  const [rows, setRows] = useState<CoachReferralAdminDto[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [statusFilter, setStatusFilter] = useState<CoachReferralStatus | "">("");
  const [loading, setLoading] = useState(true);
  const [actioning, setActioning] = useState<string | null>(null);

  const load = async () => {
    setLoading(true);
    try {
      const result = await adminBusinessApi.listReferrals({
        status: statusFilter === "" ? undefined : statusFilter,
        page,
        pageSize: PAGE_SIZE,
      });
      setRows(result.items);
      setTotal(result.totalCount);
    } catch {
      toast.error(t("admin.business.referrals.loadError"));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    let cancelled = false;
    void (async () => {
      if (!cancelled) await load();
    })();
    return () => {
      cancelled = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [statusFilter, page]);

  const transition = async (id: string, status: CoachReferralStatus) => {
    let adminNote: string | null = null;
    if (status === CoachReferralStatus.Rejected) {
      adminNote = window.prompt(t("admin.business.referrals.rejectReason") || "Reason?");
      if (adminNote === null) return;
    }
    setActioning(id);
    try {
      await adminBusinessApi.updateReferralStatus(id, status, adminNote ?? undefined);
      toast.success(t("admin.business.referrals.updatedToast"));
      await load();
    } catch {
      toast.error(t("admin.business.referrals.actionError"));
    } finally {
      setActioning(null);
    }
  };

  const totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));

  return (
    <div>
      <h1 className="text-2xl font-bold text-[#bdb9bc] mb-4">
        {t("admin.business.heading")}
      </h1>
      <AdminBusinessTabs />

      <div className="flex items-center gap-2 mb-4">
        <select
          value={statusFilter}
          onChange={(e) => {
            setPage(1);
            setStatusFilter(e.target.value === "" ? "" : (Number(e.target.value) as CoachReferralStatus));
          }}
          className="px-3 py-1.5 rounded-lg bg-white/5 border border-white/10 text-sm text-white"
        >
          <option value="">{t("admin.business.referrals.allStatuses")}</option>
          <option value={CoachReferralStatus.Submitted}>{t("promoter.status.submitted")}</option>
          <option value={CoachReferralStatus.Contacted}>{t("promoter.status.contacted")}</option>
          <option value={CoachReferralStatus.Onboarded}>{t("promoter.status.onboarded")}</option>
          <option value={CoachReferralStatus.Rejected}>{t("promoter.status.rejected")}</option>
        </select>
      </div>

      {loading ? (
        <div className="text-center py-20 text-gray-400">
          <Loader2 className="h-5 w-5 animate-spin mx-auto" />
        </div>
      ) : rows.length === 0 ? (
        <div className="text-center py-20 text-gray-400">
          {t("admin.business.referrals.empty")}
        </div>
      ) : (
        <div className="space-y-2">
          {rows.map((r) => (
            <div
              key={r.id}
              className="bg-[#2d2a42] border border-white/10 rounded-xl p-4"
            >
              <div className="flex items-start justify-between gap-3 mb-2">
                <div className="min-w-0">
                  <p className="font-semibold text-white">{r.businessName}</p>
                  <p className="text-xs text-gray-400">
                    {r.city} · {new Date(r.createdAt).toLocaleDateString()}
                  </p>
                </div>
                <span
                  className={`text-[10px] uppercase tracking-wide font-semibold px-2 py-0.5 rounded-full whitespace-nowrap ${STATUS_COLOR[r.status]}`}
                >
                  {t(`promoter.status.${["submitted", "contacted", "onboarded", "rejected"][r.status]}`)}
                </span>
              </div>

              <div className="flex flex-wrap gap-3 text-xs text-gray-300 mb-2">
                {r.contactEmail && (
                  <a href={`mailto:${r.contactEmail}`} className="inline-flex items-center gap-1 hover:text-primary">
                    <Mail size={11} />
                    {r.contactEmail}
                  </a>
                )}
                {r.contactPhone && (
                  <a href={`tel:${r.contactPhone}`} className="inline-flex items-center gap-1 hover:text-primary">
                    <Phone size={11} />
                    {r.contactPhone}
                  </a>
                )}
                {r.referralCode && (
                  <span className="inline-flex items-center gap-1 font-mono">
                    <span className="px-1.5 py-0.5 rounded bg-primary/20 text-primary">
                      {r.referralCode}
                    </span>
                  </span>
                )}
              </div>

              {r.notes && (
                <p className="text-xs text-gray-300 bg-white/5 rounded-lg px-3 py-2 mb-2">
                  {r.notes}
                </p>
              )}

              {r.adminNote && (
                <p className="text-[11px] text-amber-300 mb-2">
                  Note: {r.adminNote}
                </p>
              )}

              {r.status !== CoachReferralStatus.Onboarded && r.status !== CoachReferralStatus.Rejected && (
                <div className="flex flex-wrap gap-1.5 mt-2">
                  {r.status === CoachReferralStatus.Submitted && (
                    <button
                      type="button"
                      onClick={() => transition(r.id, CoachReferralStatus.Contacted)}
                      disabled={actioning === r.id}
                      className="inline-flex items-center gap-1 px-2.5 py-1.5 rounded-lg bg-amber-500/20 text-amber-300 text-xs hover:bg-amber-500/30 disabled:opacity-60"
                    >
                      <MessageCircle size={11} />
                      {t("admin.business.referrals.markContacted")}
                    </button>
                  )}
                  <button
                    type="button"
                    onClick={() => transition(r.id, CoachReferralStatus.Onboarded)}
                    disabled={actioning === r.id}
                    className="inline-flex items-center gap-1 px-2.5 py-1.5 rounded-lg bg-emerald-500/20 text-emerald-300 text-xs hover:bg-emerald-500/30 disabled:opacity-60"
                  >
                    <CheckCircle2 size={11} />
                    {t("admin.business.referrals.markOnboarded")}
                  </button>
                  <button
                    type="button"
                    onClick={() => transition(r.id, CoachReferralStatus.Rejected)}
                    disabled={actioning === r.id}
                    className="inline-flex items-center gap-1 px-2.5 py-1.5 rounded-lg bg-red-500/20 text-red-300 text-xs hover:bg-red-500/30 disabled:opacity-60"
                  >
                    <X size={11} />
                    {t("admin.business.referrals.markRejected")}
                  </button>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {totalPages > 1 && (
        <div className="flex justify-center items-center gap-3 mt-4">
          <button
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page === 1}
            className="px-3 py-1.5 rounded-lg bg-white/10 text-white text-sm disabled:opacity-40"
          >
            {t("coaches.prev")}
          </button>
          <span className="text-sm text-gray-300">
            {t("coaches.pageOf", { page, total: totalPages })}
          </span>
          <button
            onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
            disabled={page === totalPages}
            className="px-3 py-1.5 rounded-lg bg-white/10 text-white text-sm disabled:opacity-40"
          >
            {t("coaches.next")}
          </button>
        </div>
      )}
    </div>
  );
}
