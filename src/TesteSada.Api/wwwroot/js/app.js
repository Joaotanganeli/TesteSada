// Frontend do Teste SADA: JavaScript puro consumindo a API REST em /api/tasks.
const API_URL = "/api/tasks";

const STATUS_LABELS = {
  Pendente: "Pendente",
  EmProgresso: "Em progresso",
  Concluida: "Concluída",
};

const el = {
  form: document.getElementById("task-form"),
  formTitle: document.getElementById("form-title"),
  id: document.getElementById("task-id"),
  title: document.getElementById("title"),
  description: document.getElementById("description"),
  dueDate: document.getElementById("dueDate"),
  status: document.getElementById("status"),
  errors: document.getElementById("form-errors"),
  submitBtn: document.getElementById("submit-btn"),
  cancelBtn: document.getElementById("cancel-btn"),
  filterForm: document.getElementById("filter-form"),
  filterStatus: document.getElementById("filter-status"),
  filterFrom: document.getElementById("filter-from"),
  filterTo: document.getElementById("filter-to"),
  clearFilters: document.getElementById("clear-filters"),
  list: document.getElementById("task-list"),
  listInfo: document.getElementById("list-info"),
  toast: document.getElementById("toast"),
};

// ---------- Camada de acesso à API ----------

/** Faz a requisição e converte erros (ProblemDetails) em exceções com a lista de mensagens. */
async function request(url, options = {}) {
  const response = await fetch(url, {
    headers: { "Content-Type": "application/json" },
    ...options,
  });

  if (response.status === 204) return null;

  const body = await response.json().catch(() => null);

  if (!response.ok) {
    const error = new Error(body?.title ?? `Erro HTTP ${response.status}`);
    error.messages = body?.errors
      ? Object.values(body.errors).flat()
      : [body?.detail ?? error.message];
    throw error;
  }

  return body;
}

const api = {
  list: (filters) => {
    const params = new URLSearchParams();
    Object.entries(filters).forEach(([key, value]) => value && params.append(key, value));
    const query = params.toString();
    return request(query ? `${API_URL}?${query}` : API_URL);
  },
  create: (task) => request(API_URL, { method: "POST", body: JSON.stringify(task) }),
  update: (id, task) => request(`${API_URL}/${id}`, { method: "PUT", body: JSON.stringify(task) }),
  remove: (id) => request(`${API_URL}/${id}`, { method: "DELETE" }),
  get: (id) => request(`${API_URL}/${id}`),
};

// ---------- Utilidades de UI ----------

let toastTimer;
function showToast(message, isError = false) {
  el.toast.textContent = message;
  el.toast.className = isError ? "toast error" : "toast";
  el.toast.hidden = false;
  clearTimeout(toastTimer);
  toastTimer = setTimeout(() => (el.toast.hidden = true), 3500);
}

function showFormErrors(messages) {
  el.errors.innerHTML = "";
  messages.forEach((msg) => {
    const li = document.createElement("li");
    li.textContent = msg;
    el.errors.appendChild(li);
  });
  el.errors.hidden = messages.length === 0;
}

function formatDate(isoDate) {
  if (!isoDate) return "Sem vencimento";
  const [year, month, day] = isoDate.split("-");
  return `${day}/${month}/${year}`;
}

function todayIso() {
  const now = new Date();
  now.setMinutes(now.getMinutes() - now.getTimezoneOffset());
  return now.toISOString().slice(0, 10);
}

// ---------- Renderização ----------

