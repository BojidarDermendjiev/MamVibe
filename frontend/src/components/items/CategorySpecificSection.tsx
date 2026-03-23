import type { ReactNode } from 'react';
import { Ruler, Footprints, Puzzle, ShieldCheck, Baby, Milk, Armchair } from 'lucide-react';
import { AgeGroup } from '../../types/item';

// ── Clothing ──────────────────────────────────────────────────────────────────
const CLOTHING_SIZES = [
  { group: 'Baby',        range: '0 – 12 months', sizes: [50, 56, 62, 68, 74, 80] },
  { group: 'Toddler',     range: '1 – 3 years',   sizes: [86, 92, 98] },
  { group: 'Kids',        range: '3 – 8 years',   sizes: [104, 110, 116, 122, 128] },
  { group: 'Older Kids',  range: '8 – 12 years',  sizes: [134, 140, 146, 152] },
  { group: 'Teen',        range: '12+ years',      sizes: [158, 164, 170] },
];

// ── Toys ──────────────────────────────────────────────────────────────────────
const TOY_AGES = [
  { label: '0 – 6 months',  emoji: '🎵', sub1: 'Sensory toys',    sub2: 'Newborn',    value: AgeGroup.Newborn },
  { label: '6 – 12 months', emoji: '🎈', sub1: 'Exploration',     sub2: 'Infant',     value: AgeGroup.Infant },
  { label: '1 – 3 years',   emoji: '🧩', sub1: 'Building blocks', sub2: 'Toddler',    value: AgeGroup.Toddler },
  { label: '3 – 5 years',   emoji: '🎪', sub1: 'Role play',       sub2: 'Preschool',  value: AgeGroup.Preschool },
  { label: '5 – 12 years',  emoji: '🎮', sub1: 'Games & STEM',    sub2: 'School Age', value: AgeGroup.SchoolAge },
  { label: '12+ years',     emoji: '🎯', sub1: 'Teen & adult',    sub2: 'Teen',       value: AgeGroup.Teen },
] as const;

// ── Car Seats ─────────────────────────────────────────────────────────────────
const CAR_SEAT_GROUPS = [
  { label: 'Group 0',   emoji: '👶', sub1: '0 – 10 kg',  sub2: 'Birth – 6 months',  value: AgeGroup.Newborn },
  { label: 'Group 0+',  emoji: '🍼', sub1: '0 – 13 kg',  sub2: 'Birth – 15 months', value: AgeGroup.Infant },
  { label: 'Group 1',   emoji: '🧸', sub1: '9 – 18 kg',  sub2: '9 months – 4 yrs',  value: AgeGroup.Toddler },
  { label: 'Group 2',   emoji: '📚', sub1: '15 – 25 kg', sub2: '3 – 7 years',       value: AgeGroup.Preschool },
  { label: 'Group 2/3', emoji: '🚗', sub1: '15 – 36 kg', sub2: '3 – 12 years',      value: AgeGroup.SchoolAge },
] as const;

// ── Strollers ─────────────────────────────────────────────────────────────────
const STROLLER_AGES = [
  { label: 'From birth',  emoji: '👶', sub1: '0 – 6 months',  sub2: 'Lie-flat',   value: AgeGroup.Newborn },
  { label: '6 months+',   emoji: '🍼', sub1: '6 – 12 months', sub2: 'Infant',     value: AgeGroup.Infant },
  { label: '1 – 3 years', emoji: '🧸', sub1: 'Up to ~15 kg',  sub2: 'Toddler',    value: AgeGroup.Toddler },
  { label: '3+ years',    emoji: '🎨', sub1: 'Lightweight',   sub2: 'Preschool+', value: AgeGroup.Preschool },
] as const;

// ── Feeding ───────────────────────────────────────────────────────────────────
const FEEDING_AGES = [
  { label: 'Newborn',   emoji: '👶', sub1: '0 – 3 months',  sub2: 'Stage 1', value: AgeGroup.Newborn },
  { label: 'Infant',    emoji: '🍼', sub1: '3 – 12 months', sub2: 'Stage 2', value: AgeGroup.Infant },
  { label: 'Toddler',   emoji: '🧸', sub1: '1 – 3 years',   sub2: 'Stage 3', value: AgeGroup.Toddler },
  { label: 'Preschool', emoji: '🎨', sub1: '3 – 5 years',   sub2: 'Stage 4', value: AgeGroup.Preschool },
] as const;

