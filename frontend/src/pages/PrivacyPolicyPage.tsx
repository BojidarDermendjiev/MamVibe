import { type ReactNode } from "react";
import { Link } from "react-router-dom";
import { ShieldCheck } from "lucide-react";
import { usePageSEO } from "@/hooks/useSEO";

const LAST_UPDATED = "19 May 2026";

const TOC = [
  { id: "controller",   label: "1. Data Controller" },
  { id: "what-we-collect", label: "2. Data We Collect" },
  { id: "how-we-use",   label: "3. How We Use Your Data" },
  { id: "legal-basis",  label: "4. Legal Basis (GDPR)" },
  { id: "processors",   label: "5. Data Processors & Sharing" },
  { id: "retention",    label: "6. Retention Periods" },
  { id: "your-rights",  label: "7. Your Rights" },
  { id: "cookies",      label: "8. Cookies & Local Storage" },
  { id: "security",     label: "9. Security" },
  { id: "changes",      label: "10. Changes to This Policy" },
  { id: "contact",      label: "11. Contact" },
];

function Section({ id, title, children }: { id: string; title: string; children: ReactNode }) {
  return (
    <section id={id} className="mb-12 scroll-mt-24">
      <h2 className="text-xl font-bold text-gray-800 dark:text-gray-100 mb-4 pb-3 border-b border-gray-200 dark:border-gray-700">
        {title}
      </h2>
      <div className="space-y-3 text-gray-600 dark:text-gray-300 text-[15px] leading-relaxed">
        {children}
      </div>
    </section>
  );
}

function H3({ children }: { children: ReactNode }) {
  return <h3 className="font-semibold text-gray-800 dark:text-gray-100 mt-5 mb-2">{children}</h3>;
}

function Li({ children }: { children: ReactNode }) {
  return (
    <li className="flex gap-2">
      <span className="text-primary mt-1 shrink-0">•</span>
      <span>{children}</span>
    </li>
  );
}

