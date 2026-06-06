import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { motion } from "framer-motion";
import { usePageSEO } from "@/hooks/useSEO";
import { MoveRight, ShoppingBag, Heart, MapPin, ChevronRight, Camera, Tag, Truck, Baby, Smile, Shirt, User, Footprints } from "lucide-react";
import { doctorReviewsApi } from "../api/doctorReviewsApi";
import { childFriendlyPlacesApi } from "../api/childFriendlyPlacesApi";
import { statsApi, type PublicStats } from "../api/statsApi";
import type { DoctorReviewDto } from "../types/doctorReview";
import type { ChildFriendlyPlaceDto } from "../types/childFriendlyPlace";
import StarRating from "../components/common/StarRating";

// ── Brand data ─────────────────────────────────────────────────────────────
// Icons fetched via Google's s2/favicons service.
// Unlike faviconV2, s2/favicons never returns 404 — unknown domains get a
// default placeholder icon, so there are no console errors.
const BRANDS = [
  { name: "Cybex", domain: "cybex-online.com", accent: "#1a1a2e" },
  { name: "Little Dutch", domain: "little-dutch.com", accent: "#e8734a" },
  { name: "Nuna", domain: "nunababy.com", accent: "#2d6a4f" },
  { name: "Bugaboo", domain: "bugaboo.com", accent: "#d62828" },
  { name: "Ergobaby", domain: "ergobaby.com", accent: "#3a7dbf" },
  { name: "UPPAbaby", domain: "uppababy.com", accent: "#2b2d42" },
  { name: "Joie", domain: "joiebaby.com", accent: "#e07b39" },
  { name: "Chicco", domain: "chicco.com", accent: "#0057a8" },
  { name: "Maxi-Cosi", domain: "maxi-cosi.com", accent: "#c1121f" },
  { name: "BabyBjörn", domain: "babybjorn.com", accent: "#005f73" },
  { name: "Stokke", domain: "stokke.com", accent: "#6a4c93" },
  { name: "Graco", domain: "graco.com", accent: "#e07b39" },
  { name: "Britax", domain: "britax.co.uk", accent: "#c1121f" },
  { name: "Mamas & Papas", domain: "mamasandpapas.com", accent: "#8e6b9e" },
  { name: "Skip Hop", domain: "skiphop.com", accent: "#f4a261" },
  { name: "Hauck", domain: "hauck.de", accent: "#2b2d42" },
  { name: "Peg Perego", domain: "pegperego.com", accent: "#c1121f" },
  { name: "BABYZEN", domain: "babyzen.com", accent: "#1a1a2e" },
];

function googleFaviconUrl(domain: string, size = 64) {
  return `https://www.google.com/s2/favicons?domain=${domain}&sz=${size}`;
}

// Two identical copies — seamless -50% marquee loop
const BRAND_STRIP = [...BRANDS, ...BRANDS];

const PLACE_TYPE_LABELS: Record<number, string> = {
  0: "Walk",
  1: "Playground",
  2: "Restaurant",
  3: "Café",
  4: "Museum",
  5: "Zoo",
  6: "Beach",
  7: "Park",
  8: "Attraction",
  9: "Sports",
  10: "Other",
};

// ── WaveDivider ─────────────────────────────────────────────────────────────
// `from` and `to` accept either a literal hex or a CSS var() expression.
function WaveDivider({ from, to, flip = false }: { from: string; to: string; flip?: boolean }) {
  return (
    <div style={{ backgroundColor: from, lineHeight: 0 }}>
      <svg
        viewBox="0 0 1440 70"
        preserveAspectRatio="none"
        style={{ display: "block", width: "100%", height: 60, fill: to }}
      >
        <path
          d={
            flip
              ? "M0,70 C360,0 1080,70 1440,0 L1440,70 Z"
              : "M0,0 C360,70 1080,0 1440,70 L1440,70 L0,70 Z"
          }
        />
      </svg>
    </div>
  );
}

const CSS_SECTION_A = "var(--bg-section-a)";
const CSS_SECTION_B = "var(--bg-section-b)";

