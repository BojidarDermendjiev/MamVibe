import { cn } from "@/lib/utils";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";

interface TestimonialCardProps {
  author: string;
  content: string;
  rating: number;
  avatarUrl?: string | null;
  className?: string;
}

export function TestimonialCard({
  author,
  content,
  rating,
  avatarUrl,
  className,
}: TestimonialCardProps) {
  const initials = author
    .split(" ")
    .map((n) => n[0])
    .join("")
    .slice(0, 2)
    .toUpperCase();

  return (
    <div
      className={cn(
        "flex w-[320px] shrink-0 flex-col gap-4 rounded-xl border border-lavender/20 bg-gradient-to-br from-lavender/10 to-transparent p-6 shadow-sm",
        className,
      )}
    >
      {/* Stars */}
      <div className="flex gap-0.5">
        {Array.from({ length: 5 }).map((_, i) => (
          <svg
            key={i}
            className={cn(
              "h-4 w-4",
              i < rating ? "text-amber-400 fill-amber-400" : "text-gray-300",
            )}
            viewBox="0 0 20 20"
            fill="currentColor"
          >
            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
          </svg>
        ))}
      </div>

      {/* Content */}
      <p className="text-sm text-gray-600 leading-relaxed line-clamp-4">
        {content}
      </p>

      {/* Author */}
      <div className="mt-auto flex items-center gap-3 pt-2 border-t border-lavender/10">
        <Avatar className="h-8 w-8">
          {avatarUrl && <AvatarImage src={avatarUrl} alt={author} />}
          <AvatarFallback className="text-xs bg-primary/10 text-primary">
            {initials}
          </AvatarFallback>
        </Avatar>
        <span className="text-sm font-medium text-gray-900 dark:text-white">{author}</span>
      </div>
    </div>
  );
}
