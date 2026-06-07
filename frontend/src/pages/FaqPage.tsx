import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { usePageSEO } from '@/hooks/useSEO';
import { ChevronDown, ChevronUp } from 'lucide-react';

interface FaqItem {
  q: string;
  a: string;
}

interface FaqSection {
  title: string;
  items: FaqItem[];
}

function FaqEntry({ q, a }: FaqItem) {
  const [open, setOpen] = useState(false);
  return (
    <div className="border-b border-gray-200 dark:border-white/8 last:border-0">
      <button
        onClick={() => setOpen((v) => !v)}
        className="w-full flex items-center justify-between gap-4 py-4 text-left focus:outline-none group"
        aria-expanded={open}
      >
        <span className="text-[15px] font-medium text-gray-900 dark:text-white group-hover:text-primary transition-colors">
          {q}
        </span>
        {open
          ? <ChevronUp className="w-4 h-4 text-primary flex-none" />
          : <ChevronDown className="w-4 h-4 text-gray-400 dark:text-gray-500 flex-none group-hover:text-primary transition-colors" />}
      </button>
      {open && (
        <p className="pb-5 text-[14px] text-gray-600 dark:text-gray-400 leading-relaxed">
          {a}
        </p>
      )}
    </div>
  );
}

function FaqSection({ title, items }: FaqSection) {
  return (
    <section className="mb-10">
      <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-1 pb-3 border-b-2 border-primary/20">
        {title}
      </h2>
      <div>
        {items.map((item) => (
          <FaqEntry key={item.q} q={item.q} a={item.a} />
        ))}
      </div>
    </section>
  );
}