// ── BrandCard ───────────────────────────────────────────────────────────────
function BrandCard({
  name,
  domain,
  accent,
}: {
  name: string;
  domain: string;
  accent: string;
}) {
  const [imgError, setImgError] = useState(false);

  return (
    <div className="group/card flex items-center gap-3 bg-surface rounded-2xl border border-gray-100 dark:border-white/8 shadow-sm hover:shadow-md transition-all duration-300 select-none shrink-0 px-5 py-3.5 min-w-max">
      {/* Brand icon */}
      {!imgError ? (
        <img
          src={googleFaviconUrl(domain)}
          alt=""
          loading="lazy"
          className="w-8 h-8 rounded-lg object-contain grayscale opacity-60 group-hover/card:grayscale-0 group-hover/card:opacity-100 transition-all duration-300 shrink-0"
          onError={() => setImgError(true)}
        />
      ) : (
        /* Fallback: coloured initial circle */
        <div
          className="w-8 h-8 rounded-lg flex items-center justify-center text-white font-bold text-sm shrink-0"
          style={{ backgroundColor: accent }}
        >
          {name.charAt(0)}
        </div>
      )}

      {/* Brand name */}
      <span className="font-semibold text-gray-500 group-hover/card:text-primary-dark text-sm whitespace-nowrap transition-colors duration-300">
        {name}
      </span>
    </div>
  );
}

// ── Age groups ──────────────────────────────────────────────────────────────
const AGE_GROUPS = [
  {
    labelKey: "home.age_newborn",
    rangeKey: "home.age_newborn_range",
    icon: Baby,
    bg: "#FDDDD6",
    color: "#D96B58",
    query: "newborn",
  },
  {
    labelKey: "home.age_infant",
    rangeKey: "home.age_infant_range",
    icon: Smile,
    bg: "#D4EDE8",
    color: "#5AAFA0",
    query: "infant",
  },
  {
    labelKey: "home.age_toddler",
    rangeKey: "home.age_toddler_range",
    icon: Footprints,
    bg: "#F5E6C0",
    color: "#B8922E",
    query: "toddler",
  },
  {
    labelKey: "home.age_preschool",
    rangeKey: "home.age_preschool_range",
    icon: Shirt,
    bg: "#FDDDE0",
    color: "#D96B7B",
    query: "preschool",
  },
  {
    labelKey: "home.age_kids",
    rangeKey: "home.age_kids_range",
    icon: User,
    bg: "#D4E8E8",
    color: "#5AAFAF",
    query: "kids",
  },
];

// ── Platform features ───────────────────────────────────────────────────────
const FEATURES = [
  { icon: "🔔", titleKey: "home.feat_price_drop",   descKey: "home.feat_price_drop_desc",   bg: "#FDDDD6", color: "#C4705A" },
  { icon: "🔍", titleKey: "home.feat_saved_search", descKey: "home.feat_saved_search_desc", bg: "#D4EDE8", color: "#4A9E8E" },
  { icon: "💬", titleKey: "home.feat_offers",       descKey: "home.feat_offers_desc",       bg: "#EDE4F5", color: "#7E54A8" },
  { icon: "📦", titleKey: "home.feat_bundles",      descKey: "home.feat_bundles_desc",      bg: "#D4E8E8", color: "#3A8F8F" },
  { icon: "⭐", titleKey: "home.feat_bump",         descKey: "home.feat_bump_desc",         bg: "#FFF3CD", color: "#B8860B" },
  { icon: "👥", titleKey: "home.feat_follow",       descKey: "home.feat_follow_desc",       bg: "#FDDDE0", color: "#C4607A" },
  { icon: "🌴", titleKey: "home.feat_holiday",      descKey: "home.feat_holiday_desc",      bg: "#D4EDE8", color: "#3A8F6A" },
  { icon: "✅", titleKey: "home.feat_condition",    descKey: "home.feat_condition_desc",    bg: "#E8F4E8", color: "#3A7A3A" },
] as const;

