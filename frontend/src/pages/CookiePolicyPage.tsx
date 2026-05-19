import { type ReactNode } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { usePageSEO } from "@/hooks/useSEO";

const LAST_UPDATED = "19 May 2026";

function Section({
  num, id, title, children,
}: {
  num: number; id: string; title: string; children: ReactNode;
}) {
  return (
    <section id={id} className="mb-14 scroll-mt-20">
      <div className="flex items-start gap-4 mb-5">
        <span className="flex-none mt-1.5 text-xs font-bold text-gray-300 dark:text-gray-600 w-4 text-right select-none">
          {num}
        </span>
        <h2 className="text-2xl font-bold text-gray-900 dark:text-white leading-snug">{title}</h2>
      </div>
      <div className="ml-8 space-y-3 text-[15px] text-gray-600 dark:text-gray-300 leading-relaxed">
        {children}
      </div>
    </section>
  );
}

function Li({ children }: { children: ReactNode }) {
  return (
    <li className="flex gap-2">
      <span className="text-primary mt-1.5 shrink-0 text-xs">•</span>
      <span>{children}</span>
    </li>
  );
}

function CookieCard({
  title, description, accent = false,
}: {
  title: string; description: string; accent?: boolean;
}) {
  return (
    <div className={`rounded-lg border p-4 ${accent
      ? "border-primary/30 dark:border-primary/20 bg-primary/5 dark:bg-primary/10"
      : "border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900"
    }`}>
      <p className={`font-semibold text-sm mb-1 ${accent ? "text-primary" : "text-gray-800 dark:text-gray-100"}`}>
        {title}
      </p>
      <p className="text-sm text-gray-500 dark:text-gray-400">{description}</p>
    </div>
  );
}