export default function PrivacyPolicyPage() {
  usePageSEO({
    title: "Privacy Policy — MamVibe",
    description:
      "Learn how MamVibe collects, uses, and protects your personal data in compliance with the GDPR and Bulgarian data protection law.",
    canonical: "https://mamvibe.com/privacy",
    index: true,
  });

  return (
    <div className="min-h-screen bg-white dark:bg-[#1a1825]">
      {/* Hero */}
      <div className="bg-gradient-to-br from-[#945c67] to-[#3f4b7f] py-16 px-4">
        <div className="max-w-4xl mx-auto text-white">
          <div className="flex items-center gap-3 mb-4">
            <ShieldCheck size={32} className="opacity-90" />
            <h1 className="text-3xl font-bold">Privacy Policy</h1>
          </div>
          <p className="text-white/80 text-sm">
            Effective date: <strong className="text-white">{LAST_UPDATED}</strong>
          </p>
          <p className="text-white/70 text-sm mt-1">
            This policy applies to mamvibe.com and all MamVibe services.
          </p>
        </div>
      </div>

      <div className="max-w-4xl mx-auto px-4 py-12 grid grid-cols-1 lg:grid-cols-[220px_1fr] gap-10">
        {/* Table of Contents — sticky sidebar on desktop */}
        <aside className="hidden lg:block">
          <div className="sticky top-24 bg-gray-50 dark:bg-[#2d2a42] rounded-xl p-5 border border-gray-200 dark:border-gray-700">
            <p className="text-xs font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-3">
              Contents
            </p>
            <ul className="space-y-1.5">
              {TOC.map((item) => (
                <li key={item.id}>
                  <a
                    href={`#${item.id}`}
                    className="text-sm text-gray-600 dark:text-gray-400 hover:text-primary dark:hover:text-purple-300 transition-colors block"
                  >
                    {item.label}
                  </a>
                </li>
              ))}
            </ul>
          </div>
        </aside>

        {/* Content */}
        <article>
          <Section id="controller" title="1. Data Controller">
            <p>
              MamVibe (<strong>we</strong>, <strong>us</strong>, <strong>our</strong>) is the controller
              of personal data collected through this platform. You can reach us at{" "}
              <a href="mailto:support@mamvibe.com" className="text-primary hover:underline">
                support@mamvibe.com
              </a>
              .
            </p>
            <p>
              MamVibe is a Bulgarian second-hand baby and children's marketplace operating under the
              laws of the Republic of Bulgaria and the European Union's General Data Protection
              Regulation (GDPR — Regulation (EU) 2016/679).
            </p>
          </Section>

          <Section id="what-we-collect" title="2. Data We Collect">
            <H3>Account & Profile</H3>
            <ul className="space-y-1 ml-1">
              <Li>Full name / display name</Li>
              <Li>Email address</Li>
              <Li>Profile photo (avatar)</Li>
              <Li>Profile type (Mom / Dad / Family)</Li>
              <Li>Bio (optional, user-provided)</Li>
              <Li>Google account ID (if you register via Google OAuth)</Li>
            </ul>

            <H3>Listings & Transactions</H3>
            <ul className="space-y-1 ml-1">
              <Li>Item titles, descriptions, photos, prices, and condition</Li>
              <Li>Purchase and sale history</Li>
              <Li>Payment records (amount, status, Stripe session ID — we do not store card numbers)</Li>
              <Li>Shipping addresses provided during checkout</Li>
              <Li>Shipment tracking numbers and courier provider</Li>
              <Li>Wallet balance and transaction history</Li>
            </ul>

            <H3>Communications</H3>
            <ul className="space-y-1 ml-1">
              <Li>Messages exchanged between users via the in-app chat</Li>
              <Li>Feedback and star ratings submitted by you</Li>
              <Li>Doctor reviews and child-friendly place entries you create</Li>
            </ul>

            <H3>Technical & Usage Data</H3>
            <ul className="space-y-1 ml-1">
              <Li>IP address (used for security and abuse prevention)</Li>
              <Li>Browser type and operating system</Li>
              <Li>Pages visited, actions taken, timestamps</Li>
              <Li>Expo push notification token (mobile app users)</Li>
              <Li>Security audit log entries (login events, admin actions)</Li>
            </ul>
          </Section>

          <Section id="how-we-use" title="3. How We Use Your Data">
            <ul className="space-y-2 ml-1">
              <Li>To create and manage your account</Li>
              <Li>To display your listings to buyers and process purchases</Li>
              <Li>To facilitate real-time chat between buyers and sellers</Li>
              <Li>To process payments and manage the escrow wallet</Li>
              <Li>To generate and send e-bill receipts and fiscal receipts</Li>
              <Li>To coordinate shipping with Econt, Speedy, or Box Now</Li>
              <Li>To send transactional emails (password reset, order confirmation, shipping updates)</Li>
              <Li>To moderate listings using AI-assisted content review</Li>
              <Li>To detect fraud, abuse, and policy violations</Li>
              <Li>To improve the platform and understand how users interact with it</Li>
            </ul>
            <p className="text-sm bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-700/40 rounded-lg p-4 mt-4">
              <strong>AI features:</strong> When you use the AI listing assistant or AI moderation,
              your item photos and text may be sent to Anthropic's Claude API for processing. No data
              is stored by Anthropic beyond the immediate API call.
            </p>
          </Section>

          <Section id="legal-basis" title="4. Legal Basis (GDPR)">
            <div className="overflow-x-auto">
              <table className="w-full text-sm border border-gray-200 dark:border-gray-700 rounded-lg overflow-hidden">
                <thead className="bg-gray-100 dark:bg-[#2d2a42] text-gray-700 dark:text-gray-200">
                  <tr>
                    <th className="text-left px-4 py-3">Purpose</th>
                    <th className="text-left px-4 py-3">Legal Basis</th>
                    <th className="text-left px-4 py-3">GDPR Article</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
                  {[
                    ["Account management, transactions, shipping", "Performance of contract", "Art. 6(1)(b)"],
                    ["Security, fraud prevention, abuse detection", "Legitimate interests", "Art. 6(1)(f)"],
                    ["Transactional emails (orders, shipping)", "Performance of contract", "Art. 6(1)(b)"],
                    ["AI-assisted listing suggestions", "Legitimate interests", "Art. 6(1)(f)"],
                    ["Marketing / promotional emails", "Consent", "Art. 6(1)(a)"],
                    ["Legal, accounting, and tax obligations", "Legal obligation", "Art. 6(1)(c)"],
                  ].map(([purpose, basis, article]) => (
                    <tr key={purpose} className="hover:bg-gray-50 dark:hover:bg-white/5">
                      <td className="px-4 py-3">{purpose}</td>
                      <td className="px-4 py-3 font-medium text-gray-700 dark:text-gray-200">{basis}</td>
                      <td className="px-4 py-3 font-mono text-xs text-primary">{article}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </Section>

          <Section id="processors" title="5. Data Processors & Sharing">
            <p>
              We engage trusted third-party processors to operate the platform. Each processor is
              bound by a data processing agreement and processes data only on our documented
              instructions.
            </p>
            <div className="overflow-x-auto mt-4">
              <table className="w-full text-sm border border-gray-200 dark:border-gray-700 rounded-lg overflow-hidden">
                <thead className="bg-gray-100 dark:bg-[#2d2a42] text-gray-700 dark:text-gray-200">
                  <tr>
                    <th className="text-left px-4 py-3">Processor</th>
                    <th className="text-left px-4 py-3">Purpose</th>
                    <th className="text-left px-4 py-3">Data Transferred</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
                  {[
                    ["Stripe", "Card payment processing", "Name, email, purchase amount"],
                    ["Econt Express / Speedy / Box Now", "Parcel shipping & tracking", "Sender & recipient name, address, phone"],
                    ["TakeANap", "Bulgarian fiscal receipt issuance", "Name, email, transaction amount"],
                    ["Google", "OAuth sign-in, reCAPTCHA signals", "Email, Google account ID"],
                    ["Cloudflare", "Bot protection (Turnstile)", "IP address, browser fingerprint"],
                    ["Anthropic", "AI listing assistance & moderation", "Item text and photos (not stored)"],
                    ["SMTP provider", "Transactional email delivery", "Email address, message content"],
                    ["n8n (self-hosted)", "Workflow automation & notifications", "Email, order details"],
                  ].map(([processor, purpose, data]) => (
                    <tr key={processor} className="hover:bg-gray-50 dark:hover:bg-white/5">
                      <td className="px-4 py-3 font-medium text-gray-700 dark:text-gray-200">{processor}</td>
                      <td className="px-4 py-3">{purpose}</td>
                      <td className="px-4 py-3 text-gray-500 dark:text-gray-400">{data}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <p className="mt-4">
              We do <strong>not</strong> sell your personal data to third parties, and we do not share
              it for advertising or profiling purposes.
            </p>
          </Section>

          <Section id="retention" title="6. Retention Periods">
            <ul className="space-y-2 ml-1">
              <Li>
                <strong>Account data</strong> — retained while your account is active. Deleted within
                30 days of account closure on request.
              </Li>
              <Li>
                <strong>Payment records and e-bills</strong> — retained for 5 years to comply with
                Bulgarian accounting and tax law (Закон за счетоводството, Art. 12).
              </Li>
              <Li>
                <strong>Chat messages</strong> — retained until you delete the conversation or your
                account is closed.
              </Li>
              <Li>
                <strong>Security audit logs</strong> — retained for 31 days, then automatically
                deleted by the Loki log compactor.
              </Li>
              <Li>
                <strong>Listings (soft-deleted)</strong> — marked as deleted immediately but removed
                from storage within 90 days.
              </Li>
            </ul>
          </Section>

          <Section id="your-rights" title="7. Your Rights">
            <p>
              Under the GDPR you have the following rights regarding your personal data. To exercise
              any of them, email{" "}
              <a href="mailto:support@mamvibe.com" className="text-primary hover:underline">
                support@mamvibe.com
              </a>
              . We will respond within 30 days.
            </p>
            <ul className="space-y-2 ml-1 mt-3">
              <Li>
                <strong>Access (Art. 15)</strong> — request a copy of all data we hold about you.
              </Li>
              <Li>
                <strong>Rectification (Art. 16)</strong> — correct inaccurate or incomplete data.
              </Li>
              <Li>
                <strong>Erasure (Art. 17)</strong> — request deletion of your data ("right to be
                forgotten"), subject to legal retention obligations.
              </Li>
              <Li>
                <strong>Portability (Art. 20)</strong> — receive your data in a structured,
                machine-readable format.
              </Li>
              <Li>
                <strong>Restriction (Art. 18)</strong> — ask us to pause processing while a dispute
                is resolved.
              </Li>
              <Li>
                <strong>Objection (Art. 21)</strong> — object to processing based on legitimate
                interests.
              </Li>
              <Li>
                <strong>Withdraw consent</strong> — where processing is based on consent, you may
                withdraw it at any time without affecting prior processing.
              </Li>
            </ul>
            <p className="mt-4">
              You also have the right to lodge a complaint with the{" "}
              <strong>Commission for Personal Data Protection (КЗЛД)</strong>:{" "}
              <span className="font-mono text-sm">cpdp.bg</span>, 2 Prof. Tsvetan Lazarov Blvd,
              Sofia 1592, Bulgaria.
            </p>
          </Section>

          <Section id="cookies" title="8. Cookies & Local Storage">
            <p>
              For full details see our{" "}
              <Link to="/cookies" className="text-primary hover:underline font-medium">
                Cookie Policy
              </Link>
              . In summary:
            </p>
            <H3>The one cookie we set</H3>
            <div className="bg-gray-50 dark:bg-[#2d2a42] rounded-lg p-4 font-mono text-sm border border-gray-200 dark:border-gray-700">
              <p>
                <span className="text-primary font-semibold">refreshToken</span> — HttpOnly ·
                Secure · SameSite=Strict · Path=/api/auth · 7-day expiry
              </p>
              <p className="text-gray-500 dark:text-gray-400 text-xs mt-1 font-sans">
                This is an essential authentication cookie. It cannot be read by JavaScript and is
                exempt from cookie consent requirements.
              </p>
            </div>
            <H3>Local Storage (not cookies)</H3>
            <ul className="space-y-1 ml-1">
              <Li>Your public profile data (name, avatar) — to avoid a network round-trip on page load</Li>
              <Li>Language preference (en / bg)</Li>
              <Li>Your cookie consent decision</Li>
            </ul>
            <p className="mt-3">
              We do <strong>not</strong> use analytics cookies, advertising cookies, or any
              third-party tracking scripts.
            </p>
          </Section>

          <Section id="security" title="9. Security">
            <p>
              We implement appropriate technical and organisational measures to protect your data:
            </p>
            <ul className="space-y-1 ml-1 mt-2">
              <Li>All data in transit is encrypted via TLS 1.2+</Li>
              <Li>Passwords are hashed using PBKDF2 via ASP.NET Core Identity</Li>
              <Li>Authentication tokens are short-lived (15-minute access tokens)</Li>
              <Li>Payment card data is never stored — Stripe handles all card processing</Li>
              <Li>The platform is tested against the OWASP Top 10</Li>
              <Li>Production servers have rate limiting, IP-based abuse controls, and audit logging</Li>
            </ul>
            <p className="mt-3">
              If you discover a security vulnerability, please disclose it responsibly to{" "}
              <a href="mailto:support@mamvibe.com" className="text-primary hover:underline">
                support@mamvibe.com
              </a>
              .
            </p>
          </Section>

          <Section id="changes" title="10. Changes to This Policy">
            <p>
              We may update this Privacy Policy from time to time. Material changes will be announced
              via email or an in-app notification at least 14 days before they take effect. The
              "Effective date" at the top of this page always reflects the latest version.
            </p>
            <p>
              Continued use of MamVibe after the effective date constitutes acceptance of the updated
              policy.
            </p>
          </Section>

          <Section id="contact" title="11. Contact">
            <p>For any privacy-related questions or to exercise your rights:</p>
            <div className="bg-gray-50 dark:bg-[#2d2a42] rounded-xl p-5 mt-3 border border-gray-200 dark:border-gray-700">
              <p className="font-semibold text-gray-800 dark:text-gray-100">MamVibe Support</p>
              <a
                href="mailto:support@mamvibe.com"
                className="text-primary hover:underline text-sm"
              >
                support@mamvibe.com
              </a>
            </div>
          </Section>

          <div className="flex gap-4 text-sm text-gray-400 dark:text-gray-500 pt-4 border-t border-gray-200 dark:border-gray-700">
            <Link to="/terms" className="hover:text-primary transition-colors">Terms & Conditions</Link>
            <span>·</span>
            <Link to="/cookies" className="hover:text-primary transition-colors">Cookie Policy</Link>
          </div>
        </article>
      </div>
    </div>
  );
}
