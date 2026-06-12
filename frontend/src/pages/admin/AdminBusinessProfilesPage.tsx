import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { AlertTriangle, Loader2, Search, ShieldOff, ShieldCheck, Trash2 } from "lucide-react";
import { usePageSEO } from "@/hooks/useSEO";
import AdminBusinessTabs from "@/components/admin/AdminBusinessTabs";
import { adminBusinessApi, type BusinessProfileAdminDto } from "@/api/adminBusinessApi";
import { BusinessCategory } from "@/types/business";
import toast from "@/utils/toast";

const STATUS_LABEL: Record<number, string> = {
  0: "admin.business.profiles.status.pendingPolicy",
  1: "admin.business.profiles.status.pendingPayment",
  2: "admin.business.profiles.status.active",
  3: "admin.business.profiles.status.pastDue",
  4: "admin.business.profiles.status.suspended",
  5: "admin.business.profiles.status.removed",
};

const STATUS_COLOR: Record<number, string> = {
  0: "bg-gray-500/20 text-gray-300",
  1: "bg-amber-500/20 text-amber-300",
  2: "bg-emerald-500/20 text-emerald-300",
  3: "bg-orange-500/20 text-orange-300",
  4: "bg-red-500/20 text-red-300",
  5: "bg-red-500/30 text-red-200",
};

const PAGE_SIZE = 25;

