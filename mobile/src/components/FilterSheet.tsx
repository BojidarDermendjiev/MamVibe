import {
  Modal,
  View,
  Text,
  TouchableOpacity,
  ScrollView,
  StyleSheet,
  Pressable,
} from 'react-native';
import { useTheme } from '@/contexts/ThemeContext';
import { ROSE } from '@/constants/palette';
import { AgeGroup, ListingType } from '@mamvibe/shared';
import type { ItemFilter, Category } from '@mamvibe/shared';

interface Props {
  visible: boolean;
  filter: ItemFilter;
  categories: Category[];
  onClose: () => void;
  onChange: (partial: Partial<ItemFilter>) => void;
}

function CheckRow({
  label,
  selected,
  onPress,
  borderColor,
  textColor,
}: {
  label: string;
  selected: boolean;
  onPress: () => void;
  borderColor: string;
  textColor: string;
}) {
  return (
    <TouchableOpacity style={s.row} onPress={onPress} activeOpacity={0.65}>
      <View style={[s.checkbox, { borderColor: selected ? ROSE : borderColor }, selected && s.checkboxActive]}>
        {selected && <Text style={s.checkmark}>✓</Text>}
      </View>
      <Text style={[s.rowLabel, { color: textColor }]}>{label}</Text>
    </TouchableOpacity>
  );
}

const AGE_GROUPS: { label: string; value: AgeGroup }[] = [
  { label: 'Newborn',    value: AgeGroup.Newborn },
  { label: 'Infant',     value: AgeGroup.Infant },
  { label: 'Toddler',    value: AgeGroup.Toddler },
  { label: 'Preschool',  value: AgeGroup.Preschool },
  { label: 'School Age', value: AgeGroup.SchoolAge },
  { label: 'Teen',       value: AgeGroup.Teen },
];

const SORT_OPTIONS: { label: string; value: string }[] = [
  { label: 'Newest first',       value: 'newest' },
  { label: 'Oldest first',       value: 'oldest' },
  { label: 'Price: low to high', value: 'price_asc' },
  { label: 'Price: high to low', value: 'price_desc' },
  { label: 'Most popular',       value: 'most_liked' },
];

