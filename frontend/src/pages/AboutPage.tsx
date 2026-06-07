import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { usePageSEO } from '@/hooks/useSEO';
import { Heart, ShoppingBag, MapPin, Star, Shield, Users, Recycle, MessageSquare } from 'lucide-react';

const ABOUT_SCHEMA = {
  "@context": "https://schema.org",
  "@type": "AboutPage",
  name: "About MamVibe",
  url: "https://mamvibe.com/about",
  description:
    "MamVibe is Bulgaria's community marketplace for second-hand baby and children's items. Learn about our mission, values, and features.",
  mainEntity: {
    "@context": "https://schema.org",
    "@type": "Organization",
    name: "MamVibe",
    url: "https://mamvibe.com",
    logo: "https://mamvibe.com/logo.png",
    description:
      "MamVibe is a free Bulgarian community platform where parents buy, sell, and donate second-hand baby and children's items, read verified doctor reviews, and discover child-friendly places.",
    foundingDate: "2024",
    areaServed: "Bulgaria",
    serviceType: "Second-hand baby and children's marketplace",
    contactPoint: {
      "@type": "ContactPoint",
      contactType: "customer support",
      email: "support@mamvibe.com",
    },
  },
};

function ValueCard({ icon: Icon, title, description }: { icon: React.ElementType; title: string; description: string }) {
  return (
    <div className="flex gap-4 p-5 bg-white dark:bg-[#2d2a42] rounded-2xl border border-gray-100 dark:border-white/8 shadow-sm">
      <div className="flex-none w-11 h-11 rounded-xl bg-primary/10 flex items-center justify-center">
        <Icon className="w-5 h-5 text-primary" />
      </div>
      <div>
        <h3 className="font-semibold text-gray-900 dark:text-white mb-1">{title}</h3>
        <p className="text-sm text-gray-600 dark:text-gray-400 leading-relaxed">{description}</p>
      </div>
    </div>
  );
}

function StatBadge({ number, label }: { number: string; label: string }) {
  return (
    <div className="text-center p-6 bg-primary/5 dark:bg-primary/10 rounded-2xl border border-primary/10">
      <p className="text-3xl font-bold text-primary mb-1">{number}</p>
      <p className="text-sm text-gray-600 dark:text-gray-400">{label}</p>
    </div>
  );
}

