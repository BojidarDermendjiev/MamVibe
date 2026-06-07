import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { usePageSEO } from '@/hooks/useSEO';
import {
  Camera,
  Tag,
  Clock,
  CheckCircle,
  CreditCard,
  Truck,
  Package,
  MessageSquare,
  Search,
  ShoppingBag,
  Heart,
  Star,
} from 'lucide-react';

function StepItem({
  number,
  icon: Icon,
  title,
  description,
}: {
  number: number;
  icon: React.ElementType;
  title: string;
  description: string;
}) {
  return (
    <div className="flex gap-4">
      <div className="flex-none flex flex-col items-center">
        <div className="w-10 h-10 rounded-full bg-primary text-white flex items-center justify-center text-sm font-bold shadow-sm">
          {number}
        </div>
        <div className="w-px flex-1 bg-primary/15 mt-2 last:hidden" />
      </div>
      <div className="pb-8">
        <div className="flex items-center gap-2 mb-1">
          <Icon className="w-4 h-4 text-primary" />
          <h3 className="font-semibold text-gray-900 dark:text-white">{title}</h3>
        </div>
        <p className="text-[14px] text-gray-600 dark:text-gray-400 leading-relaxed">{description}</p>
      </div>
    </div>
  );
}

function Section({
  id,
  title,
  subtitle,
  children,
}: {
  id: string;
  title: string;
  subtitle?: string;
  children: React.ReactNode;
}) {
  return (
    <section id={id} className="scroll-mt-20 mb-16">
      <div className="mb-6">
        <h2 className="text-2xl font-bold text-gray-900 dark:text-white">{title}</h2>
        {subtitle && (
          <p className="text-gray-500 dark:text-gray-400 mt-1">{subtitle}</p>
        )}
      </div>
      {children}
    </section>
  );
}

const HOW_IT_WORKS_SCHEMA = {
  "@context": "https://schema.org",
  "@type": "HowTo",
  name: "How to buy and sell on MamVibe",
  description:
    "Step-by-step guide to buying, selling, and donating second-hand baby and children's items on MamVibe — Bulgaria's family marketplace.",
  step: [
    {
      "@type": "HowToStep",
      position: 1,
      name: "Browse listings",
      text: "Open /browse and filter by category, age group, price, and listing type to find what you need.",
    },
    {
      "@type": "HowToStep",
      position: 2,
      name: "Send a purchase request",
      text: "Click an item and press 'Send Purchase Request'. The seller has 48 hours to accept.",
    },
    {
      "@type": "HowToStep",
      position: 3,
      name: "Pay securely",
      text: "Once accepted, pay via MamVibe Wallet or by card through Stripe checkout.",
    },
    {
      "@type": "HowToStep",
      position: 4,
      name: "Receive your item",
      text: "The seller ships via Econt or Speedy. Track your shipment in Dashboard → Shipments.",
    },
    {
      "@type": "HowToStep",
      position: 5,
      name: "Confirm receipt",
      text: "Confirm receipt in Dashboard → Purchases to release funds to the seller.",
    },
  ],
};