// ── Furniture ─────────────────────────────────────────────────────────────────
const FURNITURE_AGES = [
  { label: 'Newborn',   emoji: '👶', sub1: '0 – 6 months',  sub2: 'Crib / Moses basket', value: AgeGroup.Newborn },
  { label: 'Infant',    emoji: '🍼', sub1: '6 – 12 months', sub2: 'Standard crib',        value: AgeGroup.Infant },
  { label: 'Toddler',   emoji: '🧸', sub1: '1 – 3 years',   sub2: 'Toddler bed',          value: AgeGroup.Toddler },
  { label: 'Preschool', emoji: '🎨', sub1: '3 – 5 years',   sub2: 'Junior bed',           value: AgeGroup.Preschool },
  { label: 'School Age',emoji: '📚', sub1: '5 – 12 years',  sub2: 'Kids bed / desk',      value: AgeGroup.SchoolAge },
] as const;

// ── Shoe Sizes ────────────────────────────────────────────────────────────────
const SHOE_SIZES = [
  { group: 'Baby',    range: '0 – 2 yrs',  sizes: [16, 17, 18, 19, 20, 21, 22] },
  { group: 'Toddler', range: '2 – 4 yrs',  sizes: [23, 24, 25, 26, 27] },
  { group: 'Kids',    range: '4 – 12 yrs', sizes: [28, 29, 30, 31, 32, 33, 34, 35] },
  { group: 'Teen',    range: '12+ yrs',    sizes: [36, 37, 38, 39, 40, 41] },
];

// ── Helper: generic age group card grid ──────────────────────────────────────
type AgeOption = { label: string; emoji: string; sub1: string; sub2: string; value: AgeGroup };

function AgeCardGrid({
  options,
  selected,
  onChange,
}: {
  options: readonly AgeOption[];
  selected: AgeGroup | null;
  onChange: (v: AgeGroup | null) => void;
}) {
  return (
    <>
      <div className="p-3 grid grid-cols-2 sm:grid-cols-3 gap-2">
        {options.map((ag) => {
          const isSelected = selected === ag.value;
          return (
            <button
              type="button"
              key={ag.value}
              onClick={() => onChange(isSelected ? null : ag.value)}
              className={`flex flex-col items-center gap-0.5 py-3 px-2 rounded-xl border-2 transition-all text-center ${
                isSelected
                  ? 'border-primary bg-primary/8 shadow-sm'
                  : 'border-gray-100 hover:border-lavender/60'
              }`}
            >
              <span className="text-2xl leading-none">{ag.emoji}</span>
              <span className={`text-xs font-semibold mt-1 ${isSelected ? 'text-primary' : 'text-gray-700'}`}>
                {ag.label}
              </span>
              <span className="text-[10px] text-[#364153] dark:text-white">{ag.sub1}</span>
              <span className={`text-[10px] font-medium mt-0.5 ${isSelected ? 'text-primary/70' : 'text-[#364153] dark:text-white'}`}>
                {ag.sub2}
              </span>
            </button>
          );
        })}
      </div>
      {selected !== null && (
        <div className="px-4 pb-3">
          <button
            type="button"
            onClick={() => onChange(null)}
            className="text-xs text-gray-400 hover:text-primary transition-colors"
          >
            ✕ Clear selection
          </button>
        </div>
      )}
    </>
  );
}

// ── Section wrapper ───────────────────────────────────────────────────────────
function SectionCard({
  icon,
  title,
  subtitle,
  children,
}: {
  icon: ReactNode;
  title: string;
  subtitle: string;
  children: ReactNode;
}) {
  return (
    <div className="rounded-xl border border-lavender/40 overflow-hidden">
      <div className="px-5 py-4 flex items-start gap-3 border-b border-lavender/20">
        <span className="text-primary flex-shrink-0 mt-0.5">{icon}</span>
        <div>
          <p className="text-base font-semibold text-primary">{title}</p>
          <p className="text-sm text-gray-500 mt-1">{subtitle}</p>
        </div>
      </div>
      {children}
    </div>
  );
}

// ── Public component ──────────────────────────────────────────────────────────
interface Props {
  categorySlug: string | undefined;
  ageGroup: AgeGroup | null;
  shoeSize: number | null;
  clothingSize: number | null;
  onAgeGroupChange: (v: AgeGroup | null) => void;
  onShoeSizeChange: (v: number | null) => void;
  onClothingSizeChange: (v: number | null) => void;
}

