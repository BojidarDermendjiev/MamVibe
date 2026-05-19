import { type ReactNode } from "react";
import { Link } from "react-router-dom";
import { Cookie } from "lucide-react";
import { usePageSEO } from "@/hooks/useSEO";

const LAST_UPDATED = "19 May 2026";

const TOC = [
  { id: "what-are-cookies", label: "1. What Are Cookies?" },
  { id: "our-cookie",       label: "2. The Cookie We Set" },
  { id: "local-storage",   label: "3. Local Storage" },
  { id: "third-party",     label: "4. Third-Party Cookies" },
  { id: "managing",        label: "5. Managing Cookies" },
  { id: "more-info",       label: "6. More Information" },
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

function Li({ children }: { children: ReactNode }) {
  return (
    <li className="flex gap-2">
      <span className="text-primary mt-1 shrink-0">•</span>
      <span>{children}</span>
    </li>
  );
}

function Tag({ children, color = "gray" }: { children: ReactNode; color?: "green" | "gray" | "blue" }) {
  const colors = {
    green: "bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-400 border-green-200 dark:border-green-700/40",
    gray:  "bg-gray-100 dark:bg-gray-700/50 text-gray-600 dark:text-gray-300 border-gray-200 dark:border-gray-600",
    blue:  "bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-400 border-blue-200 dark:border-blue-700/40",
  };
  return (
    <span className={`inline-block text-xs font-semibold px-2 py-0.5 rounded-full border ${colors[color]}`}>
      {children}
    </span>
  );
}

export default function CookiePolicyPage() {
  usePageSEO({
    title: "Cookie Policy — MamVibe",
    description:
      "MamVibe uses only one essential cookie for authentication. Read our transparent cookie policy to understand exactly what we store and why.",
    canonical: "https://mamvibe.com/cookies",
    index: true,
  });

  return (
    <div className="min-h-screen bg-white dark:bg-[#1a1825]">
      {/* Hero */}
      <div className="bg-gradient-to-br from-[#945c67] via-[#6b4b7f] to-[#3f4b7f] py-16 px-4">
        <div className="max-w-4xl mx-auto text-white">
          <div className="flex items-center gap-3 mb-4">
            <Cookie size={32} className="opacity-90" />
            <h1 className="text-3xl font-bold">Cookie Policy</h1>
          </div>
          <p className="text-white/80 text-sm">
            Last updated: <strong className="text-white">{LAST_UPDATED}</strong>
          </p>
          <p className="text-white/70 text-sm mt-2 max-w-xl">
            Short version: MamVibe uses <strong className="text-white">exactly one cookie</strong>
            {" "}— an essential authentication token. No tracking, no analytics, no advertising cookies.
          </p>
        </div>
      </div>

      <div className="max-w-4xl mx-auto px-4 py-12 grid grid-cols-1 lg:grid-cols-[220px_1fr] gap-10">
        {/* Table of Contents */}
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

            {/* Quick badge */}
            <div className="mt-6 p-3 bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-700/40 rounded-lg">
              <p className="text-xs font-semibold text-green-700 dark:text-green-400">Cookie score</p>
              <p className="text-2xl font-bold text-green-700 dark:text-green-400 mt-0.5">1 / 1</p>
              <p className="text-xs text-green-600 dark:text-green-500 mt-0.5">Essential only</p>
            </div>
          </div>
        </aside>

        {/* Content */}
        <article>
          <Section id="what-are-cookies" title="1. What Are Cookies?">
            <p>
              Cookies are small text files placed on your device by a website. They allow the site
              to remember information about your visit — for example, whether you are logged in.
            </p>
            <p>
              Cookies are categorised by purpose:
            </p>
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-3 mt-2">
              {[
                {
                  label: "Strictly Necessary",
                  color: "green" as const,
                  desc: "Required for the site to function. Cannot be disabled without breaking core features.",
                },
                {
                  label: "Analytics / Performance",
                  color: "blue" as const,
                  desc: "Collect aggregated data about how visitors use the site.",
                },
                {
                  label: "Advertising / Tracking",
                  color: "gray" as const,
                  desc: "Build profiles to serve targeted ads across sites.",
                },
              ].map((cat) => (
                <div
                  key={cat.label}
                  className="bg-gray-50 dark:bg-[#2d2a42] border border-gray-200 dark:border-gray-700 rounded-xl p-4"
                >
                  <Tag color={cat.color}>{cat.label}</Tag>
                  <p className="text-sm text-gray-500 dark:text-gray-400 mt-2">{cat.desc}</p>
                </div>
              ))}
            </div>
            <p className="mt-2">
              MamVibe uses only <strong>strictly necessary</strong> cookies. We do not use analytics
              or advertising cookies.
            </p>
          </Section>

          <Section id="our-cookie" title="2. The Cookie We Set">
            <p>
              MamVibe sets exactly one cookie on your browser:
            </p>

            {/* Cookie detail card */}
            <div className="mt-4 border border-gray-200 dark:border-gray-700 rounded-xl overflow-hidden">
              <div className="bg-gray-100 dark:bg-[#2d2a42] px-5 py-3 flex items-center justify-between">
                <span className="font-mono font-semibold text-gray-800 dark:text-gray-100">
                  refreshToken
                </span>
                <Tag color="green">Strictly Necessary</Tag>
              </div>
              <div className="p-5 space-y-3">
                <table className="w-full text-sm">
                  <tbody className="divide-y divide-gray-100 dark:divide-gray-700">
                    {[
                      ["Purpose", "Keeps you logged in across page refreshes without requiring your password again."],
                      ["Set by", "MamVibe backend (api.mamvibe.com)"],
                      ["Duration", "7 days (renewed on each login or token refresh)"],
                      ["HttpOnly", "Yes — JavaScript cannot read this cookie, protecting against XSS attacks"],
                      ["Secure", "Yes (production) — transmitted only over HTTPS"],
                      ["SameSite", "Strict — never sent on cross-site requests, protecting against CSRF"],
                      ["Path", "/api/auth — sent only to authentication endpoints, not to unrelated API calls"],
                      ["Content", "An opaque token string (SHA-256 hash). It contains no personal data."],
                    ].map(([key, value]) => (
                      <tr key={key}>
                        <td className="py-2.5 pr-4 font-medium text-gray-700 dark:text-gray-200 w-1/3 align-top">
                          {key}
                        </td>
                        <td className="py-2.5 text-gray-600 dark:text-gray-400">{value}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>

            <div className="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-700/40 rounded-lg p-4 mt-4 text-sm text-blue-800 dark:text-blue-300">
              <strong>Why is this cookie exempt from consent?</strong> Under the EU ePrivacy
              Directive and GDPR, cookies that are strictly necessary for a service explicitly
              requested by the user — such as maintaining a login session — do not require prior
              consent. You cannot use a logged-in account without this cookie.
            </div>
          </Section>

          <Section id="local-storage" title="3. Local Storage">
            <p>
              In addition to the cookie above, MamVibe uses your browser's{" "}
              <strong>Local Storage</strong> (a different storage mechanism — not a cookie) to
              improve performance and remember your preferences:
            </p>

            <div className="mt-4 border border-gray-200 dark:border-gray-700 rounded-xl overflow-hidden">
              <table className="w-full text-sm">
                <thead className="bg-gray-100 dark:bg-[#2d2a42] text-gray-700 dark:text-gray-200">
                  <tr>
                    <th className="text-left px-4 py-3">Key</th>
                    <th className="text-left px-4 py-3">What It Stores</th>
                    <th className="text-left px-4 py-3">Lifetime</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200 dark:divide-gray-700">
                  <tr className="hover:bg-gray-50 dark:hover:bg-white/5">
                    <td className="px-4 py-3 font-mono text-xs">mamvibe-auth</td>
                    <td className="px-4 py-3">Your public profile (name, avatar, profile type). <em>Never the access token.</em></td>
                    <td className="px-4 py-3">Until logout or account deletion</td>
                  </tr>
                  <tr className="hover:bg-gray-50 dark:hover:bg-white/5">
                    <td className="px-4 py-3 font-mono text-xs">language</td>
                    <td className="px-4 py-3">Your preferred language (en / bg)</td>
                    <td className="px-4 py-3">Until you change it</td>
                  </tr>
                  <tr className="hover:bg-gray-50 dark:hover:bg-white/5">
                    <td className="px-4 py-3 font-mono text-xs">cookieConsent</td>
                    <td className="px-4 py-3">Whether you accepted or rejected the cookie banner</td>
                    <td className="px-4 py-3">Until browser storage is cleared</td>
                  </tr>
                </tbody>
              </table>
            </div>

            <p className="mt-3">
              Local Storage items are not transmitted to our servers with every request (unlike
              cookies). They exist only in your browser and are never shared with third parties.
            </p>
          </Section>

          <Section id="third-party" title="4. Third-Party Cookies">
            <p>
              MamVibe does <strong>not</strong> load any third-party analytics (Google Analytics,
              Mixpanel, Hotjar, etc.) or advertising scripts. We do not use Facebook Pixel, TikTok
              Pixel, or any similar tracking technology.
            </p>
            <p>
              The only external services that may interact with your browser are:
            </p>
            <ul className="space-y-2 ml-1 mt-2">
              <Li>
                <strong>Stripe</strong> — when you open the card payment page, Stripe may set its
                own cookies for fraud prevention. See{" "}
                <span className="font-mono text-xs">stripe.com/privacy</span>.
              </Li>
              <Li>
                <strong>Cloudflare Turnstile</strong> — sets a short-lived challenge cookie on the
                registration page to distinguish humans from bots. No cross-site tracking.
              </Li>
              <Li>
                <strong>Google</strong> — if you use "Sign in with Google", Google OAuth processes
                your authentication. See{" "}
                <span className="font-mono text-xs">policies.google.com</span>.
              </Li>
            </ul>
            <p className="mt-3">
              These services act as independent data controllers for any data they collect. We do
              not control their cookie behaviour.
            </p>
          </Section>

          <Section id="managing" title="5. Managing Cookies">
            <p>
              Because the <code className="bg-gray-100 dark:bg-gray-800 px-1.5 py-0.5 rounded text-sm font-mono">refreshToken</code>{" "}
              cookie is essential to authentication, you cannot disable it while remaining logged
              in. You can, however:
            </p>
            <ul className="space-y-2 ml-1 mt-2">
              <Li>
                <strong>Log out</strong> — this immediately deletes the refreshToken cookie via
                the server and clears your local storage.
              </Li>
              <Li>
                <strong>Clear site data manually</strong> — in your browser's Developer Tools
                (Application → Storage → Clear site data) or via Settings → Privacy → Clear
                browsing data → Cookies.
              </Li>
              <Li>
                <strong>Use incognito / private mode</strong> — cookies and local storage are
                discarded when the private window is closed.
              </Li>
            </ul>

            <div className="mt-4 grid grid-cols-1 sm:grid-cols-2 gap-3">
              {[
                { browser: "Chrome", path: "Settings → Privacy → Cookies → See all site data" },
                { browser: "Firefox", path: "Settings → Privacy & Security → Cookies and Site Data" },
                { browser: "Safari", path: "Preferences → Privacy → Manage Website Data" },
                { browser: "Edge", path: "Settings → Cookies and site permissions → Manage and delete cookies" },
              ].map(({ browser, path }) => (
                <div
                  key={browser}
                  className="bg-gray-50 dark:bg-[#2d2a42] border border-gray-200 dark:border-gray-700 rounded-lg px-4 py-3"
                >
                  <p className="font-semibold text-sm text-gray-800 dark:text-gray-100">{browser}</p>
                  <p className="text-xs text-gray-500 dark:text-gray-400 mt-0.5">{path}</p>
                </div>
              ))}
            </div>
          </Section>

          <Section id="more-info" title="6. More Information">
            <p>
              For further details about how we process your personal data, see our{" "}
              <Link to="/privacy" className="text-primary hover:underline font-medium">
                Privacy Policy
              </Link>
              . For questions about these cookies, contact us at{" "}
              <a href="mailto:support@mamvibe.com" className="text-primary hover:underline">
                support@mamvibe.com
              </a>
              .
            </p>
            <p>
              Useful external resources:
            </p>
            <ul className="space-y-1 ml-1">
              <Li>
                <span className="font-mono text-sm">allaboutcookies.org</span> — independent guide
                to cookies and browser privacy settings
              </Li>
              <Li>
                <span className="font-mono text-sm">cpdp.bg</span> — Commission for Personal Data
                Protection (Bulgarian DPA)
              </Li>
              <Li>
                <span className="font-mono text-sm">edpb.europa.eu</span> — European Data
                Protection Board guidelines on cookies
              </Li>
            </ul>
          </Section>

          <div className="flex gap-4 text-sm text-gray-400 dark:text-gray-500 pt-4 border-t border-gray-200 dark:border-gray-700">
            <Link to="/privacy" className="hover:text-primary transition-colors">Privacy Policy</Link>
            <span>·</span>
            <Link to="/terms" className="hover:text-primary transition-colors">Terms & Conditions</Link>
          </div>
        </article>
      </div>
    </div>
  );
}
