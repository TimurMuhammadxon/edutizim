import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { financeApi, type FinanceFilterParams } from "@/api/finance";
import { branchesApi } from "@/api/branches";
import { groupsApi } from "@/api/groups";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { PageLoader } from "@/components/shared/LoadingSpinner";
import { toast } from "@/components/ui/use-toast";
import { Trash2, Search, X, Wallet, AlertTriangle, Users } from "lucide-react";
import { PAYMENT_METHOD_LABELS, formatMonthLabel } from "@/lib/groupHelpers";

interface Filters {
  search: string;
  branchId: string; // "all" | branch id
  groupId: string;  // "all" | group id
  fromDate: string;
  toDate: string;
}

function defaultFilters(): Filters {
  const now = new Date();
  // last 3 calendar months including the current one (e.g. today in September → Jul 1 .. today)
  const rangeStart = new Date(now.getFullYear(), now.getMonth() - 2, 1).toISOString().slice(0, 10);
  const todayStr = now.toISOString().slice(0, 10);
  return { search: "", branchId: "all", groupId: "all", fromDate: rangeStart, toDate: todayStr };
}

function toApiParams(f: Filters): FinanceFilterParams {
  return {
    branchId: f.branchId === "all" ? undefined : f.branchId,
    groupId: f.groupId === "all" ? undefined : f.groupId,
    search: f.search.trim() || undefined,
    fromDate: f.fromDate || undefined,
    toDate: f.toDate || undefined,
  };
}

function StatTile({ icon: Icon, label, value, tone }: { icon: React.ElementType; label: string; value: string; tone?: "positive" | "negative" }) {
  return (
    <Card>
      <CardContent className="p-4 flex items-center gap-3">
        <div className="flex h-10 w-10 items-center justify-center rounded-md bg-muted border flex-shrink-0">
          <Icon className="h-4 w-4 text-muted-foreground" />
        </div>
        <div className="min-w-0">
          <div className={`text-lg font-bold truncate ${tone === "negative" ? "text-destructive" : tone === "positive" ? "text-emerald-500" : ""}`}>
            {value}
          </div>
          <div className="text-xs text-muted-foreground">{label}</div>
        </div>
      </CardContent>
    </Card>
  );
}

