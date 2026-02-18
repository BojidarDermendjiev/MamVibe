import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { motion } from "framer-motion";
import { MoveRight, ShoppingBag } from "lucide-react";
import { HiCamera, HiChat, HiCreditCard } from "react-icons/hi";
import { itemsApi } from "../api/itemsApi";
import { type Item } from "../types/item";
import ItemCard from "../components/items/ItemCard";
import LoadingSpinner from "../components/common/LoadingSpinner";

// ── Brand data ─────────────────────────────────────────────────────────────
// Icons fetched via Google's Favicon V2 service.
// The browser only connects to www.google.com — Google fetches & caches
// the brand icon server-side, so no DNS issues with individual brand domains.
// URL: https://t2.gstatic.com/faviconV2?client=SOCIAL&type=FAVICON&fallback_opts=TYPE,SIZE,URL&url=https://DOMAIN&size=128
const BRANDS = [
  { name: "Cybex",          domain: "cybex.com",           accent: "#1a1a2e" },
  { name: "Little Dutch",   domain: "littledutch.com",     accent: "#e8734a" },
  { name: "Nuna",           domain: "nunababy.com",         accent: "#2d6a4f" },
  { name: "Bugaboo",        domain: "bugaboo.com",          accent: "#d62828" },
  { name: "Ergobaby",       domain: "ergobaby.com",         accent: "#3a7dbf" },
  { name: "UPPAbaby",       domain: "uppababy.com",         accent: "#2b2d42" },
  { name: "Joie",           domain: "joiebaby.com",         accent: "#e07b39" },
  { name: "Chicco",         domain: "chicco.com",           accent: "#0057a8" },
  { name: "Maxi-Cosi",      domain: "maxi-cosi.com",        accent: "#c1121f" },
  { name: "BabyBjörn",      domain: "babybjorn.com",        accent: "#005f73" },
  { name: "Stokke",         domain: "stokke.com",           accent: "#6a4c93" },
  { name: "Graco",          domain: "graco.com",            accent: "#e07b39" },
  { name: "Britax",         domain: "britax.com",           accent: "#c1121f" },
  { name: "Mamas & Papas",  domain: "mamasandpapas.com",    accent: "#8e6b9e" },
  { name: "Skip Hop",       domain: "skiphop.com",          accent: "#f4a261" },
  { name: "Hauck",          domain: "hauck.de",             accent: "#2b2d42" },
  { name: "Peg Perego",     domain: "pegperego.com",        accent: "#c1121f" },
  { name: "BABYZEN",        domain: "babyzen.com",          accent: "#1a1a2e" },
];