export default function CategorySpecificSection({
  categorySlug,
  ageGroup,
  shoeSize,
  clothingSize,
  onAgeGroupChange,
  onShoeSizeChange,
  onClothingSizeChange,
}: Props) {
  if (categorySlug === 'clothing') {
    return (
      <SectionCard
        icon={<Ruler className="w-5 h-5" />}
        title="Find the perfect size for your little one"
        subtitle="Pick the EU clothing size — helps buyers find the right fit instantly"
      >
        <div className="p-4 space-y-3">
          {CLOTHING_SIZES.map((group) => (
            <div key={group.group}>
              <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-1.5">
                {group.group} <span className="font-normal normal-case">· {group.range}</span>
              </p>
              <div className="flex flex-wrap gap-1.5">
                {group.sizes.map((sz) => (
                  <button
                    type="button"
                    key={sz}
                    onClick={() => onClothingSizeChange(clothingSize === sz ? null : sz)}
                    className={`w-12 h-10 rounded-lg border-2 text-sm font-semibold transition-all ${
                      clothingSize === sz
                        ? 'border-primary bg-primary/10 text-primary shadow-sm'
                        : 'border-gray-200 text-gray-600 hover:border-lavender'
                    }`}
                  >
                    {sz}
                  </button>
                ))}
              </div>
            </div>
          ))}
          {clothingSize !== null && (
            <button
              type="button"
              onClick={() => onClothingSizeChange(null)}
              className="text-xs text-gray-400 hover:text-primary transition-colors"
            >
              ✕ Clear size
            </button>
          )}
        </div>
      </SectionCard>
    );
  }

  if (categorySlug === 'shoes') {
    return (
      <SectionCard
        icon={<Footprints className="w-5 h-5" />}
        title="Shoe Size (EU)"
        subtitle="Pick the EU size so buyers find the right fit instantly"
      >
        <div className="p-4 space-y-3">
          {SHOE_SIZES.map((group) => (
            <div key={group.group}>
              <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-1.5">
                {group.group} <span className="font-normal normal-case">· {group.range}</span>
              </p>
              <div className="flex flex-wrap gap-1.5">
                {group.sizes.map((sz) => (
                  <button
                    type="button"
                    key={sz}
                    onClick={() => onShoeSizeChange(shoeSize === sz ? null : sz)}
                    className={`w-10 h-10 rounded-lg border-2 text-sm font-semibold transition-all ${
                      shoeSize === sz
                        ? 'border-primary bg-primary/10 text-primary shadow-sm'
                        : 'border-gray-200 text-gray-600 hover:border-lavender'
                    }`}
                  >
                    {sz}
                  </button>
                ))}
              </div>
            </div>
          ))}
          {shoeSize !== null && (
            <button
              type="button"
              onClick={() => onShoeSizeChange(null)}
              className="text-xs text-gray-400 hover:text-primary transition-colors"
            >
              ✕ Clear size
            </button>
          )}
        </div>
      </SectionCard>
    );
  }

  if (categorySlug === 'toys') {
    return (
      <SectionCard
        icon={<Puzzle className="w-5 h-5" />}
        title="Recommended Age"
        subtitle="Select the age range this toy is designed for"
      >
        <AgeCardGrid options={TOY_AGES} selected={ageGroup} onChange={onAgeGroupChange} />
      </SectionCard>
    );
  }

  if (categorySlug === 'car-seats') {
    return (
      <SectionCard
        icon={<ShieldCheck className="w-5 h-5" />}
        title="Car Seat Group"
        subtitle="Select the weight group — helps parents find the right seat"
      >
        <AgeCardGrid options={CAR_SEAT_GROUPS} selected={ageGroup} onChange={onAgeGroupChange} />
      </SectionCard>
    );
  }

  if (categorySlug === 'strollers') {
    return (
      <SectionCard
        icon={<Baby className="w-5 h-5" />}
        title="Suitable From"
        subtitle="Select the age range this stroller supports"
      >
        <AgeCardGrid options={STROLLER_AGES} selected={ageGroup} onChange={onAgeGroupChange} />
      </SectionCard>
    );
  }

  if (categorySlug === 'furniture') {
    return (
      <SectionCard
        icon={<Armchair className="w-5 h-5" />}
        title="Suitable Age"
        subtitle="Select the age range this furniture item is designed for"
      >
        <AgeCardGrid options={FURNITURE_AGES} selected={ageGroup} onChange={onAgeGroupChange} />
      </SectionCard>
    );
  }

  if (categorySlug === 'feeding') {
    return (
      <SectionCard
        icon={<Milk className="w-5 h-5" />}
        title="Suitable Age"
        subtitle="Select the age this feeding item is designed for"
      >
        <AgeCardGrid options={FEEDING_AGES} selected={ageGroup} onChange={onAgeGroupChange} />
      </SectionCard>
    );
  }

  return null;
}
