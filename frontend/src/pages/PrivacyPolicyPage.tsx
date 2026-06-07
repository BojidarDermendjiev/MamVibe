import { type ReactNode } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { usePageSEO } from "@/hooks/useSEO";

const LAST_UPDATED = "8 June 2026";

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

function H3({ children }: { children: ReactNode }) {
  return <h3 className="font-semibold text-gray-800 dark:text-gray-100 mt-4 mb-1">{children}</h3>;
}

function Li({ children }: { children: ReactNode }) {
  return (
    <li className="flex gap-2">
      <span className="text-primary mt-1.5 shrink-0 text-xs">•</span>
      <span>{children}</span>
    </li>
  );
}

function Table({ headers, rows }: { headers: string[]; rows: string[][] }) {
  return (
    <div className="overflow-x-auto rounded-lg border border-gray-200 dark:border-gray-700">
      <table className="w-full text-sm">
        <thead className="bg-gray-50 dark:bg-gray-800 text-gray-700 dark:text-gray-200">
          <tr>
            {headers.map((h) => (
              <th key={h} className="text-left px-4 py-3 font-semibold">{h}</th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-100 dark:divide-gray-700 bg-white dark:bg-gray-900">
          {rows.map((row, i) => (
            <tr key={i} className="hover:bg-gray-50 dark:hover:bg-gray-800/50">
              {row.map((cell, j) => (
                <td
                  key={j}
                  className={`px-4 py-3 ${j === 0 ? "text-gray-700 dark:text-gray-200" : j === row.length - 1 ? "font-mono text-xs text-primary" : ""}`}
                >
                  {cell}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default function PrivacyPolicyPage() {
  const { t } = useTranslation();

  usePageSEO({
    title: "Privacy Policy — MamVibe",
    description:
      "Learn how MamVibe collects, uses, and protects your personal data in compliance with the GDPR and Bulgarian data protection law.",
    canonical: "https://mamvibe.com/privacy",
    index: true,
  });

  const legalBasisRows = [
    [t("privacyPage.s4_r1_purpose"), t("privacyPage.s4_r1_basis"), "Art. 6(1)(b)"],
    [t("privacyPage.s4_r2_purpose"), t("privacyPage.s4_r2_basis"), "Art. 6(1)(f)"],
    [t("privacyPage.s4_r3_purpose"), t("privacyPage.s4_r3_basis"), "Art. 6(1)(b)"],
    [t("privacyPage.s4_r4_purpose"), t("privacyPage.s4_r4_basis"), "Art. 6(1)(f)"],
    [t("privacyPage.s4_r5_purpose"), t("privacyPage.s4_r5_basis"), "Art. 6(1)(a)"],
    [t("privacyPage.s4_r6_purpose"), t("privacyPage.s4_r6_basis"), "Art. 6(1)(c)"],
  ];

  const processors = [
    ["Stripe",                          t("privacyPage.s5_stripe_purpose"),   t("privacyPage.s5_stripe_data")],
    [t("privacyPage.s5_couriers_name"), t("privacyPage.s5_couriers_purpose"), t("privacyPage.s5_couriers_data")],
    ["TakeANap",                        t("privacyPage.s5_takeanap_purpose"), t("privacyPage.s5_takeanap_data")],
    ["Google",                          t("privacyPage.s5_google_purpose"),   t("privacyPage.s5_google_data")],
    ["Cloudflare",                      t("privacyPage.s5_cf_purpose"),       t("privacyPage.s5_cf_data")],
    ["Anthropic",                       t("privacyPage.s5_anthropic_purpose"),t("privacyPage.s5_anthropic_data")],
    ["SMTP provider",                   t("privacyPage.s5_smtp_purpose"),     t("privacyPage.s5_smtp_data")],
    ["n8n (self-hosted)",               t("privacyPage.s5_n8n_purpose"),      t("privacyPage.s5_n8n_data")],
  ];

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-[#0e0c1a] py-14 px-4">
      <div className="max-w-3xl mx-auto">

        {/* Page header */}
        <header className="mb-12">
          <h1 className="text-4xl font-bold text-gray-900 dark:text-white mb-1">
            {t("privacyPage.title")}
          </h1>
          <p className="text-sm text-gray-400 dark:text-gray-500">
            {t("legalPages.effective")} {LAST_UPDATED}
          </p>
          <hr className="mt-6 border-gray-200 dark:border-gray-700" />
        </header>

        {/* Intro */}
        <p className="text-[15px] text-gray-600 dark:text-gray-300 leading-relaxed mb-14">
          {t("privacyPage.hero_subtitle")}
        </p>

        {/* Sections */}
        <Section num={1} id="controller" title={t("privacyPage.toc_1")}>
          <p>
            {t("privacyPage.s1_p1_pre")}
            <strong>{t("privacyPage.pronoun_we")}</strong>,{" "}
            <strong>{t("privacyPage.pronoun_us")}</strong>,{" "}
            <strong>{t("privacyPage.pronoun_our")}</strong>
            {t("privacyPage.s1_p1_mid")}{" "}
            <a href="mailto:support@mamvibe.com" className="text-primary hover:underline">
              support@mamvibe.com
            </a>
            .
          </p>
          <p>{t("privacyPage.s1_p2")}</p>
        </Section>

        <Section num={2} id="what-we-collect" title={t("privacyPage.toc_2")}>
          <H3>{t("privacyPage.s2_h_account")}</H3>
          <ul className="space-y-1">
            <Li>{t("privacyPage.s2_name")}</Li>
            <Li>{t("privacyPage.s2_email")}</Li>
            <Li>{t("privacyPage.s2_photo")}</Li>
            <Li>{t("privacyPage.s2_type")}</Li>
            <Li>{t("privacyPage.s2_bio")}</Li>
            <Li>{t("privacyPage.s2_google")}</Li>
          </ul>

          <H3>{t("privacyPage.s2_h_transactions")}</H3>
          <ul className="space-y-1">
            <Li>{t("privacyPage.s2_titles")}</Li>
            <Li>{t("privacyPage.s2_history")}</Li>
            <Li>{t("privacyPage.s2_payment")}</Li>
            <Li>{t("privacyPage.s2_address")}</Li>
            <Li>{t("privacyPage.s2_tracking")}</Li>
            <Li>{t("privacyPage.s2_wallet")}</Li>
          </ul>

          <H3>{t("privacyPage.s2_h_comms")}</H3>
          <ul className="space-y-1">
            <Li>{t("privacyPage.s2_chat")}</Li>
            <Li>{t("privacyPage.s2_feedback")}</Li>
            <Li>{t("privacyPage.s2_reviews")}</Li>
          </ul>

          <H3>{t("privacyPage.s2_h_technical")}</H3>
          <ul className="space-y-1">
            <Li>{t("privacyPage.s2_ip")}</Li>
            <Li>{t("privacyPage.s2_browser")}</Li>
            <Li>{t("privacyPage.s2_pages")}</Li>
            <Li>{t("privacyPage.s2_expo")}</Li>
            <Li>{t("privacyPage.s2_audit")}</Li>
          </ul>
        </Section>

        <Section num={3} id="how-we-use" title={t("privacyPage.toc_3")}>
          <ul className="space-y-1.5">
            <Li>{t("privacyPage.s3_li_1")}</Li>
            <Li>{t("privacyPage.s3_li_2")}</Li>
            <Li>{t("privacyPage.s3_li_3")}</Li>
            <Li>{t("privacyPage.s3_li_4")}</Li>
            <Li>{t("privacyPage.s3_li_5")}</Li>
            <Li>{t("privacyPage.s3_li_6")}</Li>
            <Li>{t("privacyPage.s3_li_7")}</Li>
            <Li>{t("privacyPage.s3_li_8")}</Li>
            <Li>{t("privacyPage.s3_li_9")}</Li>
            <Li>{t("privacyPage.s3_li_10")}</Li>
          </ul>
          <div className="mt-3 rounded-lg border border-amber-200 dark:border-amber-800/40 bg-amber-50 dark:bg-amber-900/10 p-4 text-sm text-amber-800 dark:text-amber-300">
            <strong>{t("privacyPage.s3_ai_label")}</strong> {t("privacyPage.s3_ai_text")}
          </div>
        </Section>

        <Section num={4} id="legal-basis" title={t("privacyPage.toc_4")}>
          <Table
            headers={[t("privacyPage.s4_th_purpose"), t("privacyPage.s4_th_basis"), t("privacyPage.s4_th_article")]}
            rows={legalBasisRows}
          />
        </Section>

        <Section num={5} id="processors" title={t("privacyPage.toc_5")}>
          <p>{t("privacyPage.s5_p1")}</p>
          <Table
            headers={[t("privacyPage.s5_th_processor"), t("privacyPage.s5_th_purpose"), t("privacyPage.s5_th_data")]}
            rows={processors}
          />
          <p>{t("privacyPage.s5_no_sell")}</p>
        </Section>

        <Section num={6} id="retention" title={t("privacyPage.toc_6")}>
          <ul className="space-y-1.5">
            <Li><strong>{t("privacyPage.s6_account_label")}</strong> — {t("privacyPage.s6_account_text")}</Li>
            <Li><strong>{t("privacyPage.s6_payment_label")}</strong> — {t("privacyPage.s6_payment_text")}</Li>
            <Li><strong>{t("privacyPage.s6_chat_label")}</strong> — {t("privacyPage.s6_chat_text")}</Li>
            <Li><strong>{t("privacyPage.s6_audit_label")}</strong> — {t("privacyPage.s6_audit_text")}</Li>
            <Li><strong>{t("privacyPage.s6_listings_label")}</strong> — {t("privacyPage.s6_listings_text")}</Li>
          </ul>
        </Section>

        <Section num={7} id="your-rights" title={t("privacyPage.toc_7")}>
          <p>
            {t("privacyPage.s7_p1_pre")}{" "}
            <a href="mailto:support@mamvibe.com" className="text-primary hover:underline">
              support@mamvibe.com
            </a>
            . {t("privacyPage.s7_p1_post")}
          </p>
          <ul className="space-y-1.5 mt-1">
            <Li><strong>{t("privacyPage.s7_access_label")}</strong> — {t("privacyPage.s7_access_text")}</Li>
            <Li><strong>{t("privacyPage.s7_rect_label")}</strong> — {t("privacyPage.s7_rect_text")}</Li>
            <Li><strong>{t("privacyPage.s7_erasure_label")}</strong> — {t("privacyPage.s7_erasure_text")}</Li>
            <Li><strong>{t("privacyPage.s7_portability_label")}</strong> — {t("privacyPage.s7_portability_text")}</Li>
            <Li><strong>{t("privacyPage.s7_restriction_label")}</strong> — {t("privacyPage.s7_restriction_text")}</Li>
            <Li><strong>{t("privacyPage.s7_objection_label")}</strong> — {t("privacyPage.s7_objection_text")}</Li>
            <Li><strong>{t("privacyPage.s7_withdraw_label")}</strong> — {t("privacyPage.s7_withdraw_text")}</Li>
          </ul>
          <p>
            {t("privacyPage.s7_dpa_pre")}{" "}
            <strong>{t("privacyPage.s7_dpa_name")}</strong>:{" "}
            <span className="font-mono text-sm">cpdp.bg</span>, {t("privacyPage.s7_dpa_post")}
          </p>
        </Section>

        <Section num={8} id="cookies" title={t("privacyPage.toc_8")}>
          <p>
            {t("privacyPage.s8_see_pre")}{" "}
            <Link to="/cookies" className="text-primary hover:underline font-medium">
              {t("legalPages.nav_cookies")}
            </Link>
            . {t("privacyPage.s8_see_post")}
          </p>
          <H3>{t("privacyPage.s8_h_our_cookie")}</H3>
          <div className="rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 p-4 font-mono text-sm">
            <p>
              <span className="text-primary font-semibold">refreshToken</span>
              {" "}— HttpOnly · Secure · SameSite=Strict · Path=/api/auth · 7-day expiry
            </p>
            <p className="font-sans text-xs text-gray-400 dark:text-gray-500 mt-1.5">
              {t("privacyPage.s8_cookie_note")}
            </p>
          </div>
          <H3>{t("privacyPage.s8_h_local")}</H3>
          <ul className="space-y-1">
            <Li>{t("privacyPage.s8_local_1")}</Li>
            <Li>{t("privacyPage.s8_local_2")}</Li>
            <Li>{t("privacyPage.s8_local_3")}</Li>
          </ul>
          <p>{t("privacyPage.s8_no_analytics")}</p>
        </Section>

        <Section num={9} id="security" title={t("privacyPage.toc_9")}>
          <p>{t("privacyPage.s9_intro")}</p>
          <ul className="space-y-1">
            <Li>{t("privacyPage.s9_tls")}</Li>
            <Li>{t("privacyPage.s9_hash")}</Li>
            <Li>{t("privacyPage.s9_tokens")}</Li>
            <Li>{t("privacyPage.s9_stripe")}</Li>
            <Li>{t("privacyPage.s9_owasp")}</Li>
            <Li>{t("privacyPage.s9_rate")}</Li>
            <Li>{t("privacyPage.s9_turnstile")}</Li>
          </ul>
          <p>
            {t("privacyPage.s9_disclose_pre")}{" "}
            <a href="mailto:support@mamvibe.com" className="text-primary hover:underline">
              support@mamvibe.com
            </a>
            .
          </p>
        </Section>

        <Section num={10} id="changes" title={t("privacyPage.toc_10")}>
          <p>{t("privacyPage.s10_p1")}</p>
          <p>{t("privacyPage.s10_p2")}</p>
        </Section>

        <Section num={11} id="contact" title={t("privacyPage.toc_11")}>
          <p>{t("privacyPage.s11_intro")}</p>
          <div className="rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 p-4 inline-block">
            <p className="font-semibold text-gray-800 dark:text-gray-100">{t("privacyPage.s11_company")}</p>
            <a href="mailto:support@mamvibe.com" className="text-primary hover:underline text-sm">
              support@mamvibe.com
            </a>
          </div>
        </Section>

      </div>
    </div>
  );
}
