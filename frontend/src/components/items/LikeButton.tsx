import { useState } from 'react';
import { HiHeart, HiOutlineHeart } from 'react-icons/hi';
import { clsx } from 'clsx';

interface LikeButtonProps {
  itemId: string;
  likeCount: number;
  isLiked: boolean;
  onToggle?: (id: string) => void;
  size?: 'sm' | 'md';
}

export default function LikeButton({ itemId, likeCount, isLiked, onToggle, size = 'md' }: LikeButtonProps) {
  const [liked, setLiked] = useState(isLiked);
  const [count, setCount] = useState(likeCount);
  const [animating, setAnimating] = useState(false);

  const handleClick = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setLiked(!liked);
    setCount(liked ? count - 1 : count + 1);
    setAnimating(true);
    setTimeout(() => setAnimating(false), 400);
    onToggle?.(itemId);
  };

  return (
    <button
      onClick={handleClick}
      className={clsx(
        'flex items-center gap-1 transition-all duration-200 hover:scale-110',
        liked ? 'text-primary' : 'text-lavender hover:text-primary'
      )}
    >
      {liked ? (
        <HiHeart className={clsx(
          animating && 'animate-like-bounce',
          size === 'sm' ? 'h-4 w-4' : 'h-6 w-6'
        )} />
      ) : (
        <HiOutlineHeart className={size === 'sm' ? 'h-4 w-4' : 'h-6 w-6'} />
      )}
      <span className={size === 'sm' ? 'text-sm' : 'text-base'}>{count}</span>
    </button>
  );
}
