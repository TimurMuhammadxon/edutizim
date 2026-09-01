import type { ReactNode } from "react";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { useTranslation } from "@/lib/i18n";

interface CrudFormDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  children: ReactNode;
  onSave: () => void;
  saveDisabled?: boolean;
  saving?: boolean;
}

export function CrudFormDialog({ open, onOpenChange, title, children, onSave, saveDisabled, saving }: CrudFormDialogProps) {
  const t = useTranslation();
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
        </DialogHeader>
        <div className="space-y-4 py-2">{children}</div>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>{t.cancel}</Button>
          <Button onClick={onSave} disabled={saveDisabled}>
            {saving ? t.loading : t.save}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
