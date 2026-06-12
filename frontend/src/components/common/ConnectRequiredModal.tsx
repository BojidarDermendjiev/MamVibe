import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { ExternalLink, Wallet } from "lucide-react";
import Modal from "./Modal";
import Button from "./Button";

interface ConnectRequiredModalProps {
  isOpen: boolean;
  onClose: () => void;
}

/**
 * Shown when the backend rejects a paid-item listing with the
 * <c>connect_required</c> code — the user has not completed Stripe Connect
 * onboarding yet, so we'd have no payout target if a buyer paid them. The
 * primary action takes them straight to the Bank Payouts dashboard panel.
 */
export default function ConnectRequiredModal({ isOpen, onClose }: ConnectRequiredModalProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const handleGoToPayouts = () => {
    onClose();
    navigate("/dashboard#bank-payouts");
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={t("connect.required.title")}>
      <div className="space-y-4">
        <div className="flex items-start gap-3">
          <div className="flex-shrink-0 mt-0.5 w-10 h-10 rounded-full bg-primary/15 flex items-center justify-center">
            <Wallet className="h-5 w-5 text-primary" />
          </div>
          <p className="text-sm text-gray-600 dark:text-[#bdb9bc] leading-relaxed">
            {t("connect.required.body")}
          </p>
        </div>
        <Button fullWidth onClick={handleGoToPayouts}>
          <span className="inline-flex items-center gap-2">
            <ExternalLink className="h-4 w-4" />
            {t("connect.required.cta")}
          </span>
        </Button>
      </div>
    </Modal>
  );
}
