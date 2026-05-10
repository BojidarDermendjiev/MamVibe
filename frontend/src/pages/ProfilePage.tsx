import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { HiPencil } from 'react-icons/hi';
import { useAuthStore } from '../store/authStore';
import Avatar from '../components/common/Avatar';
import Button from '../components/common/Button';
import { usePageSEO } from '@/hooks/useSEO';

export default function ProfilePage() {
  const { t } = useTranslation();
  const { user } = useAuthStore();

  // Noindex: private profile page, not a public SEO landing target.
  usePageSEO({ title: "My Profile", description: "View and edit your MamVibe profile.", index: false });

  if (!user) return null;

  return (
    <div className="max-w-2xl mx-auto px-4 py-8 animate-fade-in">
      <div className="bg-white rounded-xl p-8 border border-lavender/30 text-center shadow-sm hover:shadow-md transition-shadow duration-300">
        <Avatar src={user.avatarUrl} profileType={user.profileType} size="lg" className="mx-auto mb-4" />
        <h1 className="text-2xl font-bold text-primary">{user.displayName}</h1>
        <p className="text-gray-500 mt-1">{user.email}</p>
        {user.bio && <p className="text-gray-600 mt-4 max-w-md mx-auto">{user.bio}</p>}
        <div className="mt-6">
          <Link to="/settings">
            <Button variant="secondary">
              <HiPencil className="h-4 w-4 mr-2" /> {t('profile.edit')}
            </Button>
          </Link>
        </div>
      </div>
    </div>
  );
}
