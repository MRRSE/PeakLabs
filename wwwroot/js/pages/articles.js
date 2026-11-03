(() => {
  const searchInput = document.getElementById("articlePublicSearch");
  const cards = [...document.querySelectorAll("[data-article-card]")];
  const categoryButtons = [...document.querySelectorAll(".filter-pills [data-category-id]")];
  const count = document.getElementById("articleResultsCount");
  const empty = document.getElementById("articleListEmpty");
  const requestedCategory = new URLSearchParams(window.location.search).get("categoryId") || "";
  let activeCategory = categoryButtons.some((button) => button.dataset.categoryId === requestedCategory)
    ? requestedCategory
    : "";

  function filterArticles() {
    const query = (searchInput?.value || "").trim().toLocaleLowerCase();
    let visible = 0;

    cards.forEach((card) => {
      const matchesQuery = !query || (card.dataset.search || "").toLocaleLowerCase().includes(query);
      const matchesCategory = !activeCategory || card.dataset.categoryId === activeCategory;
      card.hidden = !(matchesQuery && matchesCategory);
      if (!card.hidden) visible++;
    });

    if (count) count.textContent = `${visible.toLocaleString("fa-IR")} مقاله`;
    if (empty) {
      empty.hidden = visible > 0;
      if (cards.length > 0) empty.textContent = "مقاله‌ای با این جستجو پیدا نشد.";
    }
  }

  categoryButtons.forEach((button) => {
    button.classList.toggle("active", button.dataset.categoryId === activeCategory);
  });

  searchInput?.addEventListener("input", filterArticles);
  categoryButtons.forEach((button) => {
    button.addEventListener("click", () => {
      const categoryId = button.dataset.categoryId || "";
      const url = new URL(window.location.href);

      if (categoryId) {
        url.searchParams.set("categoryId", categoryId);
      } else {
        url.searchParams.delete("categoryId");
      }

      window.location.assign(url);
    });
  });

  filterArticles();
})();
