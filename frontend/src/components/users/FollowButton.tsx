import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { followsApi } from '../../api/followsApi';
import { useAuthStore } from '../../store/authStore';

interface FollowButtonProps {
  userId: string;
  onRequireAuth?: () => void;
}

export default function FollowButton({ userId, onRequireAuth }: FollowButtonProps) {
  const { t } = useTranslation();
  const { user } = useAuthStore();
  const [isFollowing, setIsFollowing] = useState(false);
  const [followerCount, setFollowerCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [statusLoaded, setStatusLoaded] = useState(false);

  useEffect(() => {
    if (!user || user.id === userId) return;
    followsApi.getStatus(userId)
      .then(({ data }) => {
        setIsFollowing(data.isFollowing);
        setFollowerCount(data.followerCount);
        setStatusLoaded(true);
      })
      .catch(() => { setStatusLoaded(true); });
  }, [user, userId]);

  if (!user || user.id === userId) return null;

  const handleClick = async () => {
    if (!user) { onRequireAuth?.(); return; }
    setLoading(true);
    try {
      const { data } = await followsApi.toggle(userId);
      setIsFollowing(data.isFollowing);
      setFollowerCount(data.followerCount);
    } catch {
      // ignore
    } finally {
      setLoading(false);
    }
  };

  if (!statusLoaded) return null;

  return (
    <button
      onClick={handleClick}
      disabled={loading}
      className={`flex items-center gap-1.5 px-3 py-1.5 rounded-full text-sm font-medium transition-all ${
        isFollowing
          ? 'bg-mauve text-white hover:bg-mauve/80'
          : 'bg-white dark:bg-white/10 border border-mauve text-mauve hover:bg-mauve hover:text-white dark:hover:bg-mauve'
      } disabled:opacity-60`}
    >
      {isFollowing ? t('follow.following') : t('follow.follow')}
      {followerCount > 0 && (
        <span className="text-xs opacity-75">{followerCount}</span>
      )}
    </button>
  );
}
