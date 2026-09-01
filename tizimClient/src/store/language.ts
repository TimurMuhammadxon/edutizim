import { create } from "zustand";
import { persist } from "zustand/middleware";

export type LangCode = "uz-latn" | "ru" | "uz-cyrl";

interface LanguageState {
  lang: LangCode;
  setLang: (lang: LangCode) => void;
}

export const useLanguageStore = create<LanguageState>()(
  persist(
    (set) => ({
      lang: "uz-latn",
      setLang: (lang) => set({ lang }),
    }),
    { name: "app-lang" }
  )
);
