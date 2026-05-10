import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import Button from "../components/common/Button";
import { usePageSEO } from "@/hooks/useSEO";

export default function NotFoundPage() {
  const { t } = useTranslation();

  // Noindex 404 pages — they must not consume crawl budget.
  usePageSEO({
    title: "Page Not Found",
    description: "The page you are looking for does not exist. Return to MamVibe to browse baby items.",
    index: false,
  });

  return (
    <main className="grid min-h-full place-items-center px-6 py-24 sm:py-32 lg:px-8">
      <div className="text-center">
        <p className="text-base font-semibold text-primary sm:text-7xl">404</p>
        <h1 className="mt-4 text-5xl font-semibold tracking-tight text-balance sm:text-7xl">
          Page not found
        </h1>
        <p className="mt-6 text-lg font-medium text-pretty text-gray-500 sm:text-xl/8">
          Sorry, we couldn't find the page you're looking for.
        </p>
        <div className="mt-10 flex items-center justify-center gap-x-6">
          <Link to="/">
            <Button>{t("common.back")}</Button>
          </Link>
        </div>
      </div>
    </main>
  );
}
