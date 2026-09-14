const members = [
  {
    name: "Sara Rezaei",
    init: "SR",
    role: "Lead developer",
    email: "sara@peaklabs.dev",
    perms: ["Articles", "Analytics"],
    last: "12 min ago",
    status: "online",
  },
  {
    name: "Reza Hosseini",
    init: "RH",
    role: "Lead developer",
    email: "reza@peaklabs.dev",
    perms: ["Articles", "Portfolio"],
    last: "48 min ago",
    status: "online",
  },
  {
    name: "Milad Jafari",
    init: "MJ",
    role: "Editor",
    email: "milad@peaklabs.dev",
    perms: ["Articles"],
    last: "2 hours ago",
    status: "online",
  },
  {
    name: "Alex Morgan",
    init: "AM",
    role: "Editor",
    email: "alex@peaklabs.dev",
    perms: ["Articles"],
    last: "Yesterday",
    status: "offline",
  },
];

function memberRows(arr, id) {
  document.getElementById(id).innerHTML = arr
    .map(
      (m) =>
        `<tr><td><div class="member"><div class="avatar">${m.init}</div><div><div class="item-title">${m.name}</div><div class="role">${m.email}</div></div></div></td><td>${tRole(m.role)}</td><td><div class="permissions">${m.perms.map((p) => `<span class="permission">${p}</span>`).join("")}</div></td><td>${m.last}</td><td><span class="status ${m.status === "online" ? "published" : "archived"}">${t(m.status)}</span></td><td><div class="actions"><button class="action" onclick="openModal('${id === "adminRows" ? "admin" : "developer"}')"><i class="bi bi-pencil"></i></button></div></td></tr>`,
    )
    .join("");
}
function tRole(r) {
  return (
    {
      "Lead developer": t("lead"),
      Editor: t("editor"),
      "Super Admin": t("superadmin"),
      "Content Admin": t("contentAdmin"),
    }[r] || r
  );
}
memberRows(members, "developerRows");
memberRows(admins, "adminRows");