export default function AboutPage() {
  const { t } = useTranslation();

  usePageSEO({
    title: "About MamVibe — Bulgaria's Baby Marketplace",
    description:
      "MamVibe is a free Bulgarian community platform where parents buy, sell, and donate second-hand baby items, read doctor reviews, and discover child-friendly places across Bulgaria.",
    canonical: "https://mamvibe.com/about",
    index: true,
    structuredData: ABOUT_SCHEMA,
  });

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-[#0e0c1a]">
      {/* Hero */}
      <section className="bg-gradient-to-br from-primary/8 via-transparent to-lavender/10 dark:from-primary/15 py-20 px-4">
        <div className="max-w-3xl mx-auto text-center">
          <div className="inline-flex items-center gap-2 bg-primary/10 text-primary text-sm font-semibold px-4 py-1.5 rounded-full mb-6">
            <Heart className="w-4 h-4" />
            {t('about.badge', 'Made for Bulgarian families')}
          </div>
          <h1 className="text-4xl sm:text-5xl font-bold text-gray-900 dark:text-white mb-5 leading-tight">
            {t('about.hero_title', 'Give baby items a second life')}
          </h1>
          <p className="text-lg text-gray-600 dark:text-gray-300 leading-relaxed max-w-2xl mx-auto">
            {t('about.hero_subtitle', 'MamVibe is a free community marketplace where Bulgarian parents buy, sell, and donate second-hand baby and children\'s items — clothing, strollers, car seats, toys, furniture, and more.')}
          </p>
        </div>
      </section>

      <div className="max-w-4xl mx-auto px-4 py-16 space-y-20">

        {/* Mission */}
        <section>
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">
            {t('about.mission_title', 'Our mission')}
          </h2>
          <div className="prose prose-gray dark:prose-invert max-w-none text-[15px] leading-relaxed text-gray-600 dark:text-gray-300 space-y-4">
            <p>
              {t('about.mission_p1', 'Children grow fast — faster than parents can keep up with. The clothes, shoes, and gear that were perfect three months ago are already too small. MamVibe exists so that those items find a new home instead of a landfill.')}
            </p>
            <p>
              {t('about.mission_p2', 'We built a platform specifically for Bulgarian families: Bulgarian language support, integrated Econt and Speedy courier services, BGN prices, and a community that understands the local context. Whether you\'re in Sofia, Plovdiv, Varna, or a small town, MamVibe connects you with families nearby.')}
            </p>
            <p>
              {t('about.mission_p3', 'Listing an item takes under two minutes. Our AI content moderation reviews submissions instantly — most listings are live within seconds. Payments go through Stripe or our internal wallet. Shipping is arranged directly on the platform through Econt or Speedy, with options for courier office pickup, home delivery, or parcel lockers.')}
            </p>
          </div>
        </section>

        {/* Stats */}
        <section>
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-6">
            {t('about.community_title', 'A growing community')}
          </h2>
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
            <StatBadge number={t('about.stat_free', 'Free')} label={t('about.stat_free_label', 'Always free to use')} />
            <StatBadge number="2" label={t('about.stat_couriers_label', 'Integrated couriers')} />
            <StatBadge number="8" label={t('about.stat_categories_label', 'Item categories')} />
            <StatBadge number="BG" label={t('about.stat_country_label', 'Focused on Bulgaria')} />
          </div>
        </section>

        {/* Values */}
        <section>
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-6">
            {t('about.values_title', 'What we stand for')}
          </h2>
          <div className="grid sm:grid-cols-2 gap-4">
            <ValueCard
              icon={Recycle}
              title={t('about.value_sustainability_title', 'Sustainability')}
              description={t('about.value_sustainability_desc', 'Every item sold or donated is one less item in a landfill. We believe second-hand is the future of children\'s fashion and gear.')}
            />
            <ValueCard
              icon={Users}
              title={t('about.value_community_title', 'Community')}
              description={t('about.value_community_desc', 'MamVibe is built by parents, for parents. Real-time chat, seller ratings, doctor reviews, and child-friendly places are all part of a connected community.')}
            />
            <ValueCard
              icon={Shield}
              title={t('about.value_trust_title', 'Trust & safety')}
              description={t('about.value_trust_desc', 'AI content moderation, buyer reputation checks, a purchase request flow, and dispute resolution protect both sides of every transaction.')}
            />
            <ValueCard
              icon={Heart}
              title={t('about.value_accessibility_title', 'Accessibility')}
              description={t('about.value_accessibility_desc', 'Registration is free. Listing is free. There are no monthly fees. Donations are supported — parents can give items away at no cost.')}
            />
          </div>
        </section>

        {/* Platform features */}
        <section>
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">
            {t('about.features_title', 'Everything on one platform')}
          </h2>
          <p className="text-[15px] text-gray-600 dark:text-gray-300 leading-relaxed mb-6">
            {t('about.features_intro', 'MamVibe combines a marketplace, a community review platform, and a local directory into one app.')}
          </p>
          <div className="grid sm:grid-cols-3 gap-4">
            {[
              {
                icon: ShoppingBag,
                title: t('about.feat_marketplace_title', 'Marketplace'),
                desc: t('about.feat_marketplace_desc', 'Buy, sell, or donate clothing, shoes, strollers, car seats, toys, furniture, and feeding accessories across Bulgaria.'),
              },
              {
                icon: Star,
                title: t('about.feat_doctors_title', 'Doctor reviews'),
                desc: t('about.feat_doctors_desc', 'Browse verified parent reviews of Bulgarian paediatricians and gynaecologists, filtered by city and specialisation.'),
              },
              {
                icon: MapPin,
                title: t('about.feat_places_title', 'Child-friendly places'),
                desc: t('about.feat_places_desc', 'Discover parks, playgrounds, cafes, and family-friendly restaurants across Bulgaria, submitted by real families.'),
              },
              {
                icon: MessageSquare,
                title: t('about.feat_chat_title', 'Real-time chat'),
                desc: t('about.feat_chat_desc', 'Message sellers directly, negotiate prices, ask questions, or arrange local pickup — all inside MamVibe.'),
              },
              {
                icon: Shield,
                title: t('about.feat_payments_title', 'Secure payments'),
                desc: t('about.feat_payments_desc', 'Pay by card via Stripe, use the internal MamVibe Wallet, or choose cash on delivery. Funds are held until you confirm receipt.'),
              },
              {
                icon: Recycle,
                title: t('about.feat_shipping_title', 'Integrated shipping'),
                desc: t('about.feat_shipping_desc', 'Econt and Speedy are integrated directly — no need to visit courier websites. Track shipments in your dashboard.'),
              },
            ].map(({ icon: Icon, title, desc }) => (
              <div key={title} className="p-5 bg-white dark:bg-[#2d2a42] rounded-2xl border border-gray-100 dark:border-white/8 shadow-sm">
                <Icon className="w-5 h-5 text-primary mb-3" />
                <h3 className="font-semibold text-gray-900 dark:text-white mb-1.5 text-sm">{title}</h3>
                <p className="text-sm text-gray-600 dark:text-gray-400 leading-relaxed">{desc}</p>
              </div>
            ))}
          </div>
        </section>

        {/* Item categories */}
        <section>
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">
            {t('about.categories_title', 'What you can list')}
          </h2>
          <p className="text-[15px] text-gray-600 dark:text-gray-300 leading-relaxed mb-5">
            {t('about.categories_desc', 'MamVibe covers everything a growing child needs, from newborn to school age.')}
          </p>
          <div className="flex flex-wrap gap-2">
            {[
              t('about.cat_clothing', 'Clothing'),
              t('about.cat_shoes', 'Shoes'),
              t('about.cat_strollers', 'Strollers'),
              t('about.cat_car_seats', 'Car Seats'),
              t('about.cat_toys', 'Toys'),
              t('about.cat_furniture', 'Furniture'),
              t('about.cat_feeding', 'Feeding'),
              t('about.cat_other', 'Other'),
            ].map((cat) => (
              <span
                key={cat}
                className="px-3 py-1.5 bg-primary/8 text-primary font-medium text-sm rounded-lg"
              >
                {cat}
              </span>
            ))}
          </div>
        </section>

        {/* Contact CTA */}
        <section className="bg-white dark:bg-[#2d2a42] rounded-3xl border border-gray-100 dark:border-white/8 p-8 text-center">
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-3">
            {t('about.cta_title', 'Ready to start?')}
          </h2>
          <p className="text-gray-600 dark:text-gray-400 mb-6 max-w-md mx-auto">
            {t('about.cta_desc', 'Join thousands of Bulgarian families buying, selling, and donating baby items on MamVibe.')}
          </p>
          <div className="flex flex-wrap gap-3 justify-center">
            <Link
              to="/browse"
              className="px-6 py-2.5 bg-primary text-white rounded-xl font-semibold text-sm hover:bg-primary/90 transition-colors"
            >
              {t('home.browse_btn', 'Browse Items')}
            </Link>
            <Link
              to="/register"
              className="px-6 py-2.5 bg-gray-100 dark:bg-white/10 text-gray-800 dark:text-gray-100 rounded-xl font-semibold text-sm hover:bg-gray-200 dark:hover:bg-white/15 transition-colors"
            >
              {t('auth.register_btn', 'Create Account')}
            </Link>
          </div>
          <p className="mt-4 text-xs text-gray-400">
            {t('about.cta_support', 'Questions? Email us at')}{' '}
            <a href="mailto:support@mamvibe.com" className="text-primary hover:underline">
              support@mamvibe.com
            </a>
          </p>
        </section>

      </div>
    </div>
  );
}