// ── Organisation schema (homepage) ─────────────────────────────────────────
const ORGANIZATION_SCHEMA = {
  "@context": "https://schema.org",
  "@type": "Organization",
  name: "MamVibe",
  url: "https://mamvibe.com",
  logo: "https://mamvibe.com/logo.png",
  description:
    "MamVibe is a free platform for families to buy, sell, and donate second-hand baby clothes, strollers, toys and more.",
  foundingDate: "2024",
  sameAs: [],
  contactPoint: {
    "@type": "ContactPoint",
    contactType: "customer support",
    email: "support@mamvibe.com",
  },
};

const WEBSITE_SCHEMA = {
  "@context": "https://schema.org",
  "@type": "WebSite",
  name: "MamVibe",
  url: "https://mamvibe.com",
  potentialAction: {
    "@type": "SearchAction",
    target: {
      "@type": "EntryPoint",
      urlTemplate: "https://mamvibe.com/browse?search={search_term_string}",
    },
    "query-input": "required name=search_term_string",
  },
};

// ── Page component ──────────────────────────────────────────────────────────
export default function HomePage() {
  const { t } = useTranslation();
  const [titleNumber, setTitleNumber] = useState(0);

  // SEO: inject unique title, description, Open Graph, and structured data
  // for the homepage. The Organisation + WebSite schemas tell Google about
  // our brand identity and enable the Sitelinks Search Box in SERPs.
  usePageSEO({
    title: "Buy, Sell & Donate Baby Items",
    description:
      "MamVibe is a free platform for Bulgarian families to buy, sell, and donate second-hand baby clothes, strollers, toys, car seats and more. Give baby items a second life.",
    canonical: "https://mamvibe.com/",
    structuredData: [ORGANIZATION_SCHEMA, WEBSITE_SCHEMA],
  });
  const [doctorReviews, setDoctorReviews] = useState<DoctorReviewDto[]>([]);
  const [childPlaces, setChildPlaces] = useState<ChildFriendlyPlaceDto[]>([]);
  const [stats, setStats] = useState<PublicStats | null>(null);
  const titles = useMemo(
    () => [
      t("home.hero_word_amazing"),
      t("home.hero_word_stylish"),
      t("home.hero_word_vibrant"),
      t("home.hero_word_special"),
      t("home.hero_word_lovely"),
    ],
    [t],
  );

  useEffect(() => {
    const id = setTimeout(() => {
      setTitleNumber((n) => (n === titles.length - 1 ? 0 : n + 1));
    }, 2000);
    return () => clearTimeout(id);
  }, [titleNumber, titles]);

  useEffect(() => {
    doctorReviewsApi
      .getAll({ pageSize: 3 })
      .then(setDoctorReviews)
      .catch(() => {});
    childFriendlyPlacesApi
      .getAll({ pageSize: 3 })
      .then(setChildPlaces)
      .catch(() => {});
    statsApi.getPublic().then(setStats).catch(() => {});
  }, []);

  const steps = [
    {
      icon: Camera,
      titleKey: "home.step1_title",
      descKey: "home.step1_desc",
      bg: "#FDDDD6",
      color: "#C4705A",
      badge: "1",
    },
    {
      icon: Tag,
      titleKey: "home.step2_title",
      descKey: "home.step2_desc",
      bg: "#D4EDE8",
      color: "#4A9E8E",
      badge: "2",
    },
    {
      icon: Truck,
      titleKey: "home.step3_title",
      descKey: "home.step3_desc",
      bg: "#F5E6C0",
      color: "#9A7A2A",
      badge: "3",
    },
    {
      icon: Heart,
      titleKey: "home.step4_title",
      descKey: "home.step4_desc",
      bg: "#FDDDE0",
      color: "#C4607A",
      badge: "4",
    },
  ];

  return (
    <div>
      {/* ── Hero ── */}
      <section className="relative w-full overflow-hidden min-h-[560px] flex items-center">
        {/* LCP candidate: hero image must NOT be lazy-loaded.
            fetchpriority="high" signals the browser to load it immediately
            as it is the Largest Contentful Paint element on the homepage. */}
        <img
          src="/hero-bg.jpg"
          alt=""
          aria-hidden="true"
          fetchPriority="high"
          loading="eager"
          decoding="sync"
          width={1920}
          height={1080}
          className="absolute inset-0 w-full h-full object-cover object-center"
        />
        {/* Gradient overlay for text readability */}
        <div className="absolute inset-0 bg-gradient-to-r from-black/60 via-black/45 to-black/30" />

        <div className="relative z-10 container mx-auto px-4 w-full">
          <div className="flex gap-8 pt-32 pb-20 lg:pt-44 lg:pb-28 items-center justify-center flex-col">
            <div className="flex gap-4 flex-col items-center">
              <h1 className="text-5xl md:text-7xl max-w-2xl tracking-tight text-center font-bold drop-shadow-lg">
                <span className="text-white">{t("home.hero_title")} </span>
                <span className="relative flex w-full justify-center overflow-hidden text-center md:pb-4 md:pt-1">
                  &nbsp;
                  {titles.map((title, index) => (
                    <motion.span
                      key={index}
                      className="absolute font-bold text-peach-light drop-shadow-md"
                      initial={{ opacity: 0, y: -100 }}
                      transition={{ type: "spring", stiffness: 50 }}
                      animate={
                        titleNumber === index
                          ? { y: 0, opacity: 1 }
                          : { y: titleNumber > index ? -150 : 150, opacity: 0 }
                      }
                    >
                      {title}
                    </motion.span>
                  ))}
                </span>
              </h1>
              <p className="text-lg md:text-xl leading-relaxed text-white/85 max-w-2xl text-center drop-shadow">
                {t("home.hero_subtitle")}
              </p>
            </div>

            <div className="flex flex-row gap-3">
              <Link to="/browse">
                <button className="inline-flex items-center gap-2 px-7 py-3 rounded-lg border-2 border-white text-white font-semibold hover:bg-white hover:text-primary transition-all duration-300">
                  {t("home.browse_btn")} <ShoppingBag className="w-4 h-4" />
                </button>
              </Link>
              <Link to="/create">
                <button className="inline-flex items-center gap-2 px-7 py-3 rounded-lg border-2 border-white text-white font-semibold hover:bg-white hover:text-primary transition-all duration-300">
                  {t("home.create_btn")} <MoveRight className="w-4 h-4" />
                </button>
              </Link>
            </div>
          </div>
        </div>
      </section>

      {/* ── Trusted Brands ── */}
      <section aria-labelledby="brands-heading" className="bg-section-b py-12 overflow-hidden">
        {/* h2 styled to look like a small label — semantically correct for heading hierarchy */}
        <h2 id="brands-heading" className="text-xs font-semibold uppercase tracking-widest text-gray-400 text-center mb-8">
          {t("home.brands_title")}
        </h2>

        {/* Single row — left → right */}
        <div className="group relative w-full overflow-hidden [--duration:40s]">
          <div className="flex w-max gap-4 px-4 animate-marquee group-hover:[animation-play-state:paused]">
            {BRAND_STRIP.map((b, i) => (
              <BrandCard
                key={`r1-${i}`}
                name={b.name}
                domain={b.domain}
                accent={b.accent}
              />
            ))}
          </div>
          <div className="pointer-events-none absolute inset-y-0 left-0 w-24" style={{ background: `linear-gradient(to right, var(--bg-section-b), transparent)` }} />
          <div className="pointer-events-none absolute inset-y-0 right-0 w-24" style={{ background: `linear-gradient(to left, var(--bg-section-b), transparent)` }} />
        </div>
      </section>

      <WaveDivider from={CSS_SECTION_B} to={CSS_SECTION_A} flip />

      {/* ── How it works ── */}
      <section
        aria-labelledby="how-it-works-heading"
        className="bg-section-a py-20 px-4"
      >
        <div className="max-w-5xl mx-auto">
          <div className="text-center mb-16">
            <h2 id="how-it-works-heading" className="text-3xl md:text-4xl font-bold mb-3 text-primary-dark">
              {t("home.how_it_works")}
            </h2>
            <p className="text-gray-500 dark:text-gray-400">{t("home.how_it_works_subtitle")}</p>
          </div>

          <div className="grid grid-cols-2 md:grid-cols-4 gap-10">
            {steps.map((step, i) => (
              <div
                key={i}
                className="flex flex-col items-center text-center gap-4"
              >
                {/* Icon box with badge */}
                <div className="relative">
                  <div
                    className="w-20 h-20 rounded-2xl flex items-center justify-center"
                    style={{ backgroundColor: step.bg }}
                  >
                    <step.icon
                      className="w-9 h-9"
                      style={{ color: step.color }}
                    />
                  </div>
                  <div className="absolute -top-2 -right-2 w-6 h-6 rounded-full bg-[#E8724A] text-white text-xs font-bold flex items-center justify-center shadow-sm">
                    {step.badge}
                  </div>
                </div>
                {/* Text */}
                <div>
                  <h3 className="font-bold text-gray-800 dark:text-gray-100 text-base mb-2">
                    {t(step.titleKey)}
                  </h3>
                  <p className="text-sm text-gray-500 dark:text-gray-400 leading-relaxed">
                    {t(step.descKey)}
                  </p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* ── Shop by Age ── */}
      <section className="bg-section-a py-20 px-4">
        <div className="max-w-5xl mx-auto">
          <div className="text-center mb-12">
            <h2 className="text-3xl md:text-4xl font-bold mb-3 text-primary-dark">
              {t("home.shop_by_age")}
            </h2>
            <p className="text-gray-500 dark:text-gray-400">{t("home.shop_by_age_subtitle")}</p>
          </div>
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-5 gap-14">
            {AGE_GROUPS.map((group) => (
              <Link
                key={group.labelKey}
                to={`/browse?age=${group.query}`}
                className="bg-section-b rounded-2xl p-6 flex flex-col items-center gap-3 shadow-sm border border-black/5 dark:border-white/5 hover:shadow-md hover:-translate-y-1 transition-all duration-300"
              >
                <div
                  className="w-14 h-14 rounded-2xl flex items-center justify-center"
                  style={{ backgroundColor: group.bg }}
                >
                  <group.icon size={26} style={{ color: group.color }} />
                </div>
                <div className="text-center">
                  <p className="font-bold text-gray-800 dark:text-gray-100 text-sm">
                    {t(group.labelKey)}
                  </p>
                  <p className="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
                    {t(group.rangeKey)}
                  </p>
                </div>
              </Link>
            ))}
          </div>
        </div>
      </section>

      <WaveDivider from={CSS_SECTION_A} to={CSS_SECTION_B} />

      {/* ── Features + Stats ── */}
      <section
        aria-labelledby="features-heading"
        className="bg-section-b py-24 px-4"
      >
        <div className="max-w-5xl mx-auto">
          {/* Section heading */}
          <div className="text-center mb-12">
            <h2 id="features-heading" className="text-3xl md:text-4xl font-bold text-primary-dark mb-3">
              {t("home.features_title")}
            </h2>
            <p className="text-gray-500 dark:text-gray-500 max-w-xl mx-auto">{t("home.features_subtitle")}</p>
          </div>

          {/* Feature grid */}
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-6">
            {FEATURES.map((feat, i) => (
              <motion.div
                key={feat.titleKey}
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ duration: 0.35, delay: i * 0.05 }}
                className="flex flex-col items-center text-center gap-3 p-5 rounded-2xl border border-black/6 dark:border-white/5 bg-surface hover:shadow-md transition-shadow duration-300"
              >
                <div
                  className="w-12 h-12 rounded-xl flex items-center justify-center text-2xl"
                  style={{ backgroundColor: feat.bg }}
                >
                  {feat.icon}
                </div>
                <div>
                  <p className="font-bold text-gray-800 dark:text-gray-100 text-sm mb-1">
                    {t(feat.titleKey)}
                  </p>
                  <p className="text-xs text-gray-500 dark:text-gray-400 leading-relaxed">
                    {t(feat.descKey)}
                  </p>
                </div>
              </motion.div>
            ))}
          </div>

          {/* Stats bar — below the feature grid */}
          {stats && (
            <motion.div
              initial={{ opacity: 0, y: 16 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.4 }}
              className="grid grid-cols-3 gap-4 mt-16"
            >
              {[
                { value: stats.activeListings, labelKey: "home.stat_listings" },
                { value: stats.totalSellers,   labelKey: "home.stat_sellers" },
                { value: stats.happyFamilies,  labelKey: "home.stat_families" },
              ].map(({ value, labelKey }) => (
                <div
                  key={labelKey}
                  className="bg-surface rounded-2xl py-6 flex flex-col items-center gap-1 border border-black/8 dark:border-white/5"
                >
                  <span className="text-3xl md:text-4xl font-extrabold text-primary-dark tabular-nums">
                    {value > 0 ? `${value.toLocaleString()}+` : "—"}
                  </span>
                  <span className="text-xs font-semibold uppercase tracking-widest text-gray-500 dark:text-gray-400">
                    {t(labelKey)}
                  </span>
                </div>
              ))}
            </motion.div>
          )}
        </div>
      </section>

      {/* ── Doctor Reviews ── */}
      {doctorReviews.length > 0 && (
        <>
        <WaveDivider from={CSS_SECTION_B} to={CSS_SECTION_A} flip />
        <section className="bg-section-a py-20 px-4">
          <div className="max-w-5xl mx-auto">
            <div className="text-center mb-10">
              <h2 className="text-3xl md:text-4xl font-bold text-primary-dark mb-2">
                {t("home.doctors_title")}
              </h2>
              <p className="text-gray-500 dark:text-gray-400">{t("home.doctors_subtitle")}</p>
              <Link
                to="/doctor-reviews"
                className="hidden sm:inline-flex items-center gap-1 text-sm font-semibold text-primary hover:text-primary-dark transition-colors mt-3"
              >
                {t("home.doctors_view_all")}{" "}
                <ChevronRight className="w-4 h-4" />
              </Link>
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-6">
              {doctorReviews.map((review) => (
                <div
                  key={review.id}
                  className="bg-surface rounded-2xl p-5 flex flex-col gap-3 border border-black/6 dark:border-white/10 hover:shadow-md transition-shadow duration-300"
                >
                  <StarRating value={review.rating} readonly size="sm" />
                  <div>
                    <p className="font-bold text-gray-800 dark:text-gray-100 text-base">
                      {review.doctorName}
                    </p>
                    <p className="text-sm text-gray-500 dark:text-gray-400">
                      {review.specialization}
                    </p>
                  </div>
                  <div className="flex items-center gap-1 text-xs text-gray-500 dark:text-gray-500">
                    <MapPin className="w-3 h-3" /> {review.city}
                  </div>
                  <p className="text-sm text-gray-600 dark:text-gray-300 line-clamp-3 flex-1">
                    {review.content}
                  </p>
                  <div className="flex items-center gap-2 pt-2 border-t border-black/6 dark:border-white/10 text-xs text-gray-500">
                    <div className="w-6 h-6 rounded-full bg-peach-light flex items-center justify-center font-bold text-primary text-xs">
                      {review.isAnonymous
                        ? "?"
                        : (review.authorDisplayName?.charAt(0) ?? "?")}
                    </div>
                    {review.isAnonymous
                      ? t("feedback.anonymous")
                      : review.authorDisplayName}
                  </div>
                </div>
              ))}
            </div>
            <div className="flex justify-center mt-8 sm:hidden">
              <Link
                to="/doctor-reviews"
                className="flex items-center gap-1 text-sm font-semibold text-primary hover:text-primary-dark transition-colors"
              >
                {t("home.doctors_view_all")}{" "}
                <ChevronRight className="w-4 h-4" />
              </Link>
            </div>
          </div>
        </section>
        </>
      )}

      {/* ── Child-Friendly Places ── */}
      {childPlaces.length > 0 && (
        <>
        <WaveDivider from={doctorReviews.length > 0 ? CSS_SECTION_A : CSS_SECTION_B} to={CSS_SECTION_B} />
        <section className="bg-section-b py-20 px-4">
          <div className="max-w-5xl mx-auto">
            <div className="text-center mb-10">
              <h2 className="text-3xl md:text-4xl font-bold text-primary-dark mb-2">
                {t("home.places_title")}
              </h2>
              <p className="text-gray-500 dark:text-gray-400">{t("home.places_subtitle")}</p>
              <Link
                to="/child-friendly-places"
                className="hidden sm:inline-flex items-center gap-1 text-sm font-semibold text-primary hover:text-primary-dark transition-colors mt-3"
              >
                {t("home.places_view_all")} <ChevronRight className="w-4 h-4" />
              </Link>
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-6">
              {childPlaces.map((place) => (
                <div
                  key={place.id}
                  className="bg-surface rounded-2xl overflow-hidden border border-black/6 dark:border-white/10 hover:shadow-md transition-shadow duration-300 flex flex-col"
                >
                  {place.photoUrl ? (
                    <img
                      src={place.photoUrl}
                      alt={place.name}
                      loading="lazy"
                      decoding="async"
                      width={400}
                      height={160}
                      className="w-full h-40 object-cover"
                    />
                  ) : (
                    <div className="w-full h-40 bg-section-a flex items-center justify-center text-4xl">
                      🏡
                    </div>
                  )}
                  <div className="p-5 flex flex-col gap-2 flex-1">
                    <div className="flex items-start justify-between gap-2">
                      <p className="font-bold text-gray-800 dark:text-gray-100 text-base leading-tight">
                        {place.name}
                      </p>
                      <span className="shrink-0 text-xs font-semibold px-2 py-0.5 rounded-full bg-peach-light text-primary">
                        {PLACE_TYPE_LABELS[place.placeType] ?? "Place"}
                      </span>
                    </div>
                    <div className="flex items-center gap-1 text-xs text-gray-500 dark:text-gray-500">
                      <MapPin className="w-3 h-3" /> {place.city}
                    </div>
                    <p className="text-sm text-gray-600 dark:text-gray-300 line-clamp-2 flex-1">
                      {place.description}
                    </p>
                  </div>
                </div>
              ))}
            </div>
            <div className="flex justify-center mt-8 sm:hidden">
              <Link
                to="/child-friendly-places"
                className="flex items-center gap-1 text-sm font-semibold text-primary hover:text-primary-dark transition-colors"
              >
                {t("home.places_view_all")} <ChevronRight className="w-4 h-4" />
              </Link>
            </div>
          </div>
        </section>
        </>
      )}

      <WaveDivider
        from={childPlaces.length > 0 ? CSS_SECTION_B : doctorReviews.length > 0 ? CSS_SECTION_A : CSS_SECTION_B}
        to={CSS_SECTION_A}
        flip
      />

      {/* ── Support section ── */}
      <section className="bg-section-a py-24 px-4">
        <motion.div
          initial={{ opacity: 0, y: 24 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          transition={{ duration: 0.5 }}
          className="max-w-2xl mx-auto text-center"
        >
          {/* Animated heart */}
          <motion.div
            animate={{ scale: [1, 1.15, 1] }}
            transition={{ duration: 2, repeat: Infinity, ease: "easeInOut" }}
            className="inline-flex items-center justify-center w-16 h-16 rounded-full mb-6"
            style={{ backgroundColor: "rgba(148,92,103,0.1)" }}
          >
            <Heart className="w-8 h-8 text-primary fill-primary/20" />
          </motion.div>

          <h2 className="text-3xl md:text-4xl font-bold text-primary-dark mb-4">
            {t("home.support_title")}
          </h2>
          <p className="text-base text-gray-500 dark:text-gray-400 leading-relaxed mb-10 max-w-md mx-auto">
            {t("home.support_body")}
          </p>

          <Link to="/donate">
            <motion.button
              whileHover={{ scale: 1.04 }}
              whileTap={{ scale: 0.97 }}
              className="inline-flex items-center gap-2.5 px-8 py-3.5 rounded-full font-semibold text-base text-white transition-all duration-200 shadow-md hover:shadow-lg"
              style={{ backgroundColor: "#945c67" }}
            >
              <Heart className="w-4 h-4 fill-white/30" />
              {t("home.support_btn")}
            </motion.button>
          </Link>
        </motion.div>
      </section>

      <WaveDivider from={CSS_SECTION_A} to={CSS_SECTION_B} />
    </div>
  );
}