export default function HowItWorksPage() {
  const { t } = useTranslation();

  usePageSEO({
    title: "How It Works — Buy, Sell & Donate",
    description:
      "Step-by-step guide to buying, selling, and donating second-hand baby and children's items on MamVibe. Learn about shipping, payments, and the purchase flow.",
    canonical: "https://mamvibe.com/how-it-works",
    index: true,
    structuredData: HOW_IT_WORKS_SCHEMA,
  });

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-[#0e0c1a] py-14 px-4">
      <div className="max-w-2xl mx-auto">

        {/* Page header */}
        <header className="mb-12">
          <h1 className="text-4xl font-bold text-gray-900 dark:text-white mb-3">
            {t('hiw.title', 'How It Works')}
          </h1>
          <p className="text-gray-500 dark:text-gray-400 leading-relaxed">
            {t('hiw.subtitle', 'Everything you need to know to start buying, selling, or donating on MamVibe in minutes.')}
          </p>
          {/* Quick navigation */}
          <div className="flex flex-wrap gap-2 mt-5">
            {[
              { href: '#buying',   label: t('hiw.nav_buying',   'Buying') },
              { href: '#selling',  label: t('hiw.nav_selling',  'Selling') },
              { href: '#shipping', label: t('hiw.nav_shipping', 'Shipping') },
              { href: '#payments', label: t('hiw.nav_payments', 'Payments') },
              { href: '#community',label: t('hiw.nav_community','Community') },
            ].map(({ href, label }) => (
              <a
                key={href}
                href={href}
                className="text-xs font-medium px-3 py-1.5 bg-primary/8 text-primary rounded-full hover:bg-primary/15 transition-colors"
              >
                {label}
              </a>
            ))}
          </div>
        </header>

        {/* Buying */}
        <Section
          id="buying"
          title={t('hiw.buying_title', 'How to buy')}
          subtitle={t('hiw.buying_subtitle', 'From browsing to doorstep in a few easy steps.')}
        >
          <div className="bg-white dark:bg-[#2d2a42] rounded-2xl border border-gray-100 dark:border-white/8 shadow-sm p-6">
            <StepItem
              number={1}
              icon={Search}
              title={t('hiw.buy_s1_title', 'Find what you need')}
              description={t('hiw.buy_s1_desc', 'Open Browse (/browse) and use the filters — category, age group, price range, and listing type (for sale or donation). You can also type a keyword in the search bar.')}
            />
            <StepItem
              number={2}
              icon={ShoppingBag}
              title={t('hiw.buy_s2_title', 'Send a purchase request')}
              description={t('hiw.buy_s2_desc', 'Click the item to view its photos and description. Press "Send Purchase Request" to notify the seller. You can also chat with the seller beforehand to ask questions or negotiate the price.')}
            />
            <StepItem
              number={3}
              icon={Clock}
              title={t('hiw.buy_s3_title', 'Wait for seller response')}
              description={t('hiw.buy_s3_desc', 'The seller has 48 hours to accept or decline your request. If they do not respond, the request is automatically cancelled and the item becomes available again.')}
            />
            <StepItem
              number={4}
              icon={CreditCard}
              title={t('hiw.buy_s4_title', 'Complete payment')}
              description={t('hiw.buy_s4_desc', 'Once the seller accepts, you have a short window to complete payment. Choose MamVibe Wallet (instant) or pay by card via Stripe checkout. Cash on delivery is also available when using Econt or Speedy.')}
            />
            <StepItem
              number={5}
              icon={Truck}
              title={t('hiw.buy_s5_title', 'Receive your item')}
              description={t('hiw.buy_s5_desc', 'After payment, the seller ships within 3 business days via Econt or Speedy. Choose courier office pickup, home delivery, or parcel locker. Track your shipment in Dashboard → Shipments.')}
            />
            <StepItem
              number={6}
              icon={CheckCircle}
              title={t('hiw.buy_s6_title', 'Confirm receipt')}
              description={t('hiw.buy_s6_desc', 'Once your item arrives, confirm receipt in Dashboard → Purchases to release the payment to the seller. If you do not confirm within 5 days of delivery, the purchase auto-confirms.')}
            />
          </div>
          <p className="mt-3 text-xs text-gray-500 dark:text-gray-500">
            {t('hiw.buy_dispute_note', 'If an item arrives significantly different from its description, open a dispute within 48 hours of delivery via Dashboard → Purchases → Report Problem.')}
          </p>
        </Section>

        {/* Selling / Donating */}
        <Section
          id="selling"
          title={t('hiw.selling_title', 'How to sell or donate')}
          subtitle={t('hiw.selling_subtitle', 'List your items in minutes. Most listings go live instantly.')}
        >
          <div className="bg-white dark:bg-[#2d2a42] rounded-2xl border border-gray-100 dark:border-white/8 shadow-sm p-6">
            <StepItem
              number={1}
              icon={Camera}
              title={t('hiw.sell_s1_title', 'Log in and click "Create"')}
              description={t('hiw.sell_s1_desc', 'You must be logged in to list items. Click "Create" in the top navigation or visit /create. Upload at least one clear photo of the item (multiple photos are recommended).')}
            />
            <StepItem
              number={2}
              icon={Tag}
              title={t('hiw.sell_s2_title', 'Fill in the details')}
              description={t('hiw.sell_s2_desc', 'Enter a descriptive title, a detailed description of the item\'s condition and any relevant information, the category (clothing, shoes, strollers, etc.), the age group, and the size.')}
            />
            <StepItem
              number={3}
              icon={Heart}
              title={t('hiw.sell_s3_title', 'Set a price or donate')}
              description={t('hiw.sell_s3_desc', 'Enter a price in BGN to sell the item. Leave the price field empty to donate it for free. Donations appear on the platform with a "Free" label and families can request them at no cost.')}
            />
            <StepItem
              number={4}
              icon={CheckCircle}
              title={t('hiw.sell_s4_title', 'Submit for moderation')}
              description={t('hiw.sell_s4_desc', 'Press Submit. AI content moderation reviews your listing automatically — most clear listings are approved and go live within seconds. Unusual items may take a few hours for manual admin review.')}
            />
            <StepItem
              number={5}
              icon={MessageSquare}
              title={t('hiw.sell_s5_title', 'Respond to requests')}
              description={t('hiw.sell_s5_desc', 'When a buyer sends a purchase request, you have 48 hours to accept or decline. You will receive a notification. You can also chat with interested buyers before they send a request.')}
            />
            <StepItem
              number={6}
              icon={Package}
              title={t('hiw.sell_s6_title', 'Ship and get paid')}
              description={t('hiw.sell_s6_desc', 'After the buyer pays, ship the item via Econt or Speedy within 3 business days. Once the buyer confirms receipt (or after the 5-day auto-confirm), funds are credited to your MamVibe Wallet.')}
            />
          </div>
        </Section>

        {/* Shipping */}
        <Section
          id="shipping"
          title={t('hiw.shipping_title', 'Shipping')}
        >
          <div className="space-y-4">
            <div className="bg-white dark:bg-[#2d2a42] rounded-2xl border border-gray-100 dark:border-white/8 shadow-sm p-6 space-y-4 text-[14px] text-gray-600 dark:text-gray-400 leading-relaxed">
              <p>
                {t('hiw.ship_p1', 'MamVibe integrates two major Bulgarian couriers: Econt and Speedy. You do not need to visit their websites — everything is handled inside MamVibe.')}
              </p>
              <div>
                <p className="font-medium text-gray-800 dark:text-gray-200 mb-2">{t('hiw.ship_options_title', 'Delivery options (both couriers):')}</p>
                <ul className="space-y-1.5 ml-4">
                  <li className="flex gap-2">
                    <span className="text-primary mt-1 text-xs">•</span>
                    <span><strong>{t('hiw.ship_office', 'Courier office pickup')}</strong> — {t('hiw.ship_office_desc', 'Collect the parcel from any Econt or Speedy office near you.')}</span>
                  </li>
                  <li className="flex gap-2">
                    <span className="text-primary mt-1 text-xs">•</span>
                    <span><strong>{t('hiw.ship_home', 'Home address delivery')}</strong> — {t('hiw.ship_home_desc', 'The courier delivers directly to your door.')}</span>
                  </li>
                  <li className="flex gap-2">
                    <span className="text-primary mt-1 text-xs">•</span>
                    <span><strong>{t('hiw.ship_locker', 'Parcel locker')}</strong> — {t('hiw.ship_locker_desc', 'Drop-off or pick-up from an automated locker at a convenient location.')}</span>
                  </li>
                </ul>
              </div>
              <p>
                {t('hiw.ship_cod', 'Cash on delivery (COD) is supported for both Econt and Speedy. This allows buyers to pay the courier in cash upon delivery instead of paying online.')}
              </p>
              <p>
                {t('hiw.ship_tracking', 'Track your shipment in Dashboard → Shipments. Real-time tracking events from the courier are displayed as the parcel moves.')}
              </p>
            </div>
          </div>
        </Section>

        {/* Payments */}
        <Section
          id="payments"
          title={t('hiw.payments_title', 'Payments & Wallet')}
        >
          <div className="bg-white dark:bg-[#2d2a42] rounded-2xl border border-gray-100 dark:border-white/8 shadow-sm p-6 space-y-5 text-[14px] text-gray-600 dark:text-gray-400 leading-relaxed">
            <div>
              <p className="font-medium text-gray-800 dark:text-gray-200 mb-2">{t('hiw.pay_methods_title', 'Payment methods:')}</p>
              <ul className="space-y-1.5 ml-4">
                <li className="flex gap-2">
                  <span className="text-primary mt-1 text-xs">•</span>
                  <span><strong>{t('hiw.pay_stripe', 'Card (Stripe)')}</strong> — {t('hiw.pay_stripe_desc', 'Pay by debit or credit card securely via Stripe. MamVibe never stores your card details.')}</span>
                </li>
                <li className="flex gap-2">
                  <span className="text-primary mt-1 text-xs">•</span>
                  <span><strong>{t('hiw.pay_wallet', 'MamVibe Wallet')}</strong> — {t('hiw.pay_wallet_desc', 'An internal balance you can top up from Settings → Wallet (minimum 5 BGN). Wallet balance never expires.')}</span>
                </li>
                <li className="flex gap-2">
                  <span className="text-primary mt-1 text-xs">•</span>
                  <span><strong>{t('hiw.pay_cod', 'Cash on delivery')}</strong> — {t('hiw.pay_cod_desc', 'Pay the courier in cash when they deliver your parcel. Available with both Econt and Speedy.')}</span>
                </li>
              </ul>
            </div>
            <div>
              <p className="font-medium text-gray-800 dark:text-gray-200 mb-1">{t('hiw.pay_escrow_title', 'Secure escrow:')}</p>
              <p>{t('hiw.pay_escrow_desc', 'Payments are held in escrow until the buyer confirms receipt. This protects buyers from fraud and gives sellers confidence that they will be paid once the item is delivered.')}</p>
            </div>
            <div>
              <p className="font-medium text-gray-800 dark:text-gray-200 mb-1">{t('hiw.pay_withdraw_title', 'Withdrawing earnings:')}</p>
              <p>{t('hiw.pay_withdraw_desc', 'Sellers can withdraw their wallet balance to a bank account via Settings → Wallet → Withdraw. An IBAN is required. Withdrawals are processed within 2 business days.')}</p>
            </div>
          </div>
        </Section>

        {/* Community */}
        <Section
          id="community"
          title={t('hiw.community_title', 'Community features')}
        >
          <div className="space-y-3">
            {[
              {
                icon: MessageSquare,
                title: t('hiw.comm_chat_title', 'Real-time chat'),
                desc: t('hiw.comm_chat_desc', 'Message sellers directly at /chat (login required). Use it to ask questions, negotiate prices, or arrange local pickup. Unread message count is shown on the Chat icon in the navigation bar.'),
              },
              {
                icon: Star,
                title: t('hiw.comm_doctors_title', 'Doctor reviews'),
                desc: t('hiw.comm_doctors_desc', 'Browse verified parent reviews of Bulgarian paediatricians and gynaecologists at /doctor-reviews. Filter by city and specialisation. Log in to write a review — it is published after admin approval.'),
              },
              {
                icon: Truck,
                title: t('hiw.comm_places_title', 'Child-friendly places'),
                desc: t('hiw.comm_places_desc', 'Discover parks, playgrounds, cafes, and family-friendly restaurants across Bulgaria at /child-friendly-places. Submit a new place after logging in — it goes live after admin approval.'),
              },
              {
                icon: Heart,
                title: t('hiw.comm_ratings_title', 'Seller ratings'),
                desc: t('hiw.comm_ratings_desc', 'After each completed sale, the buyer can rate the seller (1–5 stars with a comment). Ratings are visible on every seller\'s public profile page at /profile.'),
              },
            ].map(({ icon: Icon, title, desc }) => (
              <div key={title} className="flex gap-4 bg-white dark:bg-[#2d2a42] rounded-2xl border border-gray-100 dark:border-white/8 shadow-sm p-5">
                <Icon className="w-5 h-5 text-primary mt-0.5 flex-none" />
                <div>
                  <p className="font-medium text-gray-900 dark:text-white mb-1">{title}</p>
                  <p className="text-[14px] text-gray-600 dark:text-gray-400 leading-relaxed">{desc}</p>
                </div>
              </div>
            ))}
          </div>
        </Section>

        {/* CTA */}
        <div className="rounded-2xl bg-primary/5 dark:bg-primary/10 border border-primary/15 p-6 text-center">
          <p className="text-gray-700 dark:text-gray-300 mb-4">
            {t('hiw.cta_text', 'Ready to give baby items a second life?')}
          </p>
          <div className="flex flex-wrap gap-3 justify-center">
            <Link
              to="/browse"
              className="px-6 py-2.5 bg-primary text-white rounded-xl font-semibold text-sm hover:bg-primary/90 transition-colors"
            >
              {t('home.browse_btn', 'Browse Items')}
            </Link>
            <Link
              to="/faq"
              className="px-6 py-2.5 bg-gray-100 dark:bg-white/10 text-gray-800 dark:text-gray-100 rounded-xl font-semibold text-sm hover:bg-gray-200 dark:hover:bg-white/15 transition-colors"
            >
              {t('hiw.faq_link', 'View FAQ')}
            </Link>
          </div>
        </div>

      </div>
    </div>
  );
}
