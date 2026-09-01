import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";

const MONTH_OPTIONS = [
  { value: "01", label: "Yanvar" },
  { value: "02", label: "Fevral" },
  { value: "03", label: "Mart" },
  { value: "04", label: "Aprel" },
  { value: "05", label: "May" },
  { value: "06", label: "Iyun" },
  { value: "07", label: "Iyul" },
  { value: "08", label: "Avgust" },
  { value: "09", label: "Sentyabr" },
  { value: "10", label: "Oktyabr" },
  { value: "11", label: "Noyabr" },
  { value: "12", label: "Dekabr" },
];

interface Props {
  /** "YYYY-MM" */
  value: string;
  onChange: (value: string) => void;
}

export function MonthYearPicker({ value, onChange }: Props) {
  const [year, month] = value.split("-");
  const currentYear = new Date().getFullYear();
  const years = Array.from({ length: 4 }, (_, i) => String(currentYear - 1 + i));

  return (
    <div className="flex gap-2">
      <Select value={month} onValueChange={(m) => onChange(`${year}-${m}`)}>
        <SelectTrigger>
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          {MONTH_OPTIONS.map((m) => (
            <SelectItem key={m.value} value={m.value}>{m.label}</SelectItem>
          ))}
        </SelectContent>
      </Select>
      <Select value={year} onValueChange={(y) => onChange(`${y}-${month}`)}>
        <SelectTrigger className="w-24">
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          {years.map((y) => (
            <SelectItem key={y} value={y}>{y}</SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
}
