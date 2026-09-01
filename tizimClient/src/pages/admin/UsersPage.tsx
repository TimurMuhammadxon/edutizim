import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { adminUsersApi } from "@/api/admin";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { PageLoader } from "@/components/shared/LoadingSpinner";
import { useTranslation } from "@/lib/i18n";
import { ChevronLeft, ChevronRight, Search } from "lucide-react";

const ROLE_COLORS: Record<string, string> = {
  Owner:      "bg-purple-950/50 text-purple-400 border border-purple-800/30",
  SuperAdmin: "bg-red-950/50 text-red-400 border border-red-800/30",
  OrgAdmin:   "bg-orange-950/50 text-amber-400 border border-amber-800/30",
  Teacher:    "bg-blue-950/50 text-cyan-400 border border-cyan-800/30",
  Student:    "bg-slate-900/50 text-slate-400 border border-slate-700/30",
};

export function UsersPage() {
  const t = useTranslation();
  const [page, setPage] = useState(1);
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");

  const { data, isLoading } = useQuery({
    queryKey: ["admin-users", page, search],
    queryFn: () => adminUsersApi.list({ search: search || undefined, page, pageSize: 20 }),
  });

  const totalPages = data ? Math.ceil(data.totalCount / 20) : 1;

  if (isLoading) return <PageLoader />;

  return (
    <div className="max-w-5xl mx-auto space-y-6">
      <div>
        <h1 className="text-2xl font-bold">{t.adminUsersTitle}</h1>
        <p className="text-muted-foreground mt-1">{t.manageUsersSubtitle}</p>
      </div>

      <div>
        <h2 className="text-lg font-semibold">{t.users}</h2>
        <p className="text-muted-foreground mt-1 text-sm">{t.total}: {data?.totalCount ?? 0}</p>
      </div>

      {/* Search */}
      <div className="flex gap-2">
        <Input
          placeholder={t.searchByEmail}
          value={searchInput}
          onChange={(e) => setSearchInput(e.target.value)}
          onKeyDown={(e) => { if (e.key === "Enter") { setSearch(searchInput); setPage(1); } }}
          className="max-w-sm"
        />
        <Button variant="outline" onClick={() => { setSearch(searchInput); setPage(1); }}>
          <Search className="h-4 w-4" />
        </Button>
      </div>

      <Card>
        <CardContent className="p-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>{t.fullName}</TableHead>
                <TableHead>Email</TableHead>
                <TableHead className="w-28">{t.roleColumn}</TableHead>
                <TableHead className="w-40">{t.registeredColumn}</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {data?.items.map((u) => (
                <TableRow key={u.id}>
                  <TableCell className="text-sm">{[u.firstName, u.lastName].filter(Boolean).join(" ") || "—"}</TableCell>
                  <TableCell className="font-mono text-sm">{u.email ?? u.phone ?? "—"}</TableCell>
                  <TableCell>
                    <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${ROLE_COLORS[u.role] ?? "bg-slate-900/50 text-slate-400"}`}>
                      {u.role}
                    </span>
                  </TableCell>
                  <TableCell className="text-sm text-muted-foreground">
                    {new Date(u.createdAt).toLocaleDateString("ru-RU")}
                  </TableCell>
                </TableRow>
              ))}
              {data?.items.length === 0 && (
                <TableRow>
                  <TableCell colSpan={4} className="text-center text-muted-foreground py-10">
                    {t.noUsersFound}
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {totalPages > 1 && (
        <div className="flex items-center justify-center gap-3">
          <Button variant="outline" size="sm" onClick={() => setPage((p) => p - 1)} disabled={page === 1}>
            <ChevronLeft className="h-4 w-4" />
          </Button>
          <span className="text-sm text-muted-foreground">{page} / {totalPages}</span>
          <Button variant="outline" size="sm" onClick={() => setPage((p) => p + 1)} disabled={page >= totalPages}>
            <ChevronRight className="h-4 w-4" />
          </Button>
        </div>
      )}
    </div>
  );
}
