import { useQuery } from "@tanstack/react-query";
import { useAuthStore } from "@/store/auth";
import { useBranchStore } from "@/store/branch";
import { Card, CardContent } from "@/components/ui/card";
import { PageLoader } from "@/components/shared/LoadingSpinner";
import { dashboardApi } from "@/api/dashboard";
import { Users, GraduationCap, Layers, AlertTriangle, Clock, Wallet } from "lucide-react";

const CRM_ROLES = ["Staff", "OrgAdmin", "SuperAdmin", "Owner"];

export function HomePage() {
  const user = useAuthStore((s) => s.user);

  if (user && CRM_ROLES.includes(user.role)) {
    return <CrmDashboard />;
  }

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Xush kelibsiz{user?.firstName ? `, ${user.firstName}` : ""}!</h1>
        <p className="text-muted-foreground mt-1">
          {user?.email ?? user?.phone}
        </p>
      </div>

      <Card>
        <CardContent className="p-6">
          <p className="text-muted-foreground">
            Platforma hozircha ishga tushirilmoqda. Tez orada bu yerda o'quv markazingiz uchun CRM funksiyalari paydo bo'ladi.
          </p>
        </CardContent>
      </Card>
    </div>
  );
}

function CrmDashboard() {
  const user = useAuthStore((s) => s.user);
  const branchId = useBranchStore((s) => s.branchId);

  const { data, isLoading } = useQuery({
    queryKey: ["dashboard-summary", branchId],
    queryFn: () => dashboardApi.getSummary(branchId ?? undefined),
  });

  if (isLoading || !data) return <PageLoader />;

  const cards = [
    { label: "Faol lidlar", value: data.activeLeads, icon: Users },
    { label: "Faol talabalar", value: data.activeStudents, icon: GraduationCap },
    { label: "Guruhlar", value: data.activeGroups, icon: Layers },
    { label: "Qarzdorlar", value: data.debtors, icon: AlertTriangle, danger: true },
    { label: "Sinov darsida", value: data.inTrial, icon: Clock },
    { label: "Shu oy to'laganlar", value: data.paidThisMonthCount, icon: Wallet },
  ];

  return (
    <div className="max-w-6xl mx-auto space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Xush kelibsiz{user?.firstName ? `, ${user.firstName}` : ""}!</h1>
        <p className="text-muted-foreground mt-1">O'quv markazingiz bo'yicha umumiy ko'rsatkichlar</p>
      </div>

      <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4">
        {cards.map((c) => (
          <Card key={c.label}>
            <CardContent className="p-4 space-y-2">
              <c.icon className={`h-5 w-5 ${c.danger && c.value > 0 ? "text-destructive" : "text-muted-foreground"}`} />
              <div className={`text-2xl font-bold ${c.danger && c.value > 0 ? "text-destructive" : ""}`}>{c.value}</div>
              <div className="text-xs text-muted-foreground">{c.label}</div>
            </CardContent>
          </Card>
        ))}
      </div>

      <Card>
        <CardContent className="p-6">
          <div className="text-sm text-muted-foreground">Shu oy tushum</div>
          <div className="text-3xl font-bold mt-1">{data.paidThisMonthAmount.toLocaleString()} so'm</div>
        </CardContent>
      </Card>
    </div>
  );
}
