import { create } from "zustand";
import { persist } from "zustand/middleware";

interface BranchState {
  branchId: string | null;
  setBranchId: (branchId: string | null) => void;
}

export const useBranchStore = create<BranchState>()(
  persist(
    (set) => ({
      branchId: null,
      setBranchId: (branchId) => set({ branchId }),
    }),
    { name: "app-branch-id" }
  )
);