export default function FaqPage() {
  const { t } = useTranslation();

  const faqSections: FaqSection[] = [
    {
      title: t('faq.section_general', 'General'),
      items: [
        {
          q: t('faq.q_what_is', 'What is MamVibe?'),
          a: t('faq.a_what_is', 'MamVibe is a free Bulgarian community marketplace where parents buy, sell, and donate second-hand baby and children\'s items. The platform also includes verified doctor reviews and a directory of child-friendly places across Bulgaria.'),
        },
        {
          q: t('faq.q_free', 'Is MamVibe free to use?'),
          a: t('faq.a_free', 'Yes. Registration, browsing, and listing items are completely free. There are no monthly fees or subscription charges.'),
        },
        {
          q: t('faq.q_languages', 'What languages does MamVibe support?'),
          a: t('faq.a_languages', 'MamVibe is available in Bulgarian and English. You can switch languages at any time using the language toggle in the top-right corner.'),
        },
        {
          q: t('faq.q_register', 'How do I create an account?'),
          a: t('faq.a_register', 'Visit /register and sign up with your email and password, or use the "Continue with Google" button for one-click registration. You will be asked to choose a profile type (Mom, Dad, or Other).'),
        },
        {
          q: t('faq.q_mobile', 'Is there a mobile app?'),
          a: t('faq.a_mobile', 'MamVibe is a progressive web application. You can access it from any mobile browser at mamvibe.com and add it to your home screen for an app-like experience. No download from an app store is required.'),
        },
      ],
    },
    {
      title: t('faq.section_buying', 'Buying'),
      items: [
        {
          q: t('faq.q_buy_how', 'How do I buy an item?'),
          a: t('faq.a_buy_how', 'Browse listings on /browse, click an item you like, then press "Send Purchase Request". The seller has 48 hours to accept. Once accepted, you complete payment by card (Stripe) or from your MamVibe Wallet.'),
        },
        {
          q: t('faq.q_buy_request_expire', 'What happens if the seller doesn\'t respond?'),
          a: t('faq.a_buy_request_expire', 'If the seller does not respond within 48 hours, your purchase request is automatically cancelled and you can request another item.'),
        },
        {
          q: t('faq.q_buy_confirm', 'Do I need to confirm receipt?'),
          a: t('faq.a_buy_confirm', 'Yes. Once your item arrives, go to Dashboard → Purchases and confirm receipt. This releases the payment to the seller. If you do not confirm within 5 days of delivery, the purchase auto-confirms.'),
        },
        {
          q: t('faq.q_buy_dispute', 'What if the item is not as described?'),
          a: t('faq.a_buy_dispute', 'Open a dispute within 48 hours of delivery via Dashboard → Purchases → Report Problem. Disputes are reviewed by MamVibe admins and resolved within 3–5 business days. For urgent help email support@mamvibe.com.'),
        },
        {
          q: t('faq.q_buy_donate', 'Can I get a donated (free) item?'),
          a: t('faq.a_buy_donate', 'Yes. Listings marked as "Donate" are free. Send a purchase request as normal, and the seller will arrange transfer at no cost.'),
        },
      ],
    },
    {
      title: t('faq.section_selling', 'Selling & Donating'),
      items: [
        {
          q: t('faq.q_sell_how', 'How do I list an item?'),
          a: t('faq.a_sell_how', 'Log in, click "Create" in the top navigation, upload at least one photo, fill in the title, description, category, age group, and size, then set a price (or leave it blank to donate for free). Submit — most listings are approved by AI moderation instantly.'),
        },
        {
          q: t('faq.q_sell_moderation', 'How long does moderation take?'),
          a: t('faq.a_sell_moderation', 'Automatic AI content moderation reviews listings within seconds. Clear listings with good photos and descriptions are approved instantly. Unusual or borderline items may go to manual admin review, which can take a few hours.'),
        },
        {
          q: t('faq.q_sell_donate', 'How do I list an item as a donation?'),
          a: t('faq.a_sell_donate', 'When creating a listing, leave the price field empty or set the listing type to "Donate". The item will appear as free for other families to request.'),
        },
        {
          q: t('faq.q_sell_edit', 'Can I edit my listing after it is live?'),
          a: t('faq.a_sell_edit', 'Yes. Go to Dashboard → My Listings, select the listing, and click Edit. Changes go through moderation again.'),
        },
        {
          q: t('faq.q_sell_payout', 'When do I receive my money?'),
          a: t('faq.a_sell_payout', 'Funds are credited to your MamVibe Wallet once the buyer confirms receipt (or after the 5-day auto-confirm window). Withdraw to your bank account via Settings → Wallet → Withdraw (IBAN required, processed in 2 business days).'),
        },
      ],
    },
    {
      title: t('faq.section_payments', 'Payments & Wallet'),
      items: [
        {
          q: t('faq.q_pay_methods', 'What payment methods are accepted?'),
          a: t('faq.a_pay_methods', 'You can pay by card via Stripe checkout, from your MamVibe Wallet balance, or choose cash on delivery (COD) when using Econt or Speedy shipping.'),
        },
        {
          q: t('faq.q_pay_wallet', 'What is the MamVibe Wallet?'),
          a: t('faq.a_pay_wallet', 'The MamVibe Wallet is an internal balance used for fast purchases. Top it up from Settings → Wallet (minimum 5 BGN, paid by card via Stripe). Wallet balance never expires. Withdraw earnings to your bank IBAN within 2 business days.'),
        },
        {
          q: t('faq.q_pay_secure', 'Are payments secure?'),
          a: t('faq.a_pay_secure', 'Yes. Card payments are processed by Stripe, a PCI DSS-certified payment provider. MamVibe never stores your card details. Funds are held in escrow until the buyer confirms receipt, protecting both parties.'),
        },
      ],
    },
    {
      title: t('faq.section_shipping', 'Shipping'),
      items: [
        {
          q: t('faq.q_ship_couriers', 'Which couriers are supported?'),
          a: t('faq.a_ship_couriers', 'MamVibe integrates Econt and Speedy — both major Bulgarian courier services. You do not need to visit their websites separately; everything is arranged inside MamVibe.'),
        },
        {
          q: t('faq.q_ship_options', 'What delivery options are available?'),
          a: t('faq.a_ship_options', 'For both Econt and Speedy you can choose courier office pickup, home address delivery, or parcel locker. Cash on delivery (COD) is supported for both couriers.'),
        },
        {
          q: t('faq.q_ship_track', 'How do I track my shipment?'),
          a: t('faq.a_ship_track', 'Go to Dashboard → Shipments. Each shipment shows real-time tracking events from the courier.'),
        },
        {
          q: t('faq.q_ship_when', 'How soon does the seller ship?'),
          a: t('faq.a_ship_when', 'After payment is confirmed, the seller is expected to ship within 3 business days.'),
        },
      ],
    },
    {
      title: t('faq.section_community', 'Community Features'),
      items: [
        {
          q: t('faq.q_doctors', 'How do doctor reviews work?'),
          a: t('faq.a_doctors', 'Visit /doctor-reviews to browse verified parent reviews of Bulgarian paediatricians and gynaecologists. You can filter by city and medical specialisation. To submit a review, log in — it goes live after admin approval.'),
        },
        {
          q: t('faq.q_places', 'What are child-friendly places?'),
          a: t('faq.a_places', 'The /child-friendly-places section is a crowdsourced directory of parks, playgrounds, cafes, and family-friendly restaurants across Bulgaria. Log in to submit a new place; it goes live after admin approval.'),
        },
        {
          q: t('faq.q_ratings', 'How do seller ratings work?'),
          a: t('faq.a_ratings', 'After each completed sale, the buyer can rate the seller (1–5 stars with an optional comment). Ratings are visible on every seller\'s public profile page and help buyers make informed decisions.'),
        },
      ],
    },
  ];

  const faqJsonLd = {
    "@context": "https://schema.org",
    "@type": "FAQPage",
    mainEntity: faqSections.flatMap((section) =>
      section.items.map(({ q, a }) => ({
        "@type": "Question",
        name: q,
        acceptedAnswer: {
          "@type": "Answer",
          text: a,
        },
      }))
    ),
  };

  usePageSEO({
    title: "FAQ — Frequently Asked Questions",
    description:
      "Answers to the most common questions about MamVibe — how to buy, sell, donate, ship, and pay on Bulgaria's second-hand baby marketplace.",
    canonical: "https://mamvibe.com/faq",
    index: true,
    structuredData: faqJsonLd,
  });

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-[#0e0c1a] py-14 px-4">
      <div className="max-w-2xl mx-auto">

        <header className="mb-12">
          <h1 className="text-4xl font-bold text-gray-900 dark:text-white mb-3">
            {t('faq.page_title', 'Frequently Asked Questions')}
          </h1>
          <p className="text-gray-500 dark:text-gray-400">
            {t('faq.page_subtitle', 'Everything you need to know about MamVibe.')}
          </p>
        </header>

        {faqSections.map((section) => (
          <FaqSection key={section.title} title={section.title} items={section.items} />
        ))}

        {/* Contact CTA */}
        <div className="mt-12 rounded-2xl bg-primary/5 dark:bg-primary/10 border border-primary/15 p-6 text-center">
          <p className="text-gray-700 dark:text-gray-300 mb-2">
            {t('faq.still_have_questions', "Still have questions?")}
          </p>
          <a
            href="mailto:support@mamvibe.com"
            className="font-medium text-primary hover:underline"
          >
            support@mamvibe.com
          </a>
          <span className="mx-2 text-gray-400">·</span>
          <Link to="/how-it-works" className="font-medium text-primary hover:underline">
            {t('faq.how_it_works_link', 'How It Works guide')}
          </Link>
        </div>

      </div>
    </div>
  );
}