export default function FilterSheet({ visible, filter, categories, onClose, onChange }: Props) {
  const { colors } = useTheme();

  const clearAll = () =>
    onChange({ categoryId: undefined, listingType: undefined, ageGroup: undefined, sortBy: 'newest', page: 1 });

  const activeCount = [
    !!filter.categoryId,
    filter.listingType !== undefined,
    filter.ageGroup !== undefined,
    filter.sortBy !== 'newest',
  ].filter(Boolean).length;

  return (
    <Modal visible={visible} transparent animationType="slide" onRequestClose={onClose}>
      <View style={s.overlay}>
        <Pressable style={StyleSheet.absoluteFillObject} onPress={onClose} />

        <View style={[s.sheet, { backgroundColor: colors.card }]}>
          {/* Handle */}
          <View style={[s.handle, { backgroundColor: colors.border }]} />

          {/* Header */}
          <View style={s.header}>
            <Text style={[s.title, { color: colors.text }]}>Filters</Text>
            {activeCount > 0 && (
              <TouchableOpacity onPress={clearAll}>
                <Text style={[s.clearBtn, { color: ROSE }]}>Clear all</Text>
              </TouchableOpacity>
            )}
          </View>

          <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={s.scroll}>

            {/* ── Category ── */}
            <Text style={[s.sectionTitle, { color: colors.text }]}>Category</Text>
            <CheckRow
              label="All"
              selected={!filter.categoryId}
              onPress={() => onChange({ categoryId: undefined, page: 1 })}
              borderColor={colors.border}
              textColor={colors.text}
            />
            {categories.map((cat) => (
              <CheckRow
                key={cat.id}
                label={cat.name}
                selected={filter.categoryId === cat.id}
                onPress={() => onChange({ categoryId: filter.categoryId === cat.id ? undefined : cat.id, page: 1 })}
                borderColor={colors.border}
                textColor={colors.text}
              />
            ))}

            <View style={[s.divider, { backgroundColor: colors.border }]} />

            {/* ── Listing Type ── */}
            <Text style={[s.sectionTitle, { color: colors.text }]}>Listing Type</Text>
            <CheckRow
              label="All"
              selected={filter.listingType === undefined}
              onPress={() => onChange({ listingType: undefined, page: 1 })}
              borderColor={colors.border}
              textColor={colors.text}
            />
            <CheckRow
              label="For Sale"
              selected={filter.listingType === ListingType.Sell}
              onPress={() => onChange({ listingType: filter.listingType === ListingType.Sell ? undefined : ListingType.Sell, page: 1 })}
              borderColor={colors.border}
              textColor={colors.text}
            />
            <CheckRow
              label="Free / Donate"
              selected={filter.listingType === ListingType.Donate}
              onPress={() => onChange({ listingType: filter.listingType === ListingType.Donate ? undefined : ListingType.Donate, page: 1 })}
              borderColor={colors.border}
              textColor={colors.text}
            />

            <View style={[s.divider, { backgroundColor: colors.border }]} />

            {/* ── Age Group ── */}
            <Text style={[s.sectionTitle, { color: colors.text }]}>Age Group</Text>
            <CheckRow
              label="All ages"
              selected={filter.ageGroup === undefined}
              onPress={() => onChange({ ageGroup: undefined, page: 1 })}
              borderColor={colors.border}
              textColor={colors.text}
            />
            {AGE_GROUPS.map((ag) => (
              <CheckRow
                key={ag.value}
                label={ag.label}
                selected={filter.ageGroup === ag.value}
                onPress={() => onChange({ ageGroup: filter.ageGroup === ag.value ? undefined : ag.value, page: 1 })}
                borderColor={colors.border}
                textColor={colors.text}
              />
            ))}

            <View style={[s.divider, { backgroundColor: colors.border }]} />

            {/* ── Sort ── */}
            <Text style={[s.sectionTitle, { color: colors.text }]}>Sort By</Text>
            {SORT_OPTIONS.map((opt) => (
              <CheckRow
                key={opt.value}
                label={opt.label}
                selected={filter.sortBy === opt.value}
                onPress={() => onChange({ sortBy: opt.value, page: 1 })}
                borderColor={colors.border}
                textColor={colors.text}
              />
            ))}

          </ScrollView>

          <TouchableOpacity style={s.applyBtn} onPress={onClose} activeOpacity={0.85}>
            <Text style={s.applyText}>Show Results{activeCount > 0 ? ` (${activeCount} active)` : ''}</Text>
          </TouchableOpacity>
        </View>
      </View>
    </Modal>
  );
}

const s = StyleSheet.create({
  overlay: {
    flex: 1,
    justifyContent: 'flex-end',
    backgroundColor: 'rgba(0,0,0,0.45)',
  },
  sheet: {
    borderTopLeftRadius: 20,
    borderTopRightRadius: 20,
    maxHeight: '82%',
    paddingBottom: 34,
  },
  handle: {
    width: 36,
    height: 4,
    borderRadius: 2,
    alignSelf: 'center',
    marginTop: 10,
    marginBottom: 2,
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: 20,
    paddingVertical: 14,
  },
  title: {
    fontSize: 17,
    fontWeight: '700',
  },
  clearBtn: {
    fontSize: 14,
    fontWeight: '500',
  },
  scroll: {
    paddingHorizontal: 20,
    paddingBottom: 8,
  },
  sectionTitle: {
    fontSize: 15,
    fontWeight: '700',
    marginBottom: 10,
    marginTop: 4,
  },
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: 10,
  },
  checkbox: {
    width: 20,
    height: 20,
    borderRadius: 4,
    borderWidth: 1.5,
    alignItems: 'center',
    justifyContent: 'center',
    marginRight: 12,
  },
  checkboxActive: {
    backgroundColor: ROSE,
    borderColor: ROSE,
  },
  checkmark: {
    color: '#fff',
    fontSize: 12,
    fontWeight: '700',
    lineHeight: 14,
  },
  rowLabel: {
    fontSize: 14,
  },
  divider: {
    height: 1,
    marginVertical: 16,
  },
  applyBtn: {
    backgroundColor: ROSE,
    marginHorizontal: 20,
    marginTop: 10,
    paddingVertical: 14,
    borderRadius: 12,
    alignItems: 'center',
  },
  applyText: {
    color: '#fff',
    fontSize: 15,
    fontWeight: '700',
  },
});
