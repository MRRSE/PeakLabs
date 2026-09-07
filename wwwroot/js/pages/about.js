const stack = [
  "ASP.NET Core",
  "C#",
  "PostgreSQL",
  "Docker",
  "Kubernetes",
  "Azure",
  "React",
  "Redis",
];
function renderStack() {
  document.getElementById("stackRow").innerHTML = stack
    .map(function (s) {
      return (
        '<div class="stack-pill"><i class="bi bi-check-circle-fill"></i> ' +
        s +
        "</div>"
      );
    })
    .join("");
}
renderStack();