function googleFaviconUrl(domain: string, size = 128) {
  return `https://t2.gstatic.com/faviconV2?client=SOCIAL&type=FAVICON&fallback_opts=TYPE,SIZE,URL&url=https://${domain}&size=${size}`;
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

// ── Page component ──────────────────────────────────────────────────────────
export default function HomePage() {
  const { t } = useTranslation();
  const [featured, setFeatured] = useState<Item[]>([]);
  const [loading, setLoading] = useState(true);

  const [titleNumber, setTitleNumber] = useState(0);
  const titles = useMemo(
    () => ["amazing", "stylish", "vibrant", "special", "lovely"],
    [],
  );

  useEffect(() => {
    const id = setTimeout(() => {
      setTitleNumber((n) => (n === titles.length - 1 ? 0 : n + 1));
    }, 2000);
    return () => clearTimeout(id);
  }, [titleNumber, titles]);

  useEffect(() => {
    const load = async () => {
      try {
        const { data } = await itemsApi.getAll({
          page: 1,
          pageSize: 4,
          sortBy: "popular",
        });
        setFeatured(data.items);
      } catch {
        /* ignore */
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  const handleLikeToggle = async (id: string) => {
    try {
      await itemsApi.toggleLike(id);
      setFeatured((prev) =>
        prev.map((item) =>
          item.id === id
            ? {
                ...item,
                isLikedByCurrentUser: !item.isLikedByCurrentUser,
                likeCount: item.isLikedByCurrentUser
                  ? item.likeCount - 1
                  : item.likeCount + 1,
              }
            : item,
        ),
      );
    } catch {
      /* ignore */
    }
  };

  const steps = [
    { icon: HiCamera,     titleKey: "home.step1_title", descKey: "home.step1_desc" },
    { icon: HiChat,       titleKey: "home.step2_title", descKey: "home.step2_desc" },
    { icon: HiCreditCard, titleKey: "home.step3_title", descKey: "home.step3_desc" },
  ];

  return (
    <div>
      {/* ── Hero ── */}
      <section
        className="w-full bg-cream-light bg-cover bg-center"
        style={{ backgroundImage: "url('/hero-bg.jpg')" }}
      >
        <div className="container mx-auto px-4">
          <div className="flex gap-8 py-20 lg:py-32 items-center justify-center flex-col">
            <Link to="/browse">
              <button className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-peach text-primary text-sm font-medium hover:bg-peach-light transition-colors">
                {t("home.browse_btn")} <MoveRight className="w-4 h-4" />
              </button>
            </Link>

            <div className="flex gap-4 flex-col items-center">
              <h1 className="text-5xl md:text-7xl max-w-2xl tracking-tight text-center font-bold">
                <span className="text-peach">{t("home.hero_title")} </span>
                <span className="relative flex w-full justify-center overflow-hidden text-center md:pb-4 md:pt-1">
                  &nbsp;
                  {titles.map((title, index) => (
                    <motion.span
                      key={index}
                      className="absolute font-bold text-primary"
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
              <p className="text-lg md:text-xl leading-relaxed text-gray-500 max-w-2xl text-center">
                {t("home.hero_subtitle")}
              </p>
            </div>

            <div className="flex flex-row gap-3">
              <Link to="/browse">
                <button className="inline-flex items-center gap-2 px-7 py-3 rounded-lg border-2 border-primary text-primary font-semibold hover:bg-primary hover:text-white transition-all duration-300">
                  {t("home.browse_btn")} <ShoppingBag className="w-4 h-4" />
                </button>
              </Link>
              <Link to="/create">
                <button
                  className="inline-flex items-center gap-2 px-7 py-3 rounded-lg text-white font-semibold transition-all duration-300 hover:opacity-90 hover:shadow-lg"
                  style={{ background: "linear-gradient(135deg, #945c67 0%, #3f4b7f 100%)" }}
                >
                  {t("home.create_btn")} <MoveRight className="w-4 h-4" />
                </button>
              </Link>
            </div>
          </div>
        </div>
      </section>

      {/* ── Trusted Brands ── */}
      <section className="bg-white py-12 overflow-hidden">
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
          <div className="pointer-events-none absolute inset-y-0 left-0 w-24 bg-gradient-to-r from-white" />
          <div className="pointer-events-none absolute inset-y-0 right-0 w-24 bg-gradient-to-l from-white" />
        </div>

        {/* Row 2 — right → left (different speed for depth) */}
        <div className="group relative w-full overflow-hidden [--duration:30s]">
          <div className="flex w-max gap-4 px-4 animate-marquee-reverse group-hover:[animation-play-state:paused]">
            {BRAND_STRIP_REV.map((b, i) => (
              <BrandCard key={`r2-${i}`} name={b.name} domain={b.domain} accent={b.accent} />
            ))}
          </div>
          <div className="pointer-events-none absolute inset-y-0 left-0 w-24 bg-gradient-to-r from-white" />
          <div className="pointer-events-none absolute inset-y-0 right-0 w-24 bg-gradient-to-l from-white" />
        </div>
      </section>

      {/* ── How it works ── */}
      <section className="bg-primary py-16 px-4">
        <div className="max-w-5xl mx-auto">
          <h2 className="text-2xl font-bold text-white mb-10 text-center">
            {t("home.how_it_works")}
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            {steps.map((step, i) => (
              <div key={i} className="text-center animate-stagger">
                <div className="w-16 h-16 bg-white/15 rounded-2xl flex items-center justify-center mx-auto mb-4">
                  <step.icon className="h-8 w-8 text-peach-light" />
                </div>
                <h3 className="font-semibold text-peach-light mb-2">
                  {t(step.titleKey)}
                </h3>
                <p className="text-sm text-white/70">{t(step.descKey)}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* ── Featured items ── */}
      <section className="mx-auto px-4 py-16 bg-peach">
        <h2 className="text-2xl font-bold text-primary-dark mb-8 text-center">
          {t("home.featured")}
        </h2>
        {loading ? (
          <LoadingSpinner size="lg" className="py-10" />
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {featured.map((item) => (
              <ItemCard
                key={item.id}
                item={item}
                onLikeToggle={handleLikeToggle}
              />
            ))}
          </div>
        )}
        <div className="text-center mt-8">
          <Link to="/browse">
            <button className="inline-flex items-center gap-2 px-7 py-3 rounded-lg border-2 border-primary text-primary font-semibold hover:bg-primary hover:text-white transition-all duration-300">
              {t("home.browse_btn")}
            </button>
          </Link>
        </div>
      </section>
    </div>
  );
}
