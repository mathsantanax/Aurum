import { useTheme } from "../app/providers/useTheme";

export function ThemeSelect() {
  const { preference, setPreference } = useTheme();

  return (
    <label className="inline-flex min-h-10 items-center gap-2 rounded-xl border border-slate-300 bg-white px-3 text-sm font-medium text-slate-700 shadow-sm">
      <span aria-hidden="true" className="text-base">◐</span>
      <span className="sr-only">Aparência</span>
      <select
        aria-label="Aparência"
        value={preference}
        onChange={(event) => {
          const nextPreference = event.target.value;
          if (
            nextPreference === "light" ||
            nextPreference === "dark" ||
            nextPreference === "system"
          ) {
            setPreference(nextPreference);
          }
        }}
        className="min-w-24 cursor-pointer bg-transparent text-sm font-semibold text-slate-700 outline-none"
      >
        <option value="light">Claro</option>
        <option value="dark">Escuro</option>
        <option value="system">Automático</option>
      </select>
    </label>
  );
}
