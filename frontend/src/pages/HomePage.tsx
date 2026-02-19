import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { motion } from "framer-motion";
import { MoveRight, ShoppingBag } from "lucide-react";
import { HiCamera, HiTag, HiTruck, HiHeart } from "react-icons/hi";
import { FaBaby, FaSmile, FaTshirt, FaChild } from "react-icons/fa";
import { GiFootprint } from "react-icons/gi";

// ── Brand data ─────────────────────────────────────────────────────────────
// Icons fetched via Google's s2/favicons service.
// Unlike faviconV2, s2/favicons never returns 404 — unknown domains get a
// default placeholder icon, so there are no console errors.
const BRANDS = [
  { name: "Cybex",          domain: "cybex-online.com",    accent: "#1a1a2e" },
  { name: "Little Dutch",   domain: "little-dutch.com",    accent: "#e8734a" },
  { name: "Nuna",           domain: "nunababy.com",        accent: "#2d6a4f" },
  { name: "Bugaboo",        domain: "bugaboo.com",         accent: "#d62828" },
  { name: "Ergobaby",       domain: "ergobaby.com",        accent: "#3a7dbf" },
  { name: "UPPAbaby",       domain: "uppababy.com",        accent: "#2b2d42" },
  { name: "Joie",           domain: "joiebaby.com",        accent: "#e07b39" },
  { name: "Chicco",         domain: "chicco.com",          accent: "#0057a8" },
  { name: "Maxi-Cosi",      domain: "maxi-cosi.com",       accent: "#c1121f" },
  { name: "BabyBjörn",      domain: "babybjorn.com",       accent: "#005f73" },
  { name: "Stokke",         domain: "stokke.com",          accent: "#6a4c93" },
  { name: "Graco",          domain: "graco.com",           accent: "#e07b39" },
  { name: "Britax",         domain: "britax.co.uk",        accent: "#c1121f" },
  { name: "Mamas & Papas",  domain: "mamasandpapas.com",   accent: "#8e6b9e" },
  { name: "Skip Hop",       domain: "skiphop.com",         accent: "#f4a261" },
  { name: "Hauck",          domain: "hauck.de",            accent: "#2b2d42" },
  { name: "Peg Perego",     domain: "pegperego.com",       accent: "#c1121f" },
  { name: "BABYZEN",        domain: "babyzen.com",          accent: "#1a1a2e" },
];

function googleFaviconUrl(domain: string, size = 64) {
  return `https://www.google.com/s2/favicons?domain=${domain}&sz=${size}`;
}

// Two identical copies — seamless -50% marquee loop
const BRAND_STRIP = [...BRANDS, ...BRANDS];
const BRAND_STRIP_REV = [...BRANDS].reverse().concat([...BRANDS].reverse());

