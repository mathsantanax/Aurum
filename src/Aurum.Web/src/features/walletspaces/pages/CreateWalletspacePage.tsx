import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import type { FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { getAuthErrorMessage } from "../../auth/utils/getAuthErrorMessage";
import { createWalletspace } from "../api/walletspacesApi";

export function CreateWalletspacePage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [name, setName] = useState("");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const createMutation = useMutation({
    mutationFn: createWalletspace,
    onSuccess: async (space) => {
      await queryClient.invalidateQueries({ queryKey: ["walletspaces"] });
      await queryClient.invalidateQueries({ queryKey: ["dashboard-summary"] });
      navigate(`/walletspaces/${space.id}`, { replace: true });
    },
  });

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setErrorMessage(null);
    try {
      await createMutation.mutateAsync(name.trim());
    } catch (error) {
      setErrorMessage(getAuthErrorMessage(error, "Não foi possível criar o Walletspace."));
    }
  }

  return (
    <div className="mx-auto max-w-2xl">
      <button
        type="button"
        onClick={() => navigate("/walletspaces")}
        className="mb-6 inline-flex items-center gap-2 text-sm font-semibold text-slate-600 transition hover:text-violet-900 focus-visible:rounded focus-visible:outline-2 focus-visible:outline-violet-700"
      >
        <span aria-hidden="true">←</span>
        Voltar aos espaços
      </button>
      <div>
        <p className="text-xs font-bold uppercase tracking-[0.18em] text-violet-800">Novo espaço</p>
        <h1 className="mt-2 text-3xl font-semibold tracking-tight text-slate-950">Dê um nome ao seu espaço</h1>
        <p className="mt-2 text-sm leading-6 text-slate-600">Use um nome fácil de reconhecer, como uma meta, projeto ou conta compartilhada.</p>
      </div>

      <form
        onSubmit={handleSubmit}
        className="mt-7 rounded-3xl border border-slate-200 bg-white p-6 shadow-sm shadow-slate-200/40 sm:p-8"
        aria-busy={createMutation.isPending}
      >
        <label htmlFor="walletspace-name" className="mb-2 block text-sm font-semibold text-slate-800">Nome do espaço</label>
        <input
          id="walletspace-name"
          type="text"
          name="name"
          value={name}
          onChange={(event) => setName(event.target.value)}
          minLength={3}
          maxLength={100}
          required
          placeholder="Ex.: Viagem, Casa, Reserva"
          className="min-h-12 w-full rounded-xl border border-slate-300 px-4 text-base outline-none transition placeholder:text-slate-400 focus:border-violet-700 focus:ring-4 focus:ring-violet-100"
        />
        <p className="mt-2 text-xs leading-5 text-slate-500">O nome precisa ter entre 3 e 100 caracteres.</p>
        {errorMessage && (
          <div role="alert" className="mt-5 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm leading-5 text-rose-800">{errorMessage}</div>
        )}
        <div className="mt-7 flex flex-col-reverse gap-3 sm:flex-row sm:justify-end">
          <button
            type="button"
            onClick={() => navigate("/walletspaces")}
            className="inline-flex min-h-11 items-center justify-center rounded-xl border border-slate-300 px-4 text-sm font-semibold text-slate-700 transition hover:bg-slate-50 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-700"
          >
            Cancelar
          </button>
          <button
            type="submit"
            disabled={createMutation.isPending}
            className="inline-flex min-h-11 items-center justify-center rounded-xl bg-violet-900 px-4 text-sm font-semibold text-white transition hover:bg-violet-800 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-violet-800 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {createMutation.isPending ? "Criando..." : "Criar espaço"}
          </button>
        </div>
      </form>
    </div>
  );
}
