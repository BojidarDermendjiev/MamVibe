export default function LoadingPage() {
  return (
    <div className="min-h-screen flex flex-col items-center justify-center bg-white dark:bg-[#1a1825] transition-colors duration-300 gap-6">
      <div className="flex flex-col items-center gap-3">
        <img
          src="/logo.png"
          alt="MamVibe"
          className="h-14 w-14 object-contain animate-pulse-soft"
        />
        <span className="text-base font-bold text-gray-700 dark:text-gray-200 tracking-wide">
          MamVibe
        </span>
      </div>

      <p className="shimmer-text text-xl font-semibold tracking-widest select-none">
        Loading data...
      </p>

      <div className="flex items-center gap-1.5">
        <span className="h-1.5 w-1.5 rounded-full bg-primary animate-bounce [animation-delay:0ms]" />
        <span className="h-1.5 w-1.5 rounded-full bg-primary animate-bounce [animation-delay:150ms]" />
        <span className="h-1.5 w-1.5 rounded-full bg-primary animate-bounce [animation-delay:300ms]" />
      </div>
    </div>
  );
}
