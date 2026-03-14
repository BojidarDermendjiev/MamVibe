import _toast, { type Toast, type ToastOptions } from 'react-hot-toast';
import { clsx } from 'clsx';
import {
  HiCheckCircle,
  HiXCircle,
  HiExclamation,
  HiInformationCircle,
  HiX,
} from 'react-icons/hi';

type ToastVariant = 'success' | 'error' | 'warning' | 'info';

function ToastCard({
  t,
  variant,
  title,
  message,
}: {
  t: Toast;
  variant: ToastVariant;
  title: string;
  message?: string;
}) {
  return (
    <div
      role="alert"
      className={clsx(
        'relative flex items-start gap-3 w-[340px] max-w-[90vw] rounded-xl shadow-xl overflow-hidden',
        'bg-white dark:bg-[#2d2a42] border border-gray-100 dark:border-white/10 border-l-4',
        'px-4 py-3.5 transition-all duration-300',
        t.visible ? 'opacity-100 translate-x-0' : 'opacity-0 translate-x-2',
        {
          'border-l-emerald-400': variant === 'success',
          'border-l-red-400': variant === 'error',
          'border-l-amber-400': variant === 'warning',
          'border-l-[#945c67]': variant === 'info',
        },
      )}
    >
      {/* Icon */}
      <div
        className={clsx(
          'flex-shrink-0 w-9 h-9 rounded-full flex items-center justify-center mt-0.5',
          {
            'bg-emerald-50 dark:bg-emerald-950/40': variant === 'success',
            'bg-red-50 dark:bg-red-950/40': variant === 'error',
            'bg-amber-50 dark:bg-amber-950/40': variant === 'warning',
            'bg-[#f0d0c7] dark:bg-[#945c67]/20': variant === 'info',
          },
        )}
      >
        {variant === 'success' && <HiCheckCircle className="w-5 h-5 text-emerald-500" />}
        {variant === 'error' && <HiXCircle className="w-5 h-5 text-red-500" />}
        {variant === 'warning' && <HiExclamation className="w-5 h-5 text-amber-500" />}
        {variant === 'info' && <HiInformationCircle className="w-5 h-5 text-[#945c67]" />}
      </div>

      {/* Text */}
      <div className="flex-1 min-w-0 pt-0.5">
        <p
          className={clsx('text-sm font-semibold leading-snug', {
            'text-emerald-700 dark:text-emerald-400': variant === 'success',
            'text-red-700 dark:text-red-400': variant === 'error',
            'text-amber-700 dark:text-amber-400': variant === 'warning',
            'text-[#3f4b7f] dark:text-[#c1c4e3]': variant === 'info',
          })}
        >
          {title}
        </p>
        {message && (
          <p className="text-xs text-gray-500 dark:text-gray-400 mt-0.5 leading-relaxed">
            {message}
          </p>
        )}
      </div>

      {/* Dismiss */}
      <button
        onClick={() => _toast.dismiss(t.id)}
        aria-label="Dismiss"
        className="flex-shrink-0 ml-1 p-0.5 rounded text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 transition-colors"
      >
        <HiX className="w-4 h-4" />
      </button>

      {/* Progress bar */}
      <span
        className={clsx('absolute bottom-0 left-0 h-[3px] toast-progress', {
          'bg-emerald-400': variant === 'success',
          'bg-red-400': variant === 'error',
          'bg-amber-400': variant === 'warning',
          'bg-[#945c67]': variant === 'info',
        })}
        style={{ '--toast-duration': `${t.duration ?? 3500}ms` } as React.CSSProperties}
      />
    </div>
  );
}

function showToast(variant: ToastVariant, title: string, message?: string, options?: ToastOptions) {
  return _toast.custom(
    (t) => <ToastCard t={t} variant={variant} title={title} message={message} />,
    { duration: 3500, ...options },
  );
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const toast: typeof _toast & {
  warning: (message: string, options?: ToastOptions) => string;
  info: (message: string, options?: ToastOptions) => string;
} = Object.assign(
  // Pass-through for existing toast(fn, opts) usage
  (msg: any, opts?: any) => _toast(msg, opts),
  {
    success: (message: string, opts?: ToastOptions) => showToast('success', message, undefined, opts),
    error:   (message: string, opts?: ToastOptions) => showToast('error',   message, undefined, opts),
    warning: (message: string, opts?: ToastOptions) => showToast('warning', message, undefined, opts),
    info:    (message: string, opts?: ToastOptions) => showToast('info',    message, undefined, opts),
    dismiss: _toast.dismiss,
    remove:  _toast.remove,
    custom:  _toast.custom,
    promise: _toast.promise,
  },
) as any;

export default toast;

/** Enhanced helpers with optional subtitle */
export const toastSuccess = (title: string, message?: string, opts?: ToastOptions) =>
  showToast('success', title, message, opts);
export const toastError = (title: string, message?: string, opts?: ToastOptions) =>
  showToast('error', title, message, opts);
export const toastWarning = (title: string, message?: string, opts?: ToastOptions) =>
  showToast('warning', title, message, opts);
export const toastInfo = (title: string, message?: string, opts?: ToastOptions) =>
  showToast('info', title, message, opts);
