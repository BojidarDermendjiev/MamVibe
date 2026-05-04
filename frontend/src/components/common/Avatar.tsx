import { clsx } from 'clsx';
import { ProfileType } from '../../types/auth';

interface AvatarProps {
  src?: string | null;
  profileType?: ProfileType;
  size?: 'sm' | 'md' | 'lg';
  className?: string;
}

const defaultAvatars: Record<number, string> = {
  [ProfileType.Female]: '/avatars/mom.svg',
  [ProfileType.Male]: '/avatars/dad.svg',
  [ProfileType.Family]: '/avatars/family.svg',
};

export default function Avatar({ src, profileType, size = 'md', className }: AvatarProps) {
  const sizeClasses = {
    sm: 'h-8 w-8',
    md: 'h-10 w-10',
    lg: 'h-16 w-16',
  };

  const avatarSrc = src || (profileType != null ? defaultAvatars[profileType] : null);

  if (avatarSrc) {
    const fallbackSrc = profileType != null ? defaultAvatars[profileType] : '/avatars/family.svg';
    return (
      <img
        src={avatarSrc}
        alt="Avatar"
        className={clsx('rounded-full object-cover', sizeClasses[size], className)}
        onError={(e) => {
          if (e.currentTarget.src !== fallbackSrc) {
            e.currentTarget.src = fallbackSrc;
          }
        }}
      />
    );
  }

  // Fallback if no profileType either
  return (
    <div
      className={clsx(
        'rounded-full flex items-center justify-center bg-mauve/20',
        sizeClasses[size],
        className
      )}
    >
      <img src="/avatars/family.svg" alt="Avatar" className="rounded-full w-full h-full" />
    </div>
  );
}
