import { type ButtonHTMLAttributes, type MouseEvent, useEffect, useState } from 'react';
import { clsx } from 'clsx';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'danger' | 'ghost';
  size?: 'sm' | 'md' | 'lg';
  isLoading?: boolean;
  fullWidth?: boolean;
  rippleColor?: string;
}

export default function Button({
  children,
  variant = 'primary',
  size = 'md',
  isLoading = false,
  fullWidth = false,
  rippleColor,
  className,
  disabled,
  onClick,
  ...props
}: ButtonProps) {
  const [ripples, setRipples] = useState<
    Array<{ x: number; y: number; size: number; key: number }>
  >([]);

  const handleClick = (event: MouseEvent<HTMLButtonElement>) => {
    const button = event.currentTarget;
    const rect = button.getBoundingClientRect();
    const size = Math.max(rect.width, rect.height);
    const x = event.clientX - rect.left - size / 2;
    const y = event.clientY - rect.top - size / 2;
    setRipples((prev) => [...prev, { x, y, size, key: Date.now() }]);
    onClick?.(event);
  };

  useEffect(() => {
    if (ripples.length === 0) return;
    const last = ripples[ripples.length - 1];
    const id = setTimeout(() => {
      setRipples((prev) => prev.filter((r) => r.key !== last.key));
    }, 600);
    return () => clearTimeout(id);
  }, [ripples]);

  // Pick a ripple colour that contrasts with the variant
  const defaultRippleColor =
    variant === 'secondary' || variant === 'ghost' ? '#945c67' : '#ffffff';
  const color = rippleColor ?? defaultRippleColor;

  return (
    <button
      className={clsx(
        'relative overflow-hidden inline-flex items-center justify-center rounded-lg font-medium transition-all duration-300 focus:outline-none focus:ring-2 focus:ring-offset-2 hover:scale-[1.03] active:scale-[0.97] cursor-pointer',
        {
          'bg-primary text-white hover:bg-primary-dark hover:shadow-lg hover:shadow-primary-dark/25 focus:ring-primary': variant === 'primary',
          'bg-peach-light text-primary hover:bg-lavender hover:text-primary-dark hover:shadow-md hover:shadow-lavender/30 focus:ring-lavender': variant === 'secondary',
          'bg-red-500 text-white hover:bg-red-600 hover:shadow-lg hover:shadow-red-500/25 focus:ring-red-500': variant === 'danger',
          'bg-transparent text-primary border border-lavender/30 hover:bg-lavender/20 hover:text-primary-dark focus:ring-primary': variant === 'ghost',
          'px-3 py-1.5 text-sm': size === 'sm',
          'px-5 py-2.5 text-base': size === 'md',
          'px-7 py-3 text-lg': size === 'lg',
          'w-full': fullWidth,
          'opacity-50 cursor-not-allowed': disabled || isLoading,
        },
        className
      )}
      disabled={disabled || isLoading}
      onClick={handleClick}
      {...props}
    >
      {/* Content */}
      <span className="relative z-10 inline-flex items-center gap-2">
        {isLoading && (
          <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
          </svg>
        )}
        {children}
      </span>

      {/* Ripple layer */}
      <span className="pointer-events-none absolute inset-0">
        {ripples.map((ripple) => (
          <span
            key={ripple.key}
            className="absolute rounded-full animate-rippling opacity-30"
            style={{
              width: ripple.size,
              height: ripple.size,
              top: ripple.y,
              left: ripple.x,
              backgroundColor: color,
              transform: 'scale(0)',
            }}
          />
        ))}
      </span>
    </button>
  );
}
