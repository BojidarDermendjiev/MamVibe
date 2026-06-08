import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { usePageSEO } from '@/hooks/useSEO';
import { Heart, ShoppingBag, MapPin, Star, Shield, Users, Recycle, MessageSquare, Mail } from 'lucide-react';

function LinkedinIcon({ size = 16 }: { size?: number }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="currentColor" xmlns="http://www.w3.org/2000/svg">
      <path d="M20.447 20.452h-3.554v-5.569c0-1.328-.027-3.037-1.852-3.037-1.853 0-2.136 1.445-2.136 2.939v5.667H9.351V9h3.414v1.561h.046c.477-.9 1.637-1.85 3.37-1.85 3.601 0 4.267 2.37 4.267 5.455v6.286zM5.337 7.433a2.062 2.062 0 01-2.063-2.065 2.064 2.064 0 112.063 2.065zm1.782 13.019H3.555V9h3.564v11.452zM22.225 0H1.771C.792 0 0 .774 0 1.729v20.542C0 23.227.792 24 1.771 24h20.451C23.2 24 24 23.227 24 22.271V1.729C24 .774 23.2 0 22.222 0h.003z"/>
    </svg>
  );
}

function GithubIcon({ size = 16 }: { size?: number }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="currentColor" xmlns="http://www.w3.org/2000/svg">
      <path d="M12 2C6.477 2 2 6.477 2 12c0 4.418 2.865 8.166 6.839 9.489.5.092.682-.217.682-.482 0-.237-.009-.868-.013-1.703-2.782.604-3.369-1.341-3.369-1.341-.454-1.155-1.11-1.463-1.11-1.463-.908-.62.069-.608.069-.608 1.003.07 1.531 1.03 1.531 1.03.892 1.529 2.341 1.087 2.91.832.092-.647.35-1.088.636-1.338-2.22-.253-4.555-1.11-4.555-4.943 0-1.091.39-1.984 1.029-2.683-.103-.253-.446-1.27.098-2.647 0 0 .84-.269 2.75 1.025A9.578 9.578 0 0 1 12 6.836c.85.004 1.705.115 2.504.337 1.909-1.294 2.747-1.025 2.747-1.025.546 1.377.202 2.394.1 2.647.64.699 1.028 1.592 1.028 2.683 0 3.842-2.339 4.687-4.566 4.935.359.309.678.919.678 1.852 0 1.336-.012 2.415-.012 2.743 0 .267.18.578.688.48C19.138 20.163 22 16.418 22 12c0-5.523-4.477-10-10-10z"/>
    </svg>
  );
}

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

interface TeamMember {
  name: string;
  role: string;
  bio: string;
  photo: string;
  linkedin?: string;
  github?: string;
  email?: string;
}

function TeamMemberCard({ name, role, bio, photo, linkedin, github, email }: TeamMember) {
  return (
    <div className="flex flex-col items-center text-center group">
      <div className="relative w-52 sm:w-64 mb-5">
        <div className="rounded-2xl overflow-hidden shadow-md border border-gray-100 dark:border-white/10 aspect-[3/4]">
          <img
            src={photo}
            alt={name}
            className="w-full h-full object-cover object-top group-hover:scale-105 transition-transform duration-500"
            onError={(e) => {
              const img = e.currentTarget;
              img.src = `https://ui-avatars.com/api/?name=${encodeURIComponent(name)}&size=400&background=c4b5fd&color=fff&font-size=0.4`;
            }}
          />
        </div>
      </div>

      <h3 className="text-lg font-bold text-gray-900 dark:text-white mb-0.5">{name}</h3>
      <p className="text-sm font-medium text-primary mb-3">{role}</p>
      <p className="text-sm text-gray-600 dark:text-gray-400 leading-relaxed max-w-xs mb-4">{bio}</p>

      <div className="flex items-center gap-2">
        {linkedin && (
          <a
            href={linkedin}
            target="_blank"
            rel="noopener noreferrer"
            aria-label="LinkedIn"
            className="w-9 h-9 rounded-full flex items-center justify-center bg-gray-100 dark:bg-white/10 text-gray-500 dark:text-gray-400 hover:bg-blue-100 dark:hover:bg-blue-500/20 hover:text-blue-600 transition-all"
          >
            <LinkedinIcon size={16} />
          </a>
        )}
        {github && (
          <a
            href={github}
            target="_blank"
            rel="noopener noreferrer"
            aria-label="GitHub"
            className="w-9 h-9 rounded-full flex items-center justify-center bg-gray-100 dark:bg-white/10 text-gray-500 dark:text-gray-400 hover:bg-gray-200 dark:hover:bg-white/20 hover:text-gray-900 dark:hover:text-white transition-all"
          >
            <GithubIcon size={16} />
          </a>
        )}
        {email && (
          <a
            href={`mailto:${email}`}
            aria-label="Gmail"
            className="w-9 h-9 rounded-full flex items-center justify-center bg-gray-100 dark:bg-white/10 text-gray-500 dark:text-gray-400 hover:bg-red-100 dark:hover:bg-red-500/20 hover:text-red-500 transition-all"
          >
            <Mail size={16} />
          </a>
        )}
      </div>
    </div>
  );
}


