import { create } from "zustand";

interface BranchState {
  branchId: string | null;
  setBranchId: (branchId: string | null) => void;
}

const STORAGE_KEY = "app-branch-id";

function getSavedBranchId(): string | null {
  return localStorage.getItem(STORAGE_KEY);
}

export const useBranchStore = create<BranchState>((set) => ({
  branchId: getSavedBranchId(),
  setBranchId: (branchId) => {
    if (branchId) localStorage.setItem(STORAGE_KEY, branchId);
    else localStorage.removeItem(STORAGE_KEY);
    set({ branchId });
  },
}));
