import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Check, Loader2, X, ExternalLink, Sparkles } from "lucide-react";
import { usePageSEO } from "@/hooks/useSEO";
import AdminBusinessTabs from "@/components/admin/AdminBusinessTabs";
import { adminBusinessApi, type BusinessListingAdminDto } from "@/api/adminBusinessApi";
import toast from "@/utils/toast";

const PAGE_SIZE = 25;

export default function AdminBusinessListingsPage() {
  const { t } = useTranslation();
  usePageSEO({ title: "Admin · Business listings", description: "Moderate business listings.", index: false });

  const [rows, setRows] = useState<BusinessListingAdminDto[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [pendingOnly, setPendingOnly] = useState(true);
  const [loading, setLoading] = useState(true);
  const [actioning, setActioning] = useState<string | null>(null);

  const load = async () => {
    setLoading(true);
    try {
      const result = await adminBusinessApi.listListings({
        isApproved: pendingOnly ? false : undefined,
        page,
        pageSize: PAGE_SIZE,
      });
      setRows(result.items);
      setTotal(result.totalCount);
    } catch {
      toast.error(t("admin.business.listings.loadError"));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    let cancelled = false;
    void (async () => {
      if (cancelled) return;
      await load();
    })();
    return () => {
      cancelled = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pendingOnly, page]);

  const handleApprove = async (id: string) => {
    setActioning(id);
    try {
      await adminBusinessApi.approveListing(id);
      toast.success(t("admin.business.listings.approveToast"));
      await load();
    } catch {
      toast.error(t("admin.business.listings.actionError"));
    } finally {
      setActioning(null);
    }
  };

  const handleUnapprove = async (id: string) => {
    const reason = window.prompt(t("admin.business.listings.unapproveReason") || "Reason?");
    if (reason === null) return;
    setActioning(id);
    try {
      await adminBusinessApi.unapproveListing(id, reason);
      toast.success(t("admin.business.listings.unapproveToast"));
      await load();
    } catch {
      toast.error(t("admin.business.listings.actionError"));
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
        <button
          type="button"
          onClick={() => {
            setPage(1);
            setPendingOnly(true);
          }}
          className={`px-3 py-1.5 rounded-lg text-sm ${pendingOnly ? "bg-primary text-white" : "bg-white/10 text-white"}`}
        >
          {t("admin.business.listings.tabPending")}
        </button>
        <button
          type="button"
          onClick={() => {
            setPage(1);
            setPendingOnly(false);
          }}
          className={`px-3 py-1.5 rounded-lg text-sm ${!pendingOnly ? "bg-primary text-white" : "bg-white/10 text-white"}`}
        >
          {t("admin.business.listings.tabAll")}
        </button>
      </div>

      {loading ? (
        <div className="text-center py-20 text-gray-400">
          <Loader2 className="h-5 w-5 animate-spin mx-auto" />
        </div>
      ) : rows.length === 0 ? (
        <div className="text-center py-20 text-gray-400">
          {t("admin.business.listings.empty")}
        </div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-3">
          {rows.map((r) => (
            <div
              key={r.id}
              className="bg-[#2d2a42] border border-white/10 rounded-xl p-4 flex gap-3"
            >
              {r.coverPhotoUrl ? (
                <img
                  src={r.coverPhotoUrl}
                  alt=""
                  className="w-24 h-24 rounded-lg object-cover flex-shrink-0"
                />
              ) : (
                <div className="w-24 h-24 rounded-lg bg-gradient-to-br from-primary/15 to-mauve/15 flex items-center justify-center flex-shrink-0">
                  <Sparkles className="w-7 h-7 text-primary/40" />
                </div>
              )}
              <div className="flex-1 min-w-0">
                <div className="flex items-start justify-between gap-2">
                  <div className="min-w-0">
                    <p className="text-sm font-semibold text-white truncate">{r.title}</p>
                    <p className="text-xs text-gray-400 truncate">
                      {r.businessDisplayName} · {r.city}
                    </p>
                    <p className="text-[11px] text-gray-500 truncate">{r.ownerEmail}</p>
                  </div>
                  <span
                    className={`text-[10px] uppercase tracking-wide font-semibold px-2 py-0.5 rounded-full whitespace-nowrap ${
                      r.isApproved
                        ? "bg-emerald-500/20 text-emerald-300"
                        : "bg-amber-500/20 text-amber-300"
                    }`}
                  >
                    {r.isApproved
                      ? t("admin.business.listings.approved")
                      : t("admin.business.listings.pending")}
                  </span>
                </div>
                <div className="mt-2 flex items-center gap-3 text-[11px] text-gray-400">
                  <span>{r.viewCount} views</span>
                  <span>·</span>
                  <span>{r.likeCount} likes</span>
                  <span>·</span>
                  <span>{r.commentCount} comments</span>
                </div>
                <div className="mt-3 flex items-center gap-1.5">
                  <Link
                    to={`/coaches/${r.id}`}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="inline-flex items-center gap-1 px-2.5 py-1.5 rounded-lg bg-white/10 text-white text-xs hover:bg-white/15"
                  >
                    <ExternalLink size={11} />
                    {t("admin.business.listings.view")}
                  </Link>
                  {r.isApproved ? (
                    <button
                      type="button"
                      onClick={() => handleUnapprove(r.id)}
                      disabled={actioning === r.id}
                      className="inline-flex items-center gap-1 px-2.5 py-1.5 rounded-lg bg-amber-500/20 text-amber-300 text-xs hover:bg-amber-500/30 disabled:opacity-60"
                    >
                      <X size={11} />
                      {t("admin.business.listings.unapprove")}
                    </button>
                  ) : (
                    <button
                      type="button"
                      onClick={() => handleApprove(r.id)}
                      disabled={actioning === r.id}
                      className="inline-flex items-center gap-1 px-2.5 py-1.5 rounded-lg bg-emerald-500/20 text-emerald-300 text-xs hover:bg-emerald-500/30 disabled:opacity-60"
                    >
                      <Check size={11} />
                      {t("admin.business.listings.approve")}
                    </button>
                  )}
                </div>
              </div>
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
