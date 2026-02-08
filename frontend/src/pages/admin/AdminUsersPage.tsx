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

  const fetchUsers = async () => {
    setLoading(true);
    try {
      const { data } = await adminApi.getUsers(search || undefined);
      setUsers(data.items);
    } catch { /* ignore */ } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchUsers(); }, []);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    fetchUsers();
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
      <h1 className="text-3xl font-bold text-primary mb-6">{t('admin.users')}</h1>

      <form onSubmit={handleSearch} className="mb-6">
        <input
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder={t('admin.search_users')}
          className="w-full max-w-md px-4 py-2.5 rounded-lg border border-lavender bg-white text-sm focus:outline-none focus:ring-2 focus:ring-primary"
        />
      </form>

      {loading ? (
        <LoadingSpinner size="lg" className="py-20" />
      ) : (
        <div className="bg-white rounded-xl border border-lavender/30 overflow-hidden">
          <table className="w-full">
            <thead>
              <tr className="bg-cream-dark text-left">
                <th className="px-4 py-3 text-sm font-medium text-primary">User</th>
                <th className="px-4 py-3 text-sm font-medium text-primary">Email</th>
                <th className="px-4 py-3 text-sm font-medium text-primary">Roles</th>
                <th className="px-4 py-3 text-sm font-medium text-primary">Status</th>
                <th className="px-4 py-3 text-sm font-medium text-primary">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-lavender/20">
              {users.map((u) => (
                <tr key={u.id} className="hover:bg-cream/50">
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-3">
                      <Avatar src={u.avatarUrl} size="sm" />
                      <span className="text-sm font-medium text-primary">{u.displayName}</span>
                    </div>
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-500">{u.email}</td>
                  <td className="px-4 py-3 text-sm text-gray-500">{u.roles.join(', ')}</td>
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
