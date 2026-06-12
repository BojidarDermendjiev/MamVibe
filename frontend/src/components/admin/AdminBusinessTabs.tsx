import { NavLink } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { clsx } from "clsx";
import { Users, LayoutGrid, MessageSquare, CircleDollarSign } from "lucide-react";

const TABS = [
  { to: "/admin/business/profiles", icon: Users, key: "admin.business.tabs.profiles" },
  { to: "/admin/business/listings", icon: LayoutGrid, key: "admin.business.tabs.listings" },
  { to: "/admin/business/referrals", icon: MessageSquare, key: "admin.business.tabs.referrals" },
  { to: "/admin/business/revenue", icon: CircleDollarSign, key: "admin.business.tabs.revenue" },
];

/**
 * Tab strip rendered at the top of every <c>/admin/business/*</c> page. Keeps the four
 * sub-pages visually grouped under a single "Business" sidebar entry so the existing
 * <c>AdminLayout</c> nav doesn't get cluttered with four new items.
 */
export default function AdminBusinessTabs() {
  const { t } = useTranslation();
  return (
    <div className="bg-[#2d2a42] border border-white/10 rounded-xl p-1 flex flex-wrap gap-1 mb-6">
      {TABS.map(({ to, icon: Icon, key }) => (
        <NavLink
          key={to}
          to={to}
          end={false}
          className={({ isActive }) =>
            clsx(
              "flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-colors",
              isActive
                ? "bg-white/20 text-white"
                : "text-lavender-light hover:bg-white/10 hover:text-white",
            )
          }
        >
          <Icon className="h-4 w-4" />
          {t(key)}
        </NavLink>
      ))}
    </div>
  );
}