export default function CookiePolicyPage() {
  const { t } = useTranslation();

  usePageSEO({
    title: "Cookie Policy — MamVibe",
    description:
      "MamVibe uses only one essential cookie for authentication. Read our transparent cookie policy to understand exactly what we store and why.",
    canonical: "https://mamvibe.com/cookies",
    index: true,
  });

  const cookieRows: [string, string][] = [
    [t("cookiesPage.s2_row_purpose"),  t("cookiesPage.s2_row_purpose_val")],
    [t("cookiesPage.s2_row_set_by"),   t("cookiesPage.s2_row_set_by_val")],
    [t("cookiesPage.s2_row_duration"), t("cookiesPage.s2_row_duration_val")],
    ["HttpOnly",                        t("cookiesPage.s2_row_httponly_val")],
    ["Secure",                          t("cookiesPage.s2_row_secure_val")],
    ["SameSite",                        t("cookiesPage.s2_row_samesite_val")],
    [t("cookiesPage.s2_row_path"),     t("cookiesPage.s2_row_path_val")],
    [t("cookiesPage.s2_row_content"),  t("cookiesPage.s2_row_content_val")],
  ];

  const localStorageRows: [string, string, string][] = [
    ["mamvibe-auth",   t("cookiesPage.s3_auth_stores"),    t("cookiesPage.s3_auth_lifetime")],
    ["language",       t("cookiesPage.s3_lang_stores"),    t("cookiesPage.s3_lang_lifetime")],
    ["cookieConsent",  t("cookiesPage.s3_consent_stores"), t("cookiesPage.s3_consent_lifetime")],
  ];

  const browsers = [
    { browser: "Chrome",  path: t("cookiesPage.s5_chrome") },
    { browser: "Firefox", path: t("cookiesPage.s5_firefox") },
    { browser: "Safari",  path: t("cookiesPage.s5_safari") },
    { browser: "Edge",    path: t("cookiesPage.s5_edge") },
  ];

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-[#0e0c1a] py-14 px-4">
      <div className="max-w-3xl mx-auto">

        {/* Page header */}
        <header className="mb-12">
          <h1 className="text-4xl font-bold text-gray-900 dark:text-white mb-1">
            {t("cookiesPage.title")}
          </h1>
          <p className="text-sm text-gray-400 dark:text-gray-500">
            {t("legalPages.last_updated")} {LAST_UPDATED}
          </p>
          <hr className="mt-6 border-gray-200 dark:border-gray-700" />
        </header>

        {/* Intro */}
        <p className="text-[15px] text-gray-600 dark:text-gray-300 leading-relaxed mb-14">
          {t("cookiesPage.hero_subtitle_1")}{" "}
          <strong className="text-gray-900 dark:text-white">{t("cookiesPage.hero_exactly_one")}</strong>
          {" "}{t("cookiesPage.hero_subtitle_2")}
        </p>

        {/* Sections */}
        <Section num={1} id="what-are-cookies" title={t("cookiesPage.toc_1")}>
          <p>{t("cookiesPage.s1_p1")}</p>
          <p>{t("cookiesPage.s1_p2")}</p>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-3 mt-1">
            <CookieCard accent title={t("cookiesPage.s1_cat_necessary")}   description={t("cookiesPage.s1_cat_necessary_desc")} />
            <CookieCard       title={t("cookiesPage.s1_cat_analytics")}    description={t("cookiesPage.s1_cat_analytics_desc")} />
            <CookieCard       title={t("cookiesPage.s1_cat_advertising")}  description={t("cookiesPage.s1_cat_advertising_desc")} />
          </div>
          <p>{t("cookiesPage.s1_only_necessary")}</p>
        </Section>

        <Section num={2} id="our-cookie" title={t("cookiesPage.toc_2")}>
          <p>{t("cookiesPage.s2_intro")}</p>

          {/* Cookie detail card */}
          <div className="rounded-lg border border-gray-200 dark:border-gray-700 overflow-hidden">
            <div className="flex items-center justify-between px-5 py-3 bg-gray-100 dark:bg-gray-800">
              <span className="font-mono font-semibold text-gray-800 dark:text-gray-100">refreshToken</span>
              <span className="text-xs font-semibold text-primary border border-primary/30 bg-primary/5 rounded-full px-2 py-0.5">
                {t("cookiesPage.s2_badge")}
              </span>
            </div>
            <div className="bg-white dark:bg-gray-900 divide-y divide-gray-100 dark:divide-gray-700">
              {cookieRows.map(([key, value]) => (
                <div key={key} className="flex gap-4 px-5 py-2.5 text-sm">
                  <span className="w-28 shrink-0 font-medium text-gray-700 dark:text-gray-200">{key}</span>
                  <span className="text-gray-600 dark:text-gray-400">{value}</span>
                </div>
              ))}
            </div>
          </div>

          <div className="rounded-lg border border-blue-200 dark:border-blue-800/40 bg-blue-50 dark:bg-blue-900/10 p-4 text-sm text-blue-800 dark:text-blue-300">
            <strong>{t("cookiesPage.s2_consent_label")}</strong> {t("cookiesPage.s2_consent_text")}
          </div>
        </Section>

        <Section num={3} id="local-storage" title={t("cookiesPage.toc_3")}>
          <p>{t("cookiesPage.s3_p1")}</p>

          <div className="rounded-lg border border-gray-200 dark:border-gray-700 overflow-hidden">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 dark:bg-gray-800 text-gray-700 dark:text-gray-200">
                <tr>
                  <th className="text-left px-4 py-3 font-semibold">{t("cookiesPage.s3_th_key")}</th>
                  <th className="text-left px-4 py-3 font-semibold">{t("cookiesPage.s3_th_stores")}</th>
                  <th className="text-left px-4 py-3 font-semibold">{t("cookiesPage.s3_th_lifetime")}</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 dark:divide-gray-700 bg-white dark:bg-gray-900">
                {localStorageRows.map(([key, stores, lifetime]) => (
                  <tr key={key} className="hover:bg-gray-50 dark:hover:bg-gray-800/50">
                    <td className="px-4 py-3 font-mono text-xs text-gray-700 dark:text-gray-200">{key}</td>
                    <td className="px-4 py-3 text-gray-600 dark:text-gray-400">{stores}</td>
                    <td className="px-4 py-3 text-gray-600 dark:text-gray-400">{lifetime}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <p>{t("cookiesPage.s3_footer")}</p>
        </Section>

        <Section num={4} id="third-party" title={t("cookiesPage.toc_4")}>
          <p>{t("cookiesPage.s4_p1")}</p>
          <p>{t("cookiesPage.s4_p2")}</p>
          <ul className="space-y-1.5">
            <Li><strong>{t("cookiesPage.s4_stripe_name")}</strong> — {t("cookiesPage.s4_stripe_text")}</Li>
            <Li><strong>{t("cookiesPage.s4_cf_name")}</strong> — {t("cookiesPage.s4_cf_text")}</Li>
            <Li><strong>{t("cookiesPage.s4_google_name")}</strong> — {t("cookiesPage.s4_google_text")}</Li>
          </ul>
          <p>{t("cookiesPage.s4_footer")}</p>
        </Section>

        <Section num={5} id="managing" title={t("cookiesPage.toc_5")}>
          <p>
            {t("cookiesPage.s5_p1_pre")}{" "}
            <code className="bg-gray-100 dark:bg-gray-800 px-1.5 py-0.5 rounded text-sm font-mono">refreshToken</code>{" "}
            {t("cookiesPage.s5_p1_post")}
          </p>
          <ul className="space-y-1.5">
            <Li><strong>{t("cookiesPage.s5_logout_label")}</strong> — {t("cookiesPage.s5_logout_text")}</Li>
            <Li><strong>{t("cookiesPage.s5_clear_label")}</strong> — {t("cookiesPage.s5_clear_text")}</Li>
            <Li><strong>{t("cookiesPage.s5_incognito_label")}</strong> — {t("cookiesPage.s5_incognito_text")}</Li>
          </ul>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 mt-2">
            {browsers.map(({ browser, path }) => (
              <div
                key={browser}
                className="rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 px-4 py-3"
              >
                <p className="font-semibold text-sm text-gray-800 dark:text-gray-100">{browser}</p>
                <p className="text-xs text-gray-400 dark:text-gray-500 mt-0.5">{path}</p>
              </div>
            ))}
          </div>
        </Section>

        <Section num={6} id="more-info" title={t("cookiesPage.toc_6")}>
          <p>
            {t("cookiesPage.s6_p1_pre")}{" "}
            <Link to="/privacy" className="text-primary hover:underline font-medium">
              {t("legalPages.nav_privacy")}
            </Link>
            . {t("cookiesPage.s6_p1_mid")}{" "}
            <a href="mailto:support@mamvibe.com" className="text-primary hover:underline">
              support@mamvibe.com
            </a>
            .
          </p>
          <p>{t("cookiesPage.s6_p2")}</p>
          <ul className="space-y-1">
            <Li><span className="font-mono text-sm">allaboutcookies.org</span> {t("cookiesPage.s6_allaboutcookies")}</Li>
            <Li><span className="font-mono text-sm">cpdp.bg</span> {t("cookiesPage.s6_cpdp")}</Li>
            <Li><span className="font-mono text-sm">edpb.europa.eu</span> {t("cookiesPage.s6_edpb")}</Li>
          </ul>
        </Section>

      </div>
    </div>
  );
}
