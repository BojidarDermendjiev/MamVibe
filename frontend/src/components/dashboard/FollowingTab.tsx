import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import type { FollowUserDto } from '@/types/follow';
import Avatar from '@/components/common/Avatar';
import TabErrorState from './TabErrorState';

interface FollowingTabProps {
  users: FollowUserDto[];
  error: string | null;
  onRetry: () => void;
  emptyKey: string;
  panelId: string;
  tabId: string;
}

export default function FollowingTab({ users, error, onRetry, emptyKey, panelId, tabId }: FollowingTabProps) {
  const { t } = useTranslation();

  if (error) return <TabErrorState onRetry={onRetry} />;

  return (
    <div role="tabpanel" id={panelId} aria-labelledby={tabId}>
      {users.length === 0 ? (
        <p className="text-center py-20 text-gray-400">{t(emptyKey)}</p>
      ) : (
        <div className="space-y-3">
          {users.map((u) => (
            <div key={u.id} className="bg-white dark:bg-[#2d2a42] rounded-xl p-4 border border-lavender/30 flex items-center gap-3">
              <Avatar src={u.avatarUrl} size="md" />
              <div className="flex-1 min-w-0">
                <p className="font-medium text-primary">{u.displayName}</p>
                <p className="text-xs text-gray-400 mt-0.5">
                  {t('follow.follower_count', { count: u.followerCount })} · {t('follow.item_count', { count: u.itemCount })}
                </p>
              </div>
              <Link to={`/profile/${u.id}`} className="text-xs text-mauve hover:underline flex-shrink-0">
                {t('follow.view_profile')}
              </Link>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
