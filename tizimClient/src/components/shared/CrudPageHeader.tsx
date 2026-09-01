import { Plus, Search } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

interface CrudPageHeaderProps {
  title: string;
  count: number;
  countLabel: string;
  addLabel?: string;
  onAdd?: () => void;
  addDisabled?: boolean;
}

export function CrudPageHeader({ title, count, countLabel, addLabel, onAdd, addDisabled }: CrudPageHeaderProps) {
  return (
    <div className="flex items-center justify-between">
      <div>
        <h1 className="text-2xl font-bold">{title}</h1>
        <p className="text-muted-foreground mt-1">{countLabel}: {count}</p>
      </div>
      {onAdd && (
        <Button onClick={onAdd} disabled={addDisabled}>
          <Plus className="h-4 w-4 mr-2" />
          {addLabel}
        </Button>
      )}
    </div>
  );
}

interface CrudSearchBarProps {
  value: string;
  onChange: (value: string) => void;
  onSearch: () => void;
  placeholder: string;
}

export function CrudSearchBar({ value, onChange, onSearch, placeholder }: CrudSearchBarProps) {
  return (
    <div className="flex gap-2">
      <Input
        placeholder={placeholder}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        onKeyDown={(e) => { if (e.key === "Enter") onSearch(); }}
        className="max-w-sm"
      />
      <Button variant="outline" onClick={onSearch}>
        <Search className="h-4 w-4" />
      </Button>
    </div>
  );
}
