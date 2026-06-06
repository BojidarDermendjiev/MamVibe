import { type InputHTMLAttributes, type ReactNode, forwardRef, useState } from 'react';
import { clsx } from 'clsx';
import { Eye, EyeOff } from 'lucide-react';

interface InputProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'prefix'> {
  label?: string;
  error?: string;
  /** Optional adornment rendered inside the left edge of the input (e.g. a "€" sign for price fields). */
  prefix?: ReactNode;
}

const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, className, type, prefix, ...props }, ref) => {
    const [showPassword, setShowPassword] = useState(false);
    const isPassword = type === 'password';

    return (
      <div className="w-full">
        {label && (
          <label className="block text-sm font-medium text-primary mb-1">
            {label}
          </label>
        )}
        <div className="relative">
          {prefix && (
            <span className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-gray-500 text-sm select-none">
              {prefix}
            </span>
          )}
          <input
            ref={ref}
            type={isPassword && showPassword ? 'text' : type}
            className={clsx(
              'w-full py-2.5 rounded-lg border bg-white text-gray-800 placeholder-gray-400 transition-colors duration-200',
              prefix ? 'pl-9 pr-4' : 'px-4',
              'focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent',
              error ? 'border-red-400' : 'border-lavender',
              isPassword && 'pr-11',
              className
            )}
            {...props}
          />
          {isPassword && (
            <button
              type="button"
              onClick={() => setShowPassword((v) => !v)}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 transition-colors"
              tabIndex={-1}
            >
              {showPassword ? <EyeOff size={20} /> : <Eye size={20} />}
            </button>
          )}
        </div>
        {error && (
          <p className="mt-1 text-sm text-red-500">{error}</p>
        )}
      </div>
    );
  }
);

Input.displayName = 'Input';
export default Input;
