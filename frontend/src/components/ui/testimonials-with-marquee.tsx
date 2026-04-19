import { cn } from "@/lib/utils";
import { TestimonialCard } from "@/components/ui/testimonial-card";

interface Testimonial {
  author: string;
  content: string;
  rating: number;
  avatarUrl?: string | null;
}

interface TestimonialsWithMarqueeProps {
  title: string;
  description: string;
  testimonials: Testimonial[];
  className?: string;
}

export function TestimonialsWithMarquee({
  title,
  description,
  testimonials,
  className,
}: TestimonialsWithMarqueeProps) {
  if (testimonials.length === 0) return null;

  // Split testimonials into two rows
  const mid = Math.ceil(testimonials.length / 2);
  const topRow = testimonials.slice(0, mid);
  const bottomRow = testimonials.slice(mid);

  return (
    <section className={cn("py-16 overflow-hidden", className)}>
      {/* Header */}
      <div className="max-w-4xl mx-auto text-center px-4 mb-12">
        <h2 className="text-2xl font-bold text-primary-dark mb-3">{title}</h2>
        <p className="text-text/70">{description}</p>
      </div>

      {/* Marquee rows */}
      <div className="relative flex flex-col gap-6">
        {/* Top row — scrolls left */}
        <MarqueeRow items={topRow} direction="left" />

        {/* Bottom row — scrolls right */}
        {bottomRow.length > 0 && (
          <MarqueeRow items={bottomRow} direction="right" />
        )}

        {/* Gradient edges */}
        <div className="pointer-events-none absolute inset-y-0 left-0 w-24 bg-gradient-to-r from-peach to-transparent" />
        <div className="pointer-events-none absolute inset-y-0 right-0 w-24 bg-gradient-to-l from-peach to-transparent" />
      </div>
    </section>
  );
}

function MarqueeRow({
  items,
  direction,
}: {
  items: Testimonial[];
  direction: "left" | "right";
}) {
  // Duplicate items to create seamless loop
  const duplicated = [...items, ...items];

  return (
    <div className="flex overflow-hidden">
      <div
        className={cn(
          "flex gap-6 animate-marquee",
          direction === "right" && "[animation-direction:reverse]",
        )}
        style={{ "--duration": `${items.length * 8}s` } as React.CSSProperties}
      >
        {duplicated.map((t, i) => (
          <TestimonialCard
            key={`${t.author}-${i}`}
            author={t.author}
            content={t.content}
            rating={t.rating}
            avatarUrl={t.avatarUrl}
          />
        ))}
      </div>
    </div>
  );
}
