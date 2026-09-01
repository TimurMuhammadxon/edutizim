import { api } from "./axios";
import { type RoomDto } from "@/types";

export const roomsApi = {
  list: (params?: { branchId?: string; isActive?: boolean }) =>
    api.get<RoomDto[]>("/org/rooms", { params }).then((r) => r.data),

  create: (data: { branchId: string; name: string; capacity: number }) =>
    api.post<{ id: string }>("/org/rooms", data).then((r) => r.data),

  update: (id: string, data: { name: string; capacity: number }) =>
    api.put(`/org/rooms/${id}`, data),

  toggleActive: (id: string, isActive: boolean) =>
    api.patch(`/org/rooms/${id}/active`, { isActive }),
};
