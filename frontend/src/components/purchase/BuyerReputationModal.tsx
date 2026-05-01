import { AlertTriangle, ShieldAlert, ThumbsUp, X } from 'lucide-react';
import Avatar from '../common/Avatar';
import type { BuyerCheckResult } from '../../api/purchaseRequestsApi';

interface BuyerReputationModalProps {
  buyerName: string | null;
  buyerAvatarUrl: string | null;
  result: BuyerCheckResult;
  onAccept: () => void;
  onCancel: () => void;
}

export default function BuyerReputationModal({
  buyerName,
  buyerAvatarUrl,
  result,
  onAccept,
  onCancel,
}: BuyerReputationModalProps) {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40 backdrop-blur-sm animate-fade-in">
      <div className="w-full max-w-md bg-white dark:bg-gray-800 rounded-2xl shadow-2xl overflow-hidden">

        {/* ── Header ── */}
        <div className="relative bg-gradient-to-r from-amber-50 to-orange-50 dark:from-amber-900/40 dark:to-orange-900/40 border-b border-amber-100 dark:border-amber-800 px-6 py-5">
          <button
            onClick={onCancel}
            className="absolute top-4 right-4 p-1 rounded-full text-gray-500 dark:text-gray-300 hover:text-gray-700 hover:bg-amber-100/60 dark:hover:bg-amber-800/40 transition-colors"
          >
            <X size={16} />
          </button>
          <div className="flex items-start gap-3 pr-6">
            <div className="p-2.5 bg-amber-100 dark:bg-amber-800/60 rounded-xl shrink-0">
              <ShieldAlert className="w-5 h-5 text-amber-600 dark:text-amber-400" />
            </div>
            <div>
              <h2 className="font-semibold text-gray-900 dark:text-gray-100 text-base">Buyer Alert</h2>
              <p className="text-sm text-amber-800 dark:text-amber-300 mt-0.5 leading-snug">
                This buyer has fraud reports on{' '}
                <a
                  href="https://nekorekten.com"
                  target="_blank"
                  rel="noopener noreferrer"
                  className="font-semibold underline underline-offset-2 hover:text-amber-900 dark:hover:text-amber-200 transition-colors"
                >
                  Nekorekten.com
                </a>
              </p>
            </div>
          </div>
        </div>

        {/* ── Body ── */}
        <div className="px-6 py-5 space-y-4 bg-white dark:bg-gray-800">

          {/* Buyer info pill */}
          <div className="flex items-center gap-3 p-3 bg-peach-light/30 dark:bg-gray-700/50 rounded-xl border border-lavender/20 dark:border-gray-600">
            <Avatar src={buyerAvatarUrl} size="md" />
            <div className="min-w-0">
              <p className="font-medium text-primary dark:text-gray-100 truncate">{buyerName ?? 'Unknown Buyer'}</p>
              <span className="inline-flex items-center gap-1 text-xs font-medium text-amber-700 dark:text-amber-300 bg-amber-100 dark:bg-amber-900/50 px-2 py-0.5 rounded-full mt-0.5">
                <AlertTriangle size={10} />
                {result.reportCount} report{result.reportCount !== 1 ? 's' : ''} found
              </span>
            </div>
          </div>

          {/* Reports list */}
          {result.reports.length > 0 && (
            <div className="space-y-2 max-h-52 overflow-y-auto pr-0.5">
              {result.reports.map((r, i) => (
                <div
                  key={i}
                  className="rounded-xl border border-lavender/20 bg-gray-50 dark:bg-[#2a2740] p-3 text-sm"
                >
                  <p className="text-gray-700 dark:text-gray-300 leading-relaxed">
                    {r.text || 'No description provided.'}
                  </p>
                  <div className="flex items-center justify-between mt-2">
                    {r.createdAt && (
                      <span className="text-xs text-gray-400">
                        {new Date(r.createdAt).toLocaleDateString()}
                      </span>
                    )}
                    {r.likes > 0 && (
                      <span className="ml-auto flex items-center gap-1 text-xs text-gray-400">
                        <ThumbsUp size={11} />
                        {r.likes}
                      </span>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}

          <p className="text-xs text-gray-400 dark:text-gray-500 text-center">
            You can still accept — this is a warning only.
          </p>
        </div>

        {/* ── Actions ── */}
        <div className="px-6 pb-6 flex gap-3 bg-white dark:bg-gray-800">
          <button
            onClick={onCancel}
            className="flex-1 py-2.5 rounded-xl border-2 border-lavender/60 dark:border-gray-500 text-primary dark:text-gray-200 font-medium text-sm hover:bg-lavender/20 dark:hover:bg-gray-700 transition-colors"
          >
            Cancel
          </button>
          <button
            onClick={onAccept}
            className="flex-1 py-2.5 rounded-xl bg-amber-500 text-white font-semibold text-sm hover:bg-amber-600 active:scale-95 transition-all"
          >
            Accept Anyway
          </button>
        </div>

      </div>
    </div>
  );
}
