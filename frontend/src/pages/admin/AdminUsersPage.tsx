import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import toast from 'react-hot-toast';
import { adminApi, type AdminUser } from '../../api/adminApi';
import Avatar from '../../components/common/Avatar';
import Button from '../../components/common/Button';
import LoadingSpinner from '../../components/common/LoadingSpinner';

export default function AdminUsersPage() {
  const { t } = useTranslation();
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(true);

  const fetchUsers = async (query?: string) => {
    setLoading(true);
    try {
      const { data } = await adminApi.getUsers(query || undefined);
      setUsers(data.items);
    } catch { /* ignore */ } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    let cancelled = false;
    (async () => {
      setLoading(true);
      try {
        const { data } = await adminApi.getUsers();
        if (!cancelled) setUsers(data.items);
      } catch { /* ignore */ } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => { cancelled = true; };
  }, []);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    fetchUsers(search);
  };

  const toggleBlock = async (userId: string, isBlocked: boolean) => {
    try {
      if (isBlocked) {
        await adminApi.unblockUser(userId);
      } else {
        await adminApi.blockUser(userId);
      }
      setUsers((prev) =>
        prev.map((u) => u.id === userId ? { ...u, isBlocked: !isBlocked } : u)
      );
      toast.success(isBlocked ? 'User unblocked' : 'User blocked');
    } catch {
      toast.error(t('common.error'));
    }
  };

  return (
    <div>
      <h1 className="text-3xl font-bold text-[#364153] dark:text-[#bdb9bc] mb-6">{t('admin.users')}</h1>

      <form onSubmit={handleSearch} className="mb-6">
        <input
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder={t('admin.search_users')}
          className="w-full max-w-md px-4 py-2.5 rounded-lg border border-lavender bg-white dark:bg-[#2a2740] dark:border-white/10 dark:text-gray-100 dark:placeholder-gray-500 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
        />
      </form>

      {loading ? (
        <LoadingSpinner size="lg" className="py-20" />
      ) : (
        <div className="bg-white dark:bg-[#2d2a42] rounded-xl border border-lavender/30 dark:border-white/10 overflow-hidden">
          <table className="w-full">
            <thead>
              <tr className="bg-cream-dark dark:bg-[#3a3758] text-left">
                <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">User</th>
                <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">Email</th>
                <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">Roles</th>
                <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">Status</th>
                <th className="px-4 py-3 text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-lavender/20 dark:divide-white/10">
              {users.map((u) => (
                <tr key={u.id} className="hover:bg-cream/50 dark:hover:bg-white/5">
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-3">
                      <Avatar src={u.avatarUrl} size="sm" />
                      <span className="text-sm font-medium text-[#364153] dark:text-[#bdb9bc]">{u.displayName}</span>
                    </div>
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-500 dark:text-gray-400">{u.email}</td>
                  <td className="px-4 py-3 text-sm text-gray-500 dark:text-gray-400">{u.roles.join(', ')}</td>
                  <td className="px-4 py-3">
                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                      u.isBlocked ? 'bg-red-100 text-red-600' : 'bg-green-100 text-green-600'
                    }`}>
                      {u.isBlocked ? 'Blocked' : 'Active'}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <Button
                      size="sm"
                      variant={u.isBlocked ? 'secondary' : 'danger'}
                      onClick={() => toggleBlock(u.id, u.isBlocked)}
                    >
                      {u.isBlocked ? t('admin.unblock_user') : t('admin.block_user')}
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