export function FinancePage() {
  const [tab, setTab] = useState<"current" | "period" | "payments">("current");
  const [draft, setDraft] = useState<Filters>(defaultFilters);
  const [filters, setFilters] = useState<Filters>(defaultFilters);

  const { data: branches } = useQuery({
    queryKey: ["org-branches", "all"],
    queryFn: () => branchesApi.list({ isActive: true }),
  });

  const { data: groupOptions } = useQuery({
    queryKey: ["crm-groups-filter", draft.branchId],
    queryFn: () => groupsApi.list({ branchId: draft.branchId === "all" ? undefined : draft.branchId, pageSize: 200 }),
  });

  const apiParams = toApiParams(filters);

  const { data: paymentsSummary } = useQuery({
    queryKey: ["finance-payments-summary", apiParams],
    queryFn: () => financeApi.getPaymentsSummary(apiParams),
  });

  const { data: periodDebts } = useQuery({
    queryKey: ["finance-period-debts", apiParams],
    queryFn: () => financeApi.getPeriodDebts({ ...apiParams, fromDate: filters.fromDate, toDate: filters.toDate }),
  });

  const { data: currentDebtors } = useQuery({
    queryKey: ["finance-debtors", filters.branchId, filters.groupId, filters.search],
    queryFn: () => financeApi.getDebtors({
      branchId: apiParams.branchId, groupId: apiParams.groupId, search: apiParams.search,
    }),
  });

  const periodDebtTotal = periodDebts?.reduce((sum, d) => sum + d.amountOwedInPeriod, 0) ?? 0;

  const apply = () => setFilters(draft);
  const reset = () => { const d = defaultFilters(); setDraft(d); setFilters(d); };
  const hasCustomFilters =
    filters.search !== "" || filters.branchId !== "all" || filters.groupId !== "all" ||
    filters.fromDate !== defaultFilters().fromDate || filters.toDate !== defaultFilters().toDate;

  return (
    <div className="max-w-6xl mx-auto space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Moliya</h1>
        <p className="text-muted-foreground mt-1">Qarzdorlar, to'lovlar va davr bo'yicha tahlil</p>
      </div>

      {/* Filter bar */}
      <Card>
        <CardContent className="p-4">
          <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-3 items-end">
            <div className="space-y-1.5 col-span-2 lg:col-span-2">
              <Label className="text-xs text-muted-foreground">Qidirish</Label>
              <div className="relative">
                <Search className="h-4 w-4 absolute left-2.5 top-1/2 -translate-y-1/2 text-muted-foreground" />
                <Input
                  className="pl-8"
                  placeholder="Ism yoki telefon..."
                  value={draft.search}
                  onChange={(e) => setDraft({ ...draft, search: e.target.value })}
                  onKeyDown={(e) => e.key === "Enter" && apply()}
                />
              </div>
            </div>

            <div className="space-y-1.5">
              <Label className="text-xs text-muted-foreground">Filial</Label>
              <Select
                value={draft.branchId}
                onValueChange={(v) => setDraft({ ...draft, branchId: v, groupId: "all" })}
              >
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Barcha filiallar</SelectItem>
                  {branches?.map((b) => (
                    <SelectItem key={b.id} value={b.id}>{b.name}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-1.5">
              <Label className="text-xs text-muted-foreground">Guruh</Label>
              <Select value={draft.groupId} onValueChange={(v) => setDraft({ ...draft, groupId: v })}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Barcha guruhlar</SelectItem>
                  {groupOptions?.items.map((g) => (
                    <SelectItem key={g.id} value={g.id}>{g.name}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-1.5">
              <Label className="text-xs text-muted-foreground">Boshlanish sanasi</Label>
              <Input type="date" value={draft.fromDate} onChange={(e) => setDraft({ ...draft, fromDate: e.target.value })} />
            </div>

            <div className="space-y-1.5">
              <Label className="text-xs text-muted-foreground">Tugash sanasi</Label>
              <Input type="date" value={draft.toDate} onChange={(e) => setDraft({ ...draft, toDate: e.target.value })} />
            </div>
          </div>

          <div className="flex items-center gap-2 mt-3">
            <Button size="sm" onClick={apply}>
              <Search className="h-4 w-4 mr-1.5" />
              Qo'llash
            </Button>
            {hasCustomFilters && (
              <Button size="sm" variant="ghost" onClick={reset}>
                <X className="h-4 w-4 mr-1.5" />
                Tozalash
              </Button>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Summary tiles for the selected period/filters */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
        <StatTile
          icon={Wallet}
          label={`To'lovlar (${filters.fromDate} — ${filters.toDate})`}
          value={`${(paymentsSummary?.totalAmount ?? 0).toLocaleString()} so'm`}
          tone="positive"
        />
        <StatTile
          icon={AlertTriangle}
          label={`Davr uchun qarz (${periodDebts?.length ?? 0} talaba)`}
          value={`${periodDebtTotal.toLocaleString()} so'm`}
          tone={periodDebtTotal > 0 ? "negative" : undefined}
        />
        <StatTile
          icon={Users}
          label="Joriy qarzdorlar soni"
          value={`${currentDebtors?.length ?? 0} ta`}
          tone={(currentDebtors?.length ?? 0) > 0 ? "negative" : undefined}
        />
      </div>

      <div className="flex gap-2 border-b">
        <button
          onClick={() => setTab("current")}
          className={`px-3 py-2 text-sm font-medium border-b-2 -mb-px ${
            tab === "current" ? "border-primary text-foreground" : "border-transparent text-muted-foreground"
          }`}
        >
          Joriy qarzdorlar
        </button>
        <button
          onClick={() => setTab("period")}
          className={`px-3 py-2 text-sm font-medium border-b-2 -mb-px ${
            tab === "period" ? "border-primary text-foreground" : "border-transparent text-muted-foreground"
          }`}
        >
          Davr bo'yicha qarzlar
        </button>
        <button
          onClick={() => setTab("payments")}
          className={`px-3 py-2 text-sm font-medium border-b-2 -mb-px ${
            tab === "payments" ? "border-primary text-foreground" : "border-transparent text-muted-foreground"
          }`}
        >
          To'lovlar
        </button>
      </div>

      {tab === "current" && <CurrentDebtorsTab debtors={currentDebtors} />}
      {tab === "period" && <PeriodDebtsTab debtors={periodDebts} />}
      {tab === "payments" && <PaymentsTab apiParams={apiParams} />}
    </div>
  );
}

function CurrentDebtorsTab({ debtors }: { debtors?: import("@/types").DebtorDto[] }) {
  if (!debtors) return <PageLoader />;

  return (
    <Card>
      <CardContent className="p-0">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Talaba</TableHead>
              <TableHead>Guruh</TableHead>
              <TableHead>Balans</TableHead>
              <TableHead className="text-right">Muddati o'tgan</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {debtors.map((d) => (
              <TableRow key={`${d.studentId}-${d.groupId}`}>
                <TableCell>
                  <div className="font-medium">{d.studentFullName}</div>
                  <div className="text-xs text-muted-foreground">{d.studentPhone}</div>
                </TableCell>
                <TableCell className="text-sm">{d.groupName}</TableCell>
                <TableCell>
                  <span className="text-destructive font-semibold">{d.balance.toLocaleString()} so'm</span>
                </TableCell>
                <TableCell className="text-right">
                  <Badge variant="destructive">{d.daysOverdue} kun</Badge>
                  <div className="text-xs text-muted-foreground mt-0.5">{d.nextPaymentDueDate} dan</div>
                </TableCell>
              </TableRow>
            ))}
            {debtors.length === 0 && (
              <TableRow>
                <TableCell colSpan={4} className="text-center text-muted-foreground py-10">
                  Qarzdorlar yo'q
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </CardContent>
    </Card>
  );
}

function PeriodDebtsTab({ debtors }: { debtors?: import("@/types").PeriodDebtDto[] }) {
  if (!debtors) return <PageLoader />;

  return (
    <Card>
      <CardContent className="p-0">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Talaba</TableHead>
              <TableHead>Guruh</TableHead>
              <TableHead>Qarzdor oylar</TableHead>
              <TableHead className="text-right">Davr uchun qarz</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {debtors.map((d) => (
              <TableRow key={`${d.studentId}-${d.groupId}`}>
                <TableCell>
                  <div className="font-medium">{d.studentFullName}</div>
                  <div className="text-xs text-muted-foreground">{d.studentPhone}</div>
                </TableCell>
                <TableCell className="text-sm">{d.groupName}</TableCell>
                <TableCell>
                  <div className="flex flex-wrap gap-1">
                    {d.months.map((m) => (
                      <Badge key={m.month} variant="outline" className="text-xs" title={`Kutilgan: ${m.expected.toLocaleString()} · To'langan: ${m.paid.toLocaleString()}`}>
                        {formatMonthLabel(m.month)}
                      </Badge>
                    ))}
                  </div>
                </TableCell>
                <TableCell className="text-right">
                  <span className="text-destructive font-semibold">{d.amountOwedInPeriod.toLocaleString()} so'm</span>
                </TableCell>
              </TableRow>
            ))}
            {debtors.length === 0 && (
              <TableRow>
                <TableCell colSpan={4} className="text-center text-muted-foreground py-10">
                  Tanlangan davrda qarzlar yo'q
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </CardContent>
    </Card>
  );
}

function PaymentsTab({ apiParams }: { apiParams: FinanceFilterParams }) {
  const qc = useQueryClient();
  const { data, isLoading } = useQuery({
    queryKey: ["finance-payments", apiParams],
    queryFn: () => financeApi.getPayments({ ...apiParams, pageSize: 100 }),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => financeApi.deletePayment(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["finance-payments"] });
      qc.invalidateQueries({ queryKey: ["finance-payments-summary"] });
      toast({ title: "To'lov o'chirildi" });
    },
  });

  if (isLoading) return <PageLoader />;

  return (
    <Card>
      <CardContent className="p-0">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Talaba</TableHead>
              <TableHead>Guruh</TableHead>
              <TableHead>Summa</TableHead>
              <TableHead>Qaysi oy uchun</TableHead>
              <TableHead>Usul</TableHead>
              <TableHead>Sana</TableHead>
              <TableHead className="text-right w-16">Amallar</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {data?.items.map((p) => (
              <TableRow key={p.id}>
                <TableCell className="font-medium">{p.studentFullName}</TableCell>
                <TableCell className="text-sm">{p.groupName}</TableCell>
                <TableCell className="text-sm">{p.amount.toLocaleString()} so'm</TableCell>
                <TableCell className="text-sm">{formatMonthLabel(p.forMonth)}</TableCell>
                <TableCell className="text-sm">
                  <Badge variant="outline" className="text-xs">{PAYMENT_METHOD_LABELS[p.method]}</Badge>
                </TableCell>
                <TableCell className="text-sm">{p.paidAt}</TableCell>
                <TableCell className="text-right">
                  <button
                    onClick={() => {
                      if (confirm("To'lovni o'chirishni tasdiqlaysizmi?")) deleteMutation.mutate(p.id);
                    }}
                    className="text-muted-foreground hover:text-destructive"
                  >
                    <Trash2 className="h-4 w-4" />
                  </button>
                </TableCell>
              </TableRow>
            ))}
            {data?.items.length === 0 && (
              <TableRow>
                <TableCell colSpan={7} className="text-center text-muted-foreground py-10">
                  To'lovlar yo'q
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </CardContent>
    </Card>
  );
}
