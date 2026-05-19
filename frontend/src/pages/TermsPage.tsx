import { type ReactNode } from "react";
import { Link } from "react-router-dom";
import { ScrollText } from "lucide-react";
import { usePageSEO } from "@/hooks/useSEO";

const LAST_UPDATED = "19 May 2026";

const TOC = [
  { id: "acceptance",   label: "1. Acceptance of Terms" },
  { id: "eligibility",  label: "2. Eligibility" },
  { id: "accounts",     label: "3. Accounts & Security" },
  { id: "listings",     label: "4. Listings (Sellers)" },
  { id: "buying",       label: "5. Buying" },
  { id: "payments",     label: "6. Payments & Escrow Wallet" },
  { id: "shipping",     label: "7. Shipping" },
  { id: "prohibited",   label: "8. Prohibited Items & Content" },
  { id: "ai",           label: "9. AI Features" },
  { id: "ip",           label: "10. Intellectual Property" },
  { id: "liability",    label: "11. Liability" },
  { id: "suspension",   label: "12. Suspension & Termination" },
  { id: "governing",    label: "13. Governing Law" },
  { id: "changes",      label: "14. Changes to These Terms" },
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

function Warning({ children }: { children: ReactNode }) {
  return (
    <div className="bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-700/40 rounded-lg p-4 text-[14px] text-amber-800 dark:text-amber-300">
      {children}
    </div>
  );
}

export default function TermsPage() {
  usePageSEO({
    title: "Terms & Conditions — MamVibe",
    description:
      "Read the MamVibe Terms & Conditions governing the use of our Bulgarian second-hand baby and children's marketplace.",
    canonical: "https://mamvibe.com/terms",
    index: true,
  });

  return (
    <div className="min-h-screen bg-white dark:bg-[#1a1825]">
      {/* Hero */}
      <div className="bg-gradient-to-br from-[#3f4b7f] to-[#945c67] py-16 px-4">
        <div className="max-w-4xl mx-auto text-white">
          <div className="flex items-center gap-3 mb-4">
            <ScrollText size={32} className="opacity-90" />
            <h1 className="text-3xl font-bold">Terms & Conditions</h1>
          </div>
          <p className="text-white/80 text-sm">
            Last updated: <strong className="text-white">{LAST_UPDATED}</strong>
          </p>
          <p className="text-white/70 text-sm mt-1">
            By creating an account or using MamVibe, you agree to these terms.
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
          </div>
        </aside>

        {/* Content */}
        <article>
          <Section id="acceptance" title="1. Acceptance of Terms">
            <p>
              These Terms & Conditions (<strong>Terms</strong>) form a legally binding agreement
              between you and MamVibe. By registering an account, browsing listings, or using any
              feature of the platform, you confirm that you have read, understood, and agree to be
              bound by these Terms and our{" "}
              <Link to="/privacy" className="text-primary hover:underline">Privacy Policy</Link>.
            </p>
            <p>
              If you do not agree to these Terms, you must not use MamVibe.
            </p>
          </Section>

          <Section id="eligibility" title="2. Eligibility">
            <ul className="space-y-2 ml-1">
              <Li>You must be at least <strong>18 years old</strong> to register and use MamVibe.</Li>
              <Li>
                Minors aged 13–17 may use the platform only with the explicit consent and active
                supervision of a parent or legal guardian, who accepts these Terms on their behalf.
              </Li>
              <Li>
                You must not be subject to any court order, legal restriction, or regulatory
                prohibition that would prevent you from entering into binding contracts.
              </Li>
              <Li>
                If you register on behalf of a business entity, you confirm that you have authority
                to bind that entity to these Terms.
              </Li>
            </ul>
          </Section>

          <Section id="accounts" title="3. Accounts & Security">
            <H3>Registration</H3>
            <p>
              You must provide accurate, current, and complete information when registering. You are
              responsible for keeping your account details up to date.
            </p>

            <H3>Credentials</H3>
            <ul className="space-y-1 ml-1">
              <Li>Keep your password confidential. Never share it with anyone, including MamVibe staff.</Li>
              <Li>
                You are responsible for all activity that occurs under your account. If you suspect
                unauthorised access, contact us immediately at{" "}
                <a href="mailto:support@mamvibe.com" className="text-primary hover:underline">
                  support@mamvibe.com
                </a>
                .
              </Li>
              <Li>One person may register only one account.</Li>
            </ul>

            <H3>Account Termination</H3>
            <p>
              You may delete your account at any time via Settings. We may retain certain data as
              required by law — see Section 6 of our{" "}
              <Link to="/privacy" className="text-primary hover:underline">Privacy Policy</Link>.
            </p>
          </Section>

          <Section id="listings" title="4. Listings (Sellers)">
            <H3>Accuracy</H3>
            <p>
              All listings must accurately represent the item being offered. Photos must show the
              actual item. Prices must be stated in Bulgarian Lev (BGN). You must disclose any
              material defects, damage, or missing parts.
            </p>

            <H3>Item Condition</H3>
            <p>
              You must select the correct condition category: <em>New</em>, <em>Like New</em>,{" "}
              <em>Good</em>, <em>Fair</em>, or <em>For Parts</em>. Misrepresenting condition is
              grounds for account suspension.
            </p>

            <H3>Listing Type</H3>
            <ul className="space-y-1 ml-1">
              <Li>
                <strong>Sell</strong> — item is offered at the stated price. Payment is processed
                through MamVibe; do not accept external payments.
              </Li>
              <Li>
                <strong>Donate</strong> — item is offered free of charge. No payment is collected
                by MamVibe. Shipping arrangements are between the parties.
              </Li>
            </ul>

            <H3>Moderation</H3>
            <p>
              New listings are reviewed by an AI moderation system and may be held for manual review
              by our team. Listings that violate these Terms will be removed without notice.
            </p>

            <Warning>
              You are solely responsible for the accuracy of your listings. MamVibe is a
              marketplace facilitator — we are not the seller and do not take title to any item.
            </Warning>
          </Section>

          <Section id="buying" title="5. Buying">
            <ul className="space-y-2 ml-1">
              <Li>
                By submitting a purchase or purchase request, you enter into a binding contract with
                the seller. Offers accepted through the purchase-request flow are commitments to buy.
              </Li>
              <Li>
                Review all listing photos and descriptions carefully before purchasing. Contact the
                seller via in-app chat if you have questions.
              </Li>
              <Li>
                MamVibe is not a party to the transaction between buyer and seller. However, the
                escrow wallet and dispute process exist to protect both parties.
              </Li>
            </ul>
          </Section>

          <Section id="payments" title="6. Payments & Escrow Wallet">
            <H3>Payment Methods</H3>
            <p>
              MamVibe supports card payments via Stripe and in-platform wallet payments. Card data
              is handled exclusively by Stripe — MamVibe never stores card numbers.
            </p>

            <H3>Escrow Wallet</H3>
            <p>The wallet operates as a trust-based escrow:</p>
            <ol className="space-y-1 ml-1 list-none">
              <li className="flex gap-2"><span className="text-primary shrink-0">①</span><span>Buyer tops up their wallet via Stripe PaymentIntent.</span></li>
              <li className="flex gap-2"><span className="text-primary shrink-0">②</span><span>On purchase, the item price is debited from the buyer's wallet and held in escrow.</span></li>
              <li className="flex gap-2"><span className="text-primary shrink-0">③</span><span>Funds are released to the seller's wallet after the buyer confirms delivery.</span></li>
              <li className="flex gap-2"><span className="text-primary shrink-0">④</span><span>If delivery is rejected, the escrow is returned to the buyer's wallet.</span></li>
            </ol>

            <H3>Fees</H3>
            <p>
              MamVibe does not currently charge a commission on transactions. This may change — any
              fee changes will be communicated 30 days in advance.
            </p>

            <H3>E-Bills</H3>
            <p>
              An electronic payment receipt (e-bill) is automatically issued for every completed
              sale. E-bills are available in your Dashboard and can be resent to your email.
            </p>

            <Warning>
              Payments made outside the MamVibe platform (cash, bank transfer, external apps) are
              at your own risk and are not covered by our escrow or dispute process.
            </Warning>
          </Section>

          <Section id="shipping" title="7. Shipping">
            <p>
              MamVibe integrates with Econt Express, Speedy, and Box Now for domestic Bulgarian
              shipping. Shipping costs are calculated at checkout based on weight, dimensions, and
              destination.
            </p>
            <ul className="space-y-2 ml-1 mt-2">
              <Li>
                Sellers are responsible for packaging items securely and handing them to the courier
                within 3 business days of a confirmed order.
              </Li>
              <Li>
                Shipping labels can be downloaded from your Dashboard after a shipment is created.
              </Li>
              <Li>
                Buyers and sellers can track shipments in real time via the Shipment detail page.
              </Li>
              <Li>
                MamVibe is not responsible for courier delays, lost parcels, or damage in transit.
                Disputes must be filed with the courier directly.
              </Li>
            </ul>
          </Section>

          <Section id="prohibited" title="8. Prohibited Items & Content">
            <H3>Prohibited Items</H3>
            <ul className="space-y-1 ml-1">
              <Li>Items that have been recalled by manufacturers or safety authorities</Li>
              <Li>Car seats older than 6 years or with unknown crash history</Li>
              <Li>Baby sleep products that do not meet current EU safety standards</Li>
              <Li>Counterfeit or replica goods</Li>
              <Li>Weapons, alcohol, tobacco, drugs, or adult content</Li>
              <Li>Stolen goods or items obtained illegally</Li>
              <Li>Medicines, supplements, or medical devices (unless by a licensed professional)</Li>
              <Li>Items subject to export restrictions or sanctions</Li>
            </ul>

            <H3>Prohibited Conduct</H3>
            <ul className="space-y-1 ml-1">
              <Li>Spamming, harassment, or threatening other users</Li>
              <Li>Attempting to conduct transactions outside the platform to avoid fees</Li>
              <Li>Creating multiple accounts to evade bans</Li>
              <Li>Uploading malicious code or attempting to compromise platform security</Li>
              <Li>Scraping, data-mining, or automated access without prior written consent</Li>
            </ul>
          </Section>

          <Section id="ai" title="9. AI Features">
            <p>
              MamVibe uses AI (Anthropic Claude) to assist with listing creation, price suggestions,
              and content moderation. These features are provided as-is for convenience.
            </p>
            <ul className="space-y-1 ml-1 mt-2">
              <Li>
                AI-generated titles, descriptions, and price suggestions are starting points — you
                are responsible for verifying their accuracy before publishing.
              </Li>
              <Li>
                AI moderation may incorrectly flag legitimate listings or miss prohibited content.
                Our human team reviews flagged items.
              </Li>
              <Li>
                MamVibe is not liable for decisions made on the basis of AI-generated content.
              </Li>
            </ul>
          </Section>

          <Section id="ip" title="10. Intellectual Property">
            <p>
              The MamVibe brand, logo, design system, and platform code are the property of MamVibe
              and may not be reproduced without written permission.
            </p>
            <p>
              By uploading photos or text to MamVibe, you grant us a non-exclusive, royalty-free,
              worldwide licence to display, resize, and cache that content solely for the purpose of
              operating the platform. You retain all ownership of your content and may request its
              deletion at any time.
            </p>
          </Section>

          <Section id="liability" title="11. Liability">
            <p>
              MamVibe is a marketplace platform — we facilitate transactions but are not a party to
              them. To the maximum extent permitted by Bulgarian and EU law:
            </p>
            <ul className="space-y-1 ml-1 mt-2">
              <Li>
                MamVibe is not liable for the quality, safety, legality, or description of items
                listed by sellers.
              </Li>
              <Li>
                We are not liable for indirect, consequential, or incidental loss arising from use
                of the platform.
              </Li>
              <Li>
                Our total liability to you for any claim is limited to the amount you paid to us
                (platform fees) in the 12 months preceding the claim.
              </Li>
            </ul>
            <p className="mt-3">
              Nothing in these Terms limits liability for death, personal injury, or fraud caused by
              our negligence.
            </p>
          </Section>

          <Section id="suspension" title="12. Suspension & Termination">
            <p>We may suspend or permanently ban an account if you:</p>
            <ul className="space-y-1 ml-1 mt-2">
              <Li>Violate these Terms or our community guidelines</Li>
              <Li>List prohibited items</Li>
              <Li>Receive repeated negative ratings or unresolved disputes</Li>
              <Li>Commit fraud or attempt to circumvent the payment system</Li>
              <Li>Provide false registration information</Li>
            </ul>
            <p className="mt-3">
              In serious cases (fraud, illegal items) suspension may be immediate and without prior
              notice. You may appeal a suspension by contacting{" "}
              <a href="mailto:support@mamvibe.com" className="text-primary hover:underline">
                support@mamvibe.com
              </a>
              .
            </p>
          </Section>

          <Section id="governing" title="13. Governing Law">
            <p>
              These Terms are governed by the laws of the <strong>Republic of Bulgaria</strong>.
              Any disputes that cannot be resolved amicably shall be referred to the competent
              courts of <strong>Sofia, Bulgaria</strong>.
            </p>
            <p>
              If you are a consumer resident in another EU member state, you may also benefit from
              mandatory consumer protection provisions of your country of residence and may use the
              EU Online Dispute Resolution platform at{" "}
              <span className="font-mono text-sm">ec.europa.eu/consumers/odr</span>.
            </p>
          </Section>

          <Section id="changes" title="14. Changes to These Terms">
            <p>
              We may revise these Terms at any time. For material changes we will notify you by
              email or in-app notification at least <strong>14 days</strong> before the new terms
              take effect. The updated Terms will be published on this page with a new effective
              date.
            </p>
            <p>
              Continued use of MamVibe after the effective date of any revision constitutes your
              acceptance of the new Terms.
            </p>
          </Section>

          <div className="flex gap-4 text-sm text-gray-400 dark:text-gray-500 pt-4 border-t border-gray-200 dark:border-gray-700">
            <Link to="/privacy" className="hover:text-primary transition-colors">Privacy Policy</Link>
            <span>·</span>
            <Link to="/cookies" className="hover:text-primary transition-colors">Cookie Policy</Link>
          </div>
        </article>
      </div>
    </div>
  );
}