export default function AdminBusinessProfilesPage() {
  const { t } = useTranslation();
  usePageSEO({ title: "Admin · Business profiles", description: "Moderate business profiles.", index: false });

  const [rows, setRows] = useState<BusinessProfileAdminDto[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [searchApplied, setSearchApplied] = useState("");
  const [categoryFilter, setCategoryFilter] = useState<BusinessCategory | "">("");
  const [statusFilter, setStatusFilter] = useState<number | "">("");
  const [loading, setLoading] = useState(true);
  const [actioning, setActioning] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    const load = async () => {
      if (cancelled) return;
      setLoading(true);
      try {
        const result = await adminBusinessApi.listProfiles({
          category: categoryFilter === "" ? undefined : categoryFilter,
          status: statusFilter === "" ? undefined : statusFilter,
          search: searchApplied || undefined,
          page,
          pageSize: PAGE_SIZE,
        });
        if (!cancelled) {
          setRows(result.items);
          setTotal(result.totalCount);
        }
      } catch {
        if (!cancelled) toast.error(t("admin.business.profiles.loadError") || "Could not load profiles.");
      } finally {
        if (!cancelled) setLoading(false);
      }
    };
    void load();
    return () => {
      cancelled = true;
    };
  }, [categoryFilter, statusFilter, searchApplied, page, t]);

  const totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));

  const action = async (
    id: string,
    op: "suspend" | "restore" | "remove",
    reason?: string,
  ) => {
    setActioning(id);
    try {
      if (op === "suspend") await adminBusinessApi.suspendProfile(id, reason ?? "");
      if (op === "restore") await adminBusinessApi.restoreProfile(id);
      if (op === "remove") await adminBusinessApi.removeProfile(id, reason ?? "");
      const result = await adminBusinessApi.listProfiles({
        category: categoryFilter === "" ? undefined : categoryFilter,
        status: statusFilter === "" ? undefined : statusFilter,
        search: searchApplied || undefined,
        page,
        pageSize: PAGE_SIZE,
      });
      setRows(result.items);
      setTotal(result.totalCount);
      toast.success(t(`admin.business.profiles.${op}Toast`) || "Done");
    } catch {
      toast.error(t("admin.business.profiles.actionError") || "Action failed.");
    } finally {
      setActioning(null);
    }
  };

  const handleSuspend = (row: BusinessProfileAdminDto) => {
    const reason = window.prompt(t("admin.business.profiles.suspendReason") || "Reason for suspension?");
    if (reason === null) return;
    void action(row.id, "suspend", reason);
  };
  const handleRemove = (row: BusinessProfileAdminDto) => {
    const reason = window.prompt(t("admin.business.profiles.removeReason") || "Reason for removal?");
    if (reason === null) return;
    if (!window.confirm(t("admin.business.profiles.removeConfirm") || "Permanently remove this profile?")) return;
    void action(row.id, "remove", reason);
  };

  return (
    <div>
      <h1 className="text-2xl font-bold text-[#bdb9bc] mb-4">
        {t("admin.business.heading")}
      </h1>
      <AdminBusinessTabs />

      <form
        onSubmit={(e) => {
          e.preventDefault();
          setPage(1);
          setSearchApplied(search.trim());
        }}
        className="bg-[#2d2a42] border border-white/10 rounded-xl p-3 flex flex-wrap gap-2 mb-4"
      >
        <div className="relative flex-1 min-w-[200px]">
          <Search size={13} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder={t("admin.business.profiles.searchPlaceholder") || "Search…"}
            className="w-full pl-8 pr-3 py-1.5 rounded-lg bg-white/5 border border-white/10 text-sm text-white placeholder-gray-500"
          />
        </div>
        <select
          value={categoryFilter}
          onChange={(e) => {
            setPage(1);
            setCategoryFilter(e.target.value === "" ? "" : (Number(e.target.value) as BusinessCategory));
          }}
          className="px-3 py-1.5 rounded-lg bg-white/5 border border-white/10 text-sm text-white"
        >
          <option value="">{t("admin.business.profiles.allCategories")}</option>
          <option value={BusinessCategory.Coach}>{t("admin.business.profiles.coachCategory")}</option>
          <option value={BusinessCategory.VenueAdvertiser}>{t("admin.business.profiles.venueCategory")}</option>
        </select>
        <select
          value={statusFilter}
          onChange={(e) => {
            setPage(1);
            setStatusFilter(e.target.value === "" ? "" : Number(e.target.value));
          }}
          className="px-3 py-1.5 rounded-lg bg-white/5 border border-white/10 text-sm text-white"
        >
          <option value="">{t("admin.business.profiles.allStatuses")}</option>
          {[0, 1, 2, 3, 4].map((s) => (
            <option key={s} value={s}>
              {t(STATUS_LABEL[s])}
            </option>
          ))}
        </select>
        <button
          type="submit"
          className="px-4 py-1.5 rounded-lg bg-primary text-white text-sm font-medium"
        >
          {t("admin.business.profiles.applyFilters")}
        </button>
      </form>

      <div className="bg-[#2d2a42] border border-white/10 rounded-xl overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm text-white">
            <thead className="bg-white/5 text-xs uppercase tracking-wide text-gray-400">
              <tr>
                <th className="px-3 py-2 text-left">{t("admin.business.profiles.columns.business")}</th>
                <th className="px-3 py-2 text-left">{t("admin.business.profiles.columns.owner")}</th>
                <th className="px-3 py-2 text-left">{t("admin.business.profiles.columns.city")}</th>
                <th className="px-3 py-2 text-left">{t("admin.business.profiles.columns.category")}</th>
                <th className="px-3 py-2 text-left">{t("admin.business.profiles.columns.status")}</th>
                <th className="px-3 py-2 text-left">{t("admin.business.profiles.columns.subscription")}</th>
                <th className="px-3 py-2 text-left">{t("admin.business.profiles.columns.listing")}</th>
                <th className="px-3 py-2 text-right">{t("admin.business.profiles.columns.actions")}</th>
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr>
                  <td colSpan={8} className="text-center py-10 text-gray-400">
                    <Loader2 className="h-5 w-5 animate-spin mx-auto" />
                  </td>
                </tr>
              ) : rows.length === 0 ? (
                <tr>
                  <td colSpan={8} className="text-center py-10 text-gray-400">
                    {t("admin.business.profiles.empty")}
                  </td>
                </tr>
              ) : (
                rows.map((r) => (
                  <tr key={r.id} className="border-t border-white/5">
                    <td className="px-3 py-2 align-top">
                      <p className="font-medium">{r.displayName}</p>
                      <p className="text-[11px] text-gray-400">{r.legalName}</p>
                      {r.hasDeviceConflict && (
                        <span className="inline-flex items-center gap-1 mt-1 text-[10px] uppercase font-semibold px-1.5 py-0.5 rounded-full bg-amber-500/20 text-amber-300">
                          <AlertTriangle size={10} />
                          {t("admin.business.profiles.deviceConflict")}
                        </span>
                      )}
                    </td>
                    <td className="px-3 py-2 align-top">
                      <p className="text-xs text-gray-300">{r.ownerEmail}</p>
                    </td>
                    <td className="px-3 py-2 align-top text-xs">{r.city}</td>
                    <td className="px-3 py-2 align-top">
                      <span className="text-xs">
                        {r.category === BusinessCategory.Coach
                          ? t("admin.business.profiles.coachCategory")
                          : t("admin.business.profiles.venueCategory")}
                      </span>
                    </td>
                    <td className="px-3 py-2 align-top">
                      <span
                        className={`inline-block text-[10px] uppercase tracking-wide font-semibold px-2 py-0.5 rounded-full ${STATUS_COLOR[r.status]}`}
                      >
                        {t(STATUS_LABEL[r.status])}
                      </span>
                    </td>
                    <td className="px-3 py-2 align-top text-xs">
                      {r.subscriptionPlanCode ? (
                        <>
                          {r.subscriptionPlanCode}
                          {r.subscriptionStatus != null && (
                            <span className="ml-1 text-[10px] text-gray-400">
                              ({r.subscriptionStatus})
                            </span>
                          )}
                        </>
                      ) : (
                        <span className="text-gray-500">—</span>
                      )}
                    </td>
                    <td className="px-3 py-2 align-top text-xs">
                      {r.hasListing ? (
                        r.isListingApproved ? (
                          <span className="text-emerald-300">{t("admin.business.profiles.listingApproved")}</span>
                        ) : (
                          <span className="text-amber-300">{t("admin.business.profiles.listingPending")}</span>
                        )
                      ) : (
                        <span className="text-gray-500">—</span>
                      )}
                    </td>
                    <td className="px-3 py-2 align-top text-right">
                      <div className="inline-flex gap-1.5">
                        {r.status === 4 ? (
                          <button
                            type="button"
                            onClick={() => action(r.id, "restore")}
                            disabled={actioning === r.id}
                            className="p-1.5 rounded-lg bg-emerald-500/20 text-emerald-300 hover:bg-emerald-500/30 disabled:opacity-60"
                            title={t("admin.business.profiles.restore") || ""}
                          >
                            <ShieldCheck size={14} />
                          </button>
                        ) : (
                          <button
                            type="button"
                            onClick={() => handleSuspend(r)}
                            disabled={actioning === r.id}
                            className="p-1.5 rounded-lg bg-amber-500/20 text-amber-300 hover:bg-amber-500/30 disabled:opacity-60"
                            title={t("admin.business.profiles.suspend") || ""}
                          >
                            <ShieldOff size={14} />
                          </button>
                        )}
                        <button
                          type="button"
                          onClick={() => handleRemove(r)}
                          disabled={actioning === r.id}
                          className="p-1.5 rounded-lg bg-red-500/20 text-red-300 hover:bg-red-500/30 disabled:opacity-60"
                          title={t("admin.business.profiles.remove") || ""}
                        >
                          <Trash2 size={14} />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

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