// ── BrandCard ───────────────────────────────────────────────────────────────
function BrandCard({ name, domain, accent }: { name: string; domain: string; accent: string }) {
  const [imgError, setImgError] = useState(false);

  return (
    <div className="group/card flex items-center gap-3 bg-white rounded-2xl border border-gray-100 shadow-sm hover:shadow-md transition-all duration-300 select-none shrink-0 px-5 py-3.5 min-w-max">
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
  { labelKey: "home.age_newborn",   rangeKey: "home.age_newborn_range",   icon: FaBaby,       bg: "#FDDDD6", color: "#D96B58", query: "newborn"   },
  { labelKey: "home.age_infant",    rangeKey: "home.age_infant_range",    icon: FaSmile,      bg: "#D4EDE8", color: "#5AAFA0", query: "infant"    },
  { labelKey: "home.age_toddler",   rangeKey: "home.age_toddler_range",   icon: GiFootprint,  bg: "#F5E6C0", color: "#B8922E", query: "toddler"   },
  { labelKey: "home.age_preschool", rangeKey: "home.age_preschool_range", icon: FaTshirt,     bg: "#FDDDE0", color: "#D96B7B", query: "preschool" },
  { labelKey: "home.age_kids",      rangeKey: "home.age_kids_range",      icon: FaChild,      bg: "#D4E8E8", color: "#5AAFAF", query: "kids"      },
];

// ── Page component ──────────────────────────────────────────────────────────
export default function HomePage() {
  const { t } = useTranslation();
  const [titleNumber, setTitleNumber] = useState(0);
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

  const steps = [
    { icon: HiCamera, titleKey: "home.step1_title", descKey: "home.step1_desc", bg: "#FDDDD6", color: "#C4705A", badge: "1" },
    { icon: HiTag,    titleKey: "home.step2_title", descKey: "home.step2_desc", bg: "#D4EDE8", color: "#4A9E8E", badge: "2" },
    { icon: HiTruck,  titleKey: "home.step3_title", descKey: "home.step3_desc", bg: "#F5E6C0", color: "#9A7A2A", badge: "3" },
    { icon: HiHeart,  titleKey: "home.step4_title", descKey: "home.step4_desc", bg: "#FDDDE0", color: "#C4607A", badge: "4" },
  ];

  return (
    <div>
      {/* ── Hero ── */}
      <section className="relative w-full overflow-hidden min-h-[560px] flex items-center">
        {/* Background image — place your own at /hero-bg.jpg or swap this URL */}
        <img
          src="/hero-bg.jpg"
          alt=""
          aria-hidden="true"
          className="absolute inset-0 w-full h-full object-cover object-center"
        />
        {/* Gradient overlay for text readability */}
        <div className="absolute inset-0 bg-gradient-to-r from-black/60 via-black/45 to-black/30" />

        <div className="relative z-10 container mx-auto px-4 w-full">
          <div className="flex gap-8 py-20 lg:py-32 items-center justify-center flex-col">
            <Link to="/browse">
              <button className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-white/20 border border-white/40 backdrop-blur-sm text-white text-sm font-medium hover:bg-white/30 transition-colors">
                {t("home.browse_btn")} <MoveRight className="w-4 h-4" />
              </button>
            </Link>

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
      <section className="bg-white dark:bg-[#201d30] py-12 overflow-hidden">
        <p className="text-xs font-semibold uppercase tracking-widest text-gray-400 text-center mb-8">
          Items from brands you know &amp; love
        </p>

        {/* Row 1 — left → right */}
        <div className="group relative w-full overflow-hidden mb-5 [--duration:40s]">
          <div className="flex w-max gap-4 px-4 animate-marquee group-hover:[animation-play-state:paused]">
            {BRAND_STRIP.map((b, i) => (
              <BrandCard key={`r1-${i}`} name={b.name} domain={b.domain} accent={b.accent} />
            ))}
          </div>
          <div className="pointer-events-none absolute inset-y-0 left-0 w-24 bg-gradient-to-r from-white dark:from-[#201d30]" />
          <div className="pointer-events-none absolute inset-y-0 right-0 w-24 bg-gradient-to-l from-white dark:from-[#201d30]" />
        </div>

        {/* Row 2 — right → left (different speed for depth) */}
        <div className="group relative w-full overflow-hidden [--duration:30s]">
          <div className="flex w-max gap-4 px-4 animate-marquee-reverse group-hover:[animation-play-state:paused]">
            {BRAND_STRIP_REV.map((b, i) => (
              <BrandCard key={`r2-${i}`} name={b.name} domain={b.domain} accent={b.accent} />
            ))}
          </div>
          <div className="pointer-events-none absolute inset-y-0 left-0 w-24 bg-gradient-to-r from-white dark:from-[#201d30]" />
          <div className="pointer-events-none absolute inset-y-0 right-0 w-24 bg-gradient-to-l from-white dark:from-[#201d30]" />
        </div>
      </section>

      {/* ── How it works ── */}
      <section className="dark-section py-40 px-4" style={{ backgroundColor: "#FAF3EE" }}>
        <div className="max-w-5xl mx-auto">
          <div className="text-center mb-16">
            <h2 className="text-3xl md:text-4xl font-bold text-primary-dark mb-3">
              {t("home.how_it_works")}
            </h2>
            <p className="text-gray-500">
              {t("home.how_it_works_subtitle")}
            </p>
          </div>

          <div className="grid grid-cols-2 md:grid-cols-4 gap-10">
            {steps.map((step, i) => (
              <div key={i} className="flex flex-col items-center text-center gap-4">
                {/* Icon box with badge */}
                <div className="relative">
                  <div
                    className="w-20 h-20 rounded-2xl flex items-center justify-center"
                    style={{ backgroundColor: step.bg }}
                  >
                    <step.icon className="w-9 h-9" style={{ color: step.color }} />
                  </div>
                  <div className="absolute -top-2 -right-2 w-6 h-6 rounded-full bg-[#E8724A] text-white text-xs font-bold flex items-center justify-center shadow-sm">
                    {step.badge}
                  </div>
                </div>
                {/* Text */}
                <div>
                  <h3 className="font-bold text-gray-800 text-base mb-2">{t(step.titleKey)}</h3>
                  <p className="text-sm text-gray-500 leading-relaxed">{t(step.descKey)}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* ── Shop by Age ── */}
      <section className="dark-section py-40 px-4" style={{ backgroundColor: "#FAF3EE" }}>
        <div className="max-w-5xl mx-auto">
          <div className="text-center mb-12">
            <h2 className="text-3xl md:text-4xl font-bold text-primary-dark mb-3">
              {t("home.shop_by_age")}
            </h2>
            <p className="text-gray-500">{t("home.shop_by_age_subtitle")}</p>
          </div>
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-5 gap-14">
            {AGE_GROUPS.map((group) => (
              <Link
                key={group.labelKey}
                to={`/browse?age=${group.query}`}
                className="bg-white rounded-2xl p-6 flex flex-col items-center gap-3 shadow-sm border border-gray-100 hover:shadow-md hover:-translate-y-1 transition-all duration-300"
              >
                <div
                  className="w-14 h-14 rounded-2xl flex items-center justify-center"
                  style={{ backgroundColor: group.bg }}
                >
                  <group.icon size={26} style={{ color: group.color }} />
                </div>
                <div className="text-center">
                  <p className="font-bold text-gray-800 text-sm">{t(group.labelKey)}</p>
                  <p className="text-xs text-gray-400 mt-0.5">{t(group.rangeKey)}</p>
                </div>
              </Link>
            ))}
          </div>
        </div>
      </section>

    </div>
  );
}