function renderTasks(tasks) {
  el.list.innerHTML = "";
  el.listInfo.textContent = tasks.length
    ? `${tasks.length} tarefa(s) encontrada(s).`
    : "Nenhuma tarefa encontrada.";

  const today = todayIso();

  tasks.forEach((task) => {
    const li = document.createElement("li");
    li.className = `task ${task.status}`;

    const isOverdue = task.dueDate && task.dueDate < today && task.status !== "Concluida";

    // textContent (e não innerHTML) para os dados do usuário evita XSS.
    const info = document.createElement("div");
    const title = document.createElement("h3");
    title.className = "task-title";
    title.textContent = task.title;

    const badge = document.createElement("span");
    badge.className = `badge ${task.status}`;
    badge.textContent = STATUS_LABELS[task.status] ?? task.status;

    info.append(title, badge);

    if (task.description) {
      const desc = document.createElement("p");
      desc.className = "task-desc";
      desc.textContent = task.description;
      info.append(desc);
    }

    const meta = document.createElement("div");
    meta.className = "task-meta";
    const due = document.createElement("span");
    due.textContent = `Vencimento: ${formatDate(task.dueDate)}${isOverdue ? " (atrasada)" : ""}`;
    if (isOverdue) due.classList.add("overdue");
    const code = document.createElement("span");
    code.innerHTML = "Código: <code></code>";
    code.querySelector("code").textContent = task.id;
    meta.append(due, code);
    info.append(meta);

    const buttons = document.createElement("div");
    buttons.className = "task-buttons";
    const editBtn = document.createElement("button");
    editBtn.className = "btn small";
    editBtn.textContent = "Editar";
    editBtn.addEventListener("click", () => startEdit(task.id));
    const deleteBtn = document.createElement("button");
    deleteBtn.className = "btn small danger";
    deleteBtn.textContent = "Excluir";
    deleteBtn.addEventListener("click", () => deleteTask(task));
    buttons.append(editBtn, deleteBtn);

    li.append(info, buttons);
    el.list.appendChild(li);
  });
}

// ---------- Ações ----------

function currentFilters() {
  return {
    status: el.filterStatus.value,
    dueDateFrom: el.filterFrom.value,
    dueDateTo: el.filterTo.value,
  };
}

async function loadTasks() {
  try {
    renderTasks(await api.list(currentFilters()));
  } catch (err) {
    showToast(err.messages?.join(" ") ?? err.message, true);
  }
}

function readForm() {
  return {
    title: el.title.value,
    description: el.description.value || null,
    dueDate: el.dueDate.value || null,
    status: el.status.value,
  };
}

function resetForm() {
  el.form.reset();
  el.id.value = "";
  el.formTitle.textContent = "Nova tarefa";
  el.submitBtn.textContent = "Criar tarefa";
  el.cancelBtn.hidden = true;
  showFormErrors([]);
}

async function startEdit(id) {
  try {
    const task = await api.get(id);
    el.id.value = task.id;
    el.title.value = task.title;
    el.description.value = task.description ?? "";
    el.dueDate.value = task.dueDate ?? "";
    el.status.value = task.status;
    el.formTitle.textContent = "Editar tarefa";
    el.submitBtn.textContent = "Salvar alterações";
    el.cancelBtn.hidden = false;
    showFormErrors([]);
    el.title.focus();
  } catch (err) {
    showToast(err.messages?.join(" ") ?? err.message, true);
    loadTasks();
  }
}

async function deleteTask(task) {
  if (!confirm(`Excluir a tarefa "${task.title}"?`)) return;
  try {
    await api.remove(task.id);
    if (el.id.value === task.id) resetForm();
    showToast("Tarefa excluída.");
    loadTasks();
  } catch (err) {
    showToast(err.messages?.join(" ") ?? err.message, true);
  }
}

el.form.addEventListener("submit", async (event) => {
  event.preventDefault();
  const id = el.id.value;
  try {
    if (id) {
      await api.update(id, readForm());
      showToast("Tarefa atualizada.");
    } else {
      const created = await api.create(readForm());
      showToast(`Tarefa criada! Código: ${created.id}`);
    }
    resetForm();
    loadTasks();
  } catch (err) {
    showFormErrors(err.messages ?? [err.message]);
  }
});

el.cancelBtn.addEventListener("click", resetForm);

el.filterForm.addEventListener("submit", (event) => {
  event.preventDefault();
  loadTasks();
});

el.clearFilters.addEventListener("click", () => {
  el.filterForm.reset();
  loadTasks();
});

loadTasks();
