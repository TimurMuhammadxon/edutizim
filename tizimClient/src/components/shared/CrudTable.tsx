import type { ReactNode } from "react";
import { Card, CardContent } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";

export interface CrudColumn<T> {
  header: string;
  render: (item: T) => ReactNode;
  className?: string;
}

interface CrudTableProps<T> {
  columns: CrudColumn<T>[];
  items: T[];
  getKey: (item: T) => string;
  emptyMessage: string;
  onRowClick?: (item: T) => void;
}

export function CrudTable<T>({ columns, items, getKey, emptyMessage, onRowClick }: CrudTableProps<T>) {
  return (
    <Card>
      <CardContent className="p-0">
        <Table>
          <TableHeader>
            <TableRow>
              {columns.map((c, i) => (
                <TableHead key={i} className={c.className}>{c.header}</TableHead>
              ))}
            </TableRow>
          </TableHeader>
          <TableBody>
            {items.map((item) => (
              <TableRow
                key={getKey(item)}
                className={onRowClick ? "cursor-pointer" : undefined}
                onClick={onRowClick ? () => onRowClick(item) : undefined}
              >
                {columns.map((c, i) => (
                  <TableCell key={i} className={c.className}>{c.render(item)}</TableCell>
                ))}
              </TableRow>
            ))}
            {items.length === 0 && (
              <TableRow>
                <TableCell colSpan={columns.length} className="text-center text-muted-foreground py-10">
                  {emptyMessage}
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </CardContent>
    </Card>
  );
}