export default function AboutPage() {
  const { t } = useTranslation();

  const team: TeamMember[] = [
    {
      name: "Bozhidar Dermendzhiev",
      role: t('about.team_bozhidar_role', 'Founder & Full-Stack Developer'),
      bio: t('about.team_bozhidar_bio', 'Software developer and father who built MamVibe from the ground up — combining a marketplace, community reviews, and local discovery into one platform for Bulgarian families.'),
      photo: "https://avatars.githubusercontent.com/u/116583072?v=4",
      linkedin: "https://www.linkedin.com/in/bozhidar-dermendzhiev-530441277/",
      github: "https://github.com/BojidarDermendjiev",
      email: "bozhidardermendjiew@gmail.com",
    },
  ];

  usePageSEO({
    title: "About MamVibe — Bulgaria's Baby Marketplace",
    description:
      "MamVibe is a free Bulgarian community platform where parents buy, sell, and donate second-hand baby items, read doctor reviews, and discover child-friendly places across Bulgaria.",
    canonical: "https://mamvibe.com/about",
    index: true,
    structuredData: ABOUT_SCHEMA,
  });

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-[#201d30]">
      {/* Hero */}
      <section className="bg-gradient-to-br from-primary/8 via-transparent to-lavender/10 dark:bg-none dark:bg-[#201d30] py-20 px-4">
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
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-4 text-center">
            {t('about.mission_title', 'Our mission')}
          </h2>
          <div className="prose prose-gray dark:prose-invert max-w-none text-[15px] leading-relaxed text-gray-600 dark:text-gray-300 space-y-4 text-center">
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
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-6 text-center">
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
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-6 text-center">
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

        {/* Team */}
        <section>
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-3 text-center">
            {t('about.team_title', 'The team')}
          </h2>
          <p className="text-[15px] text-gray-600 dark:text-gray-300 leading-relaxed mb-10 text-center">
            {t('about.team_desc', 'MamVibe is a passion project built by a small team of parents who wanted to create something genuinely useful for Bulgarian families. We are early-stage, product-focused, and growing.')}
          </p>
          <div className="flex flex-wrap justify-center gap-12">
            {team.map((member) => (
              <TeamMemberCard key={member.name} {...member} />
            ))}
          </div>
        </section>

        {/* Platform features */}
        <section>
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-4 text-center">
            {t('about.features_title', 'Everything on one platform')}
          </h2>
          <p className="text-[15px] text-gray-600 dark:text-gray-300 leading-relaxed mb-6 text-center">
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
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-4 text-center">
            {t('about.categories_title', 'What you can list')}
          </h2>
          <p className="text-[15px] text-gray-600 dark:text-gray-300 leading-relaxed mb-5 text-center">
            {t('about.categories_desc', 'MamVibe covers everything a growing child needs, from newborn to school age.')}
          </p>
          <div className="flex flex-wrap gap-2 justify-center">
            {[
              t('about.cat_clothing', 'Clothing'),
              t('about.cat_shoes', 'Shoes'),
              t('about.cat_strollers', 'Strollers'),
              t('about.cat_car_seats', 'Car Seats'),
              t('about.cat_toys', 'Toys'),
              t('about.cat_furniture', 'Furniture'),
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
