import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { MonthYearPicker } from "@/components/shared/MonthYearPicker";
import { PAYMENT_METHOD_LABELS, PAYMENT_METHODS } from "@/lib/groupHelpers";
import { useTranslation } from "@/lib/i18n";
import type { GroupStudentDto, PaymentMethod } from "@/types";

function defaultForMonth(nextPaymentDueDate?: string): string {
  return (nextPaymentDueDate ?? new Date().toISOString().slice(0, 10)).slice(0, 7);
}

export function RecordPaymentDialog({
  student,
  isPending,
  onSubmit,
  onClose,
}: {
  student: GroupStudentDto;
  isPending: boolean;
  onSubmit: (data: { amount: number; paidAt: string; forMonth: string; method: PaymentMethod }) => void;
  onClose: () => void;
}) {
  const t = useTranslation();
  const [amount, setAmount] = useState(String(student.effectivePrice));
  const [paidAt, setPaidAt] = useState(new Date().toISOString().slice(0, 10));
  const [forMonth, setForMonth] = useState(defaultForMonth(student.nextPaymentDueDate));
  const [method, setMethod] = useState<PaymentMethod>("Cash");

  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{student.fullName} — {t.paymentDialogSuffix}</DialogTitle>
        </DialogHeader>
        <div className="space-y-4 py-2">
          <div className="space-y-1.5">
            <Label>{t.amountColumn} (so'm)</Label>
            <Input type="number" min="0" value={amount} onChange={(e) => setAmount(e.target.value)} />
          </div>
          <div className="space-y-1.5">
            <Label>{t.forMonthColumn}</Label>
            <MonthYearPicker value={forMonth} onChange={setForMonth} />
          </div>
          <div className="space-y-1.5">
            <Label>{t.dateColumn}</Label>
            <Input type="date" value={paidAt} onChange={(e) => setPaidAt(e.target.value)} />
          </div>
          <div className="space-y-1.5">
            <Label>{t.paymentMethodLabel}</Label>
            <Select value={method} onValueChange={(v) => setMethod(v as PaymentMethod)}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {PAYMENT_METHODS.map((m) => (
                  <SelectItem key={m} value={m}>{PAYMENT_METHOD_LABELS[m]}</SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={onClose}>{t.cancel}</Button>
          <Button
            disabled={!amount || Number(amount) <= 0 || !forMonth || isPending}
            onClick={() => onSubmit({ amount: Number(amount), paidAt, forMonth: `${forMonth}-01`, method })}
          >
            {t.save}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

export function DiscountDialog({
  student,
  isPending,
  onSave,
  onRemove,
  onClose,
}: {
  student: GroupStudentDto;
  isPending: boolean;
  onSave: (data: { price: number; startDate: string; endDate: string }) => void;
  onRemove: () => void;
  onClose: () => void;
}) {
  const t = useTranslation();
  const today = new Date().toISOString().slice(0, 10);
  const yearEnd = `${new Date().getFullYear()}-12-31`;
  const [price, setPrice] = useState(String(student.discountedPrice ?? student.effectivePrice));
  const [startDate, setStartDate] = useState(student.discountStartDate ?? today);
  const [endDate, setEndDate] = useState(student.discountEndDate ?? yearEnd);

  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{student.fullName} — {t.discountDialogSuffix}</DialogTitle>
        </DialogHeader>
        <div className="space-y-4 py-2">
          <div className="space-y-1.5">
            <Label>{t.discountedPriceLabel}</Label>
            <Input type="number" min="0" value={price} onChange={(e) => setPrice(e.target.value)} />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="space-y-1.5">
              <Label>{t.fromDateLabel}</Label>
              <Input type="date" value={startDate} onChange={(e) => setStartDate(e.target.value)} />
            </div>
            <div className="space-y-1.5">
              <Label>{t.toDateLabel}</Label>
              <Input type="date" value={endDate} onChange={(e) => setEndDate(e.target.value)} />
            </div>
          </div>
        </div>
        <DialogFooter>
          {student.discountedPrice != null && (
            <Button variant="outline" className="mr-auto" disabled={isPending} onClick={onRemove}>
              {t.removeDiscountAction}
            </Button>
          )}
          <Button variant="outline" onClick={onClose}>{t.cancel}</Button>
          <Button
            disabled={!price || Number(price) < 0 || isPending}
            onClick={() => onSave({ price: Number(price), startDate, endDate })}
          >
            {t.save}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
