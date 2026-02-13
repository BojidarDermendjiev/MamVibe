import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { HiCamera, HiChat, HiCreditCard } from "react-icons/hi";
import { itemsApi } from "../api/itemsApi";
import { type Item } from "../types/item";
import { useCategories } from "../hooks/useCategories";
import ItemCard from "../components/items/ItemCard";
import LoadingSpinner from "../components/common/LoadingSpinner";
import Button from "../components/common/Button";

export default function HomePage() {
  const { t } = useTranslation();
  const [featured, setFeatured] = useState<Item[]>([]);
  const { categories } = useCategories();
  const [loading, setLoading] = useState(true);

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
                likesCount: item.isLikedByCurrentUser
                  ? item.likesCount - 1
                  : item.likesCount + 1,
              }
            : item
        )
      );
    } catch {
      /* ignore */
    }
  };

  const steps = [
    {
      icon: HiCamera,
      titleKey: "home.step1_title",
      descKey: "home.step1_desc",
    },
    { icon: HiChat, titleKey: "home.step2_title", descKey: "home.step2_desc" },
    {
      icon: HiCreditCard,
      titleKey: "home.step3_title",
      descKey: "home.step3_desc",
    },
  ];

  return (
    <div>
      {/* Hero */}
      <section className="bg-primary text-white py-20 px-4">
        <div className="max-w-4xl mx-auto text-center animate-fade-in-slow">
          <h1 className="text-4xl md:text-5xl font-bold mb-4">
            {t("home.hero_title")}
          </h1>
          <p className="text-lg text-white/70 mb-8 max-w-2xl mx-auto">
            {t("home.hero_subtitle")}
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link to="/browse">
              <Button size="lg" variant="secondary">
                {t("home.browse_btn")}
              </Button>
            </Link>
            <Link to="/create">
              <Button size="lg">{t("home.create_btn")}</Button>
            </Link>
          </div>
        </div>
      </section>

      {/* Categories */}
      <section className="mx-auto px-4 py-16 bg-peach">
        <h2 className="text-2xl font-bold text-primary-dark mb-8 text-center">
          {t("home.categories")}
        </h2>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-6">
          {categories.map((cat) => (
            <Link
              key={cat.id}
              to={`/browse?category=${cat.id}`}
              className="category-card bg-static rounded-xl p-6 border border-lavender/30 text-center 
               relative overflow-hidden
               animate-stagger group
               after:absolute after:inset-0 
               after:bg-gradient-to-r after:from-transparent after:via-white/20 after:to-transparent
               after:-translate-x-full after:transition-transform after:duration-1000
               hover:after:translate-x-full"
            >
              <h3
                className="text-lg font-semibold text-primary relative z-10
                   transition-all duration-300
                   group-hover:text-lavender group-hover:scale-110"
              >
                {cat.name}
              </h3>
              <p
                className="text-sm text-text mt-1 relative z-10
                  transition-all duration-300 delay-75
                  group-hover:text-primary-dark"
              >
                {cat.description}
              </p>
            </Link>
          ))}
        </div>
      </section>

      {/* How it works */}
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

      {/* Featured items */}
      <section className="mx-auto px-4 py-16 bg-peach">
        <h2 className="text-2xl font-bold text-primary-dark mb-8 text-center">
          {t("home.featured")}
        </h2>
        {loading ? (
          <LoadingSpinner size="lg" className="py-10" />
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {featured.map((item) => (
              <ItemCard key={item.id} item={item} onLikeToggle={handleLikeToggle} />
            ))}
          </div>
        )}
        <div className="text-center mt-8">
          <Link to="/browse">
            <Button variant="secondary" size="lg">
              {t("home.browse_btn")}
            </Button>
          </Link>
        </div>
      </section>
    </div>
  );
}
