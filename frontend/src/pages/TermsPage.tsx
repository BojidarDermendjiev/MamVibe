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

function Warning({ children }: { children: ReactNode }) {
  return (
    <div className="rounded-lg border border-amber-200 dark:border-amber-800/40 bg-amber-50 dark:bg-amber-900/10 p-4 text-sm text-amber-800 dark:text-amber-300">
      {children}
    </div>
  );
}

export default function TermsPage() {
  const { t } = useTranslation();

  usePageSEO({
    title: "Terms & Conditions — MamVibe",
    description:
      "Read the MamVibe Terms & Conditions governing the use of our Bulgarian second-hand baby and children's marketplace.",
    canonical: "https://mamvibe.com/terms",
    index: true,
  });

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-[#0e0c1a] py-14 px-4">
      <div className="max-w-3xl mx-auto">

        {/* Page header */}
        <header className="mb-12">
          <h1 className="text-4xl font-bold text-gray-900 dark:text-white mb-1">
            {t("termsPage.title")}
          </h1>
          <p className="text-sm text-gray-400 dark:text-gray-500">
            {t("legalPages.last_updated")} {LAST_UPDATED}
          </p>
          <hr className="mt-6 border-gray-200 dark:border-gray-700" />
        </header>

        {/* Intro */}
        <p className="text-[15px] text-gray-600 dark:text-gray-300 leading-relaxed mb-14">
          {t("termsPage.hero_subtitle")}
        </p>

        {/* Sections */}
        <Section num={1} id="acceptance" title={t("termsPage.toc_1")}>
          <p>
            {t("termsPage.s1_p1_pre")}
            <strong>{t("termsPage.s1_terms_bold")}</strong>
            {t("termsPage.s1_p1_mid")}{" "}
            <Link to="/privacy" className="text-primary hover:underline">
              {t("legalPages.nav_privacy")}
            </Link>
            {t("termsPage.s1_p1_post")}
          </p>
          <p>{t("termsPage.s1_p2")}</p>
        </Section>

        <Section num={2} id="eligibility" title={t("termsPage.toc_2")}>
          <ul className="space-y-1.5">
            <Li>{t("termsPage.s2_li_1")}</Li>
            <Li>{t("termsPage.s2_li_2")}</Li>
            <Li>{t("termsPage.s2_li_3")}</Li>
            <Li>{t("termsPage.s2_li_4")}</Li>
          </ul>
        </Section>

        <Section num={3} id="accounts" title={t("termsPage.toc_3")}>
          <H3>{t("termsPage.s3_h_registration")}</H3>
          <p>{t("termsPage.s3_registration_p")}</p>

          <H3>{t("termsPage.s3_h_credentials")}</H3>
          <ul className="space-y-1">
            <Li>{t("termsPage.s3_cred_li_1")}</Li>
            <Li>
              {t("termsPage.s3_cred_li_2_pre")}{" "}
              <a href="mailto:support@mamvibe.com" className="text-primary hover:underline">
                support@mamvibe.com
              </a>
              .
            </Li>
            <Li>{t("termsPage.s3_cred_li_3")}</Li>
          </ul>

          <H3>{t("termsPage.s3_h_termination")}</H3>
          <p>
            {t("termsPage.s3_term_pre")}{" "}
            <Link to="/privacy" className="text-primary hover:underline">
              {t("legalPages.nav_privacy")}
            </Link>
            {t("termsPage.s3_term_post")}
          </p>
        </Section>

        <Section num={4} id="listings" title={t("termsPage.toc_4")}>
          <H3>{t("termsPage.s4_h_accuracy")}</H3>
          <p>{t("termsPage.s4_accuracy_p")}</p>

          <H3>{t("termsPage.s4_h_condition")}</H3>
          <p>
            {t("termsPage.s4_condition_pre")}{" "}
            <em>{t("termsPage.s4_condition_new")}</em>,{" "}
            <em>{t("termsPage.s4_condition_like_new")}</em>,{" "}
            <em>{t("termsPage.s4_condition_good")}</em>,{" "}
            <em>{t("termsPage.s4_condition_fair")}</em>,{" or "}
            <em>{t("termsPage.s4_condition_parts")}</em>.{" "}
            {t("termsPage.s4_condition_post")}
          </p>

          <H3>{t("termsPage.s4_h_type")}</H3>
          <ul className="space-y-1">
            <Li><strong>{t("termsPage.s4_sell_label")}</strong> — {t("termsPage.s4_sell_text")}</Li>
            <Li><strong>{t("termsPage.s4_donate_label")}</strong> — {t("termsPage.s4_donate_text")}</Li>
          </ul>

          <H3>{t("termsPage.s4_h_moderation")}</H3>
          <p>{t("termsPage.s4_moderation_p")}</p>

          <Warning>{t("termsPage.s4_warning")}</Warning>
        </Section>

        <Section num={5} id="buying" title={t("termsPage.toc_5")}>
          <ul className="space-y-1.5">
            <Li>{t("termsPage.s5_li_1")}</Li>
            <Li>{t("termsPage.s5_li_2")}</Li>
            <Li>{t("termsPage.s5_li_3")}</Li>
          </ul>
        </Section>

        <Section num={6} id="payments" title={t("termsPage.toc_6")}>
          <H3>{t("termsPage.s6_h_payment")}</H3>
          <p>{t("termsPage.s6_payment_p")}</p>

          <H3>{t("termsPage.s6_h_escrow")}</H3>
          <p>{t("termsPage.s6_escrow_intro")}</p>
          <ol className="space-y-1 list-none">
            <li className="flex gap-2"><span className="text-primary shrink-0">①</span><span>{t("termsPage.s6_escrow_1")}</span></li>
            <li className="flex gap-2"><span className="text-primary shrink-0">②</span><span>{t("termsPage.s6_escrow_2")}</span></li>
            <li className="flex gap-2"><span className="text-primary shrink-0">③</span><span>{t("termsPage.s6_escrow_3")}</span></li>
            <li className="flex gap-2"><span className="text-primary shrink-0">④</span><span>{t("termsPage.s6_escrow_4")}</span></li>
          </ol>

          <H3>{t("termsPage.s6_h_fees")}</H3>
          <p>{t("termsPage.s6_fees_p")}</p>

          <H3>{t("termsPage.s6_h_ebills")}</H3>
          <p>{t("termsPage.s6_ebills_p")}</p>

          <Warning>{t("termsPage.s6_warning")}</Warning>
        </Section>

        <Section num={7} id="shipping" title={t("termsPage.toc_7")}>
          <p>{t("termsPage.s7_p1")}</p>
          <ul className="space-y-1.5">
            <Li>{t("termsPage.s7_li_1")}</Li>
            <Li>{t("termsPage.s7_li_2")}</Li>
            <Li>{t("termsPage.s7_li_3")}</Li>
            <Li>{t("termsPage.s7_li_4")}</Li>
          </ul>
        </Section>

        <Section num={8} id="prohibited" title={t("termsPage.toc_8")}>
          <H3>{t("termsPage.s8_h_items")}</H3>
          <ul className="space-y-1">
            <Li>{t("termsPage.s8_item_1")}</Li>
            <Li>{t("termsPage.s8_item_2")}</Li>
            <Li>{t("termsPage.s8_item_3")}</Li>
            <Li>{t("termsPage.s8_item_4")}</Li>
            <Li>{t("termsPage.s8_item_5")}</Li>
            <Li>{t("termsPage.s8_item_6")}</Li>
            <Li>{t("termsPage.s8_item_7")}</Li>
            <Li>{t("termsPage.s8_item_8")}</Li>
          </ul>

          <H3>{t("termsPage.s8_h_conduct")}</H3>
          <ul className="space-y-1">
            <Li>{t("termsPage.s8_conduct_1")}</Li>
            <Li>{t("termsPage.s8_conduct_2")}</Li>
            <Li>{t("termsPage.s8_conduct_3")}</Li>
            <Li>{t("termsPage.s8_conduct_4")}</Li>
            <Li>{t("termsPage.s8_conduct_5")}</Li>
          </ul>
        </Section>

        <Section num={9} id="ai" title={t("termsPage.toc_9")}>
          <p>{t("termsPage.s9_p1")}</p>
          <ul className="space-y-1">
            <Li>{t("termsPage.s9_li_1")}</Li>
            <Li>{t("termsPage.s9_li_2")}</Li>
            <Li>{t("termsPage.s9_li_3")}</Li>
          </ul>
        </Section>

        <Section num={10} id="ip" title={t("termsPage.toc_10")}>
          <p>{t("termsPage.s10_p1")}</p>
          <p>{t("termsPage.s10_p2")}</p>
        </Section>

        <Section num={11} id="liability" title={t("termsPage.toc_11")}>
          <p>{t("termsPage.s11_intro")}</p>
          <ul className="space-y-1">
            <Li>{t("termsPage.s11_li_1")}</Li>
            <Li>{t("termsPage.s11_li_2")}</Li>
            <Li>{t("termsPage.s11_li_3")}</Li>
          </ul>
          <p>{t("termsPage.s11_footer")}</p>
        </Section>

        <Section num={12} id="suspension" title={t("termsPage.toc_12")}>
          <p>{t("termsPage.s12_intro")}</p>
          <ul className="space-y-1">
            <Li>{t("termsPage.s12_li_1")}</Li>
            <Li>{t("termsPage.s12_li_2")}</Li>
            <Li>{t("termsPage.s12_li_3")}</Li>
            <Li>{t("termsPage.s12_li_4")}</Li>
            <Li>{t("termsPage.s12_li_5")}</Li>
          </ul>
          <p>
            {t("termsPage.s12_footer_pre")}{" "}
            <a href="mailto:support@mamvibe.com" className="text-primary hover:underline">
              support@mamvibe.com
            </a>
            .
          </p>
        </Section>

        <Section num={13} id="governing" title={t("termsPage.toc_13")}>
          <p>
            {t("termsPage.s13_p1_pre")}{" "}
            <strong>{t("termsPage.s13_bulgaria")}</strong>
            {t("termsPage.s13_p1_mid")}{" "}
            <strong>{t("termsPage.s13_sofia")}</strong>
            {t("termsPage.s13_p1_post")}
          </p>
          <p>{t("termsPage.s13_p2")}</p>
        </Section>

        <Section num={14} id="changes" title={t("termsPage.toc_14")}>
          <p>{t("termsPage.s14_p1")}</p>
          <p>{t("termsPage.s14_p2")}</p>
        </Section>

      </div>
    </div>
  );
}
