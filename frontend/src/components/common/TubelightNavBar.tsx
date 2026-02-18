import { useEffect, useState } from "react";
import { Link, useLocation } from "react-router-dom";
import { motion } from "framer-motion";
import { type LucideIcon } from "lucide-react";
import { clsx } from "clsx";

export interface NavItem {
  name: string;
  url: string;
  icon: LucideIcon;
  badge?: number;
}

interface TubelightNavBarProps {
  items: NavItem[];
  className?: string;
}

export function TubelightNavBar({ items, className }: TubelightNavBarProps) {
  const location = useLocation();

  const getActive = () => {
    const match = items.find(
      (item) =>
        (item.url === "/" && location.pathname === "/") ||
        (item.url !== "/" && location.pathname.startsWith(item.url))
    );
    return match?.name ?? items[0].name;
  };

  const [activeTab, setActiveTab] = useState(getActive);

  useEffect(() => {
    setActiveTab(getActive());
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [location.pathname]);

  return (
    <div
      className={clsx(
        "fixed bottom-0 sm:top-14 left-1/2 -translate-x-1/2 z-40 mb-4 sm:mb-0 sm:pt-3",
        className
      )}
    >
      <div className="flex items-center gap-1 bg-white/90 border border-lavender/40 backdrop-blur-lg py-1 px-1 rounded-full shadow-lg">
        {items.map((item) => {
          const Icon = item.icon;
          const isActive = activeTab === item.name;

          return (
            <Link
              key={item.name}
              to={item.url}
              onClick={() => setActiveTab(item.name)}
              className={clsx(
                "relative cursor-pointer text-sm font-semibold px-5 py-2 rounded-full transition-colors whitespace-nowrap",
                "text-primary-dark/60 hover:text-primary",
                isActive && "text-primary"
              )}
            >
              {/* Desktop: text label */}
              <span className="hidden md:inline">{item.name}</span>

              {/* Mobile: icon */}
              <span className="md:hidden relative">
                <Icon size={18} strokeWidth={2.5} />
                {!!item.badge && item.badge > 0 && (
                  <span className="absolute -top-1.5 -right-1.5 bg-red-500 text-white text-[9px] font-bold rounded-full h-3.5 min-w-3.5 flex items-center justify-center px-0.5 leading-none">
                    {item.badge > 9 ? "9+" : item.badge}
                  </span>
                )}
              </span>

              {/* Desktop badge dot */}
              {!!item.badge && item.badge > 0 && (
                <span className="hidden md:flex absolute -top-0.5 -right-0.5 bg-red-500 text-white text-[9px] font-bold rounded-full h-3.5 min-w-3.5 items-center justify-center px-0.5 leading-none">
                  {item.badge > 9 ? "9+" : item.badge}
                </span>
              )}

              {isActive && (
                <motion.div
                  layoutId="lamp"
                  className="absolute inset-0 w-full bg-primary/10 rounded-full -z-10"
                  initial={false}
                  transition={{ type: "spring", stiffness: 300, damping: 30 }}
                >
                  {/* Tubelight glow */}
                  <div className="absolute -top-2 left-1/2 -translate-x-1/2 w-8 h-1 bg-primary rounded-t-full">
                    <div className="absolute w-12 h-6 bg-primary/20 rounded-full blur-md -top-2 -left-2" />
                    <div className="absolute w-8 h-6 bg-primary/20 rounded-full blur-md -top-1" />
                    <div className="absolute w-4 h-4 bg-primary/20 rounded-full blur-sm top-0 left-2" />
                  </div>
                </motion.div>
              )}
            </Link>
          );
        })}
      </div>
    </div>
  );
}
