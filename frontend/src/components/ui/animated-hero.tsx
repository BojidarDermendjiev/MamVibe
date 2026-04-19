import { useEffect, useMemo, useState } from "react";
import { AnimatePresence, motion } from "framer-motion";
import { Heart, ArrowRight } from "lucide-react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { RippleButton } from "@/components/ui/ripple-button";

export function AnimatedHero() {
  const { t } = useTranslation();

  const words = useMemo(
    () => [
      t("home.hero_word_1"),
      t("home.hero_word_2"),
      t("home.hero_word_3"),
      t("home.hero_word_4"),
      t("home.hero_word_5"),
    ],
    [t],
  );

  const [currentWord, setCurrentWord] = useState(0);

  useEffect(() => {
    const interval = setInterval(() => {
      setCurrentWord((prev) => (prev + 1) % words.length);
    }, 2000);
    return () => clearInterval(interval);
  }, [words.length]);

  return (
    <section className="relative overflow-hidden bg-gradient-to-b from-primary to-primary-dark py-20 px-4">
      {/* Decorative blobs */}
      <div className="pointer-events-none absolute inset-0 overflow-hidden">
        <div className="absolute -top-24 -right-24 h-96 w-96 rounded-full bg-lavender/10 blur-3xl" />
        <div className="absolute -bottom-24 -left-24 h-96 w-96 rounded-full bg-peach/10 blur-3xl" />
      </div>

      <div className="relative max-w-4xl mx-auto text-center">
        {/* Badge */}
        <motion.div
          initial={{ opacity: 0, y: -10 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5 }}
          className="inline-flex items-center gap-2 rounded-full border border-white/10 bg-white/10 px-4 py-1.5 mb-8 backdrop-blur-sm"
        >
          <Heart className="h-4 w-4 text-peach-light fill-peach-light" />
          <span className="text-sm font-medium text-peach-light">
            MamVibe Community
          </span>
        </motion.div>

        {/* Title with cycling word */}
        <motion.h1
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6, delay: 0.1 }}
          className="text-4xl md:text-5xl lg:text-6xl font-bold text-white mb-6 leading-tight"
        >
          {t("home.hero_title")}
          <br />
          <span className="inline-flex items-center gap-3">
            <span className="text-white/80">{t("home.hero_static")}</span>{" "}
            <AnimatePresence mode="wait">
              <motion.span
                key={currentWord}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                exit={{ opacity: 0, y: -20 }}
                transition={{ duration: 0.3 }}
                className="inline-block text-peach-light"
              >
                {words[currentWord]}
              </motion.span>
            </AnimatePresence>
          </span>
        </motion.h1>

        {/* Subtitle */}
        <motion.p
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6, delay: 0.2 }}
          className="text-lg text-white/70 mb-10 max-w-2xl mx-auto"
        >
          {t("home.hero_subtitle")}
        </motion.p>

        {/* CTA Buttons */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6, delay: 0.3 }}
          className="flex flex-col sm:flex-row gap-4 justify-center"
        >
          <Link to="/browse">
            <RippleButton
              rippleColor="#c1c4e3"
              className="border-white/30 bg-transparent text-white hover:bg-white/10 px-7 py-3 text-base font-semibold gap-2"
            >
              {t("home.browse_btn")}
              <ArrowRight className="h-4 w-4" />
            </RippleButton>
          </Link>
          <Link to="/create">
            <RippleButton
              rippleColor="#945c67"
              className="border-peach-light bg-peach-light text-primary hover:bg-peach hover:border-peach px-7 py-3 text-base font-semibold gap-2"
            >
              {t("home.create_btn")}
              <Heart className="h-4 w-4" />
            </RippleButton>
          </Link>
        </motion.div>
      </div>
    </section>
  );
}
