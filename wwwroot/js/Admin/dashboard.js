//Mobile navbar (Dashboard)

const hamburgerBtn = document.getElementById("hamburgerBtn");
const mobilePanel = document.getElementById("sidebar");
hamburgerBtn.addEventListener("click", function () {
  const open = hamburgerBtn.classList.toggle("open");
  mobilePanel.classList.toggle("open");
  hamburgerBtn.setAttribute("aria-expanded", open);
});

const dashboardGreeting = document.getElementById("dashboardGreeting");
if (dashboardGreeting) {
  const firstName = dashboardGreeting.dataset.firstName || "کاربر";
  const hour = new Date().getHours();
  const greeting = hour < 12
    ? "صبح بخیر"
    : hour < 17
      ? "ظهر بخیر"
      : hour < 20
        ? "عصر بخیر"
        : "شب بخیر";

  dashboardGreeting.textContent = `${greeting}، ${firstName}.`;
}
mobilePanel.querySelectorAll("a").forEach(function (a) {
  a.addEventListener("click", function () {
    hamburgerBtn.classList.remove("open");
    mobilePanel.classList.remove("open");
  });
});

//Dashboard Chart

(() => {
  const canvas = document.getElementById("articleViewsChart");
  if (!canvas) return;

  const trends = window.articleViewsTrends || {};
  const trafficTrends = window.siteTrafficTrends || {};
  const initialTrend = trends.monthly || { labels: [], datasets: [] };
  const categorySelect = document.getElementById("articleViewCategory");
  const sourceSelect = document.getElementById("dashboardChartSource");
  const chartTitle = document.getElementById("dashboardChartTitle");
  const buildDatasets = (trend) => {
    const series = trend.datasets || [];
    const selectedCategory = categorySelect?.value || "all";
    const data = selectedCategory === "all"
      ? trend.labels.map((_, index) => series.reduce((total, item) => total + (item.data[index] || 0), 0))
      : (series.find((item) => item.label === selectedCategory)?.data || trend.labels.map(() => 0));
    return {
      label: selectedCategory === "all" ? "همهٔ دسته‌بندی‌ها" : selectedCategory,
      data,
      borderColor: colors.primary,
      backgroundColor: "rgba(16, 185, 129, 0.12)",
      borderWidth: 3,
      tension: 0.35,
      fill: true,
      pointRadius: 4,
      pointHoverRadius: 7,
      pointBorderWidth: 2,
      pointBackgroundColor: colors.primary,
      pointBorderColor: colors.bg,
    };
  };
  const renderChartLabels = (labels) => {
    const container = document.getElementById("articleViewChartLabels");
    if (!container || labels.length === 0) return;
    const indexes = [0, Math.floor((labels.length - 1) / 4), Math.floor((labels.length - 1) / 2), Math.floor(3 * (labels.length - 1) / 4), labels.length - 1];
    container.replaceChildren(...[...new Set(indexes)].map((index) => {
      const label = document.createElement("span");
      label.textContent = labels[index];
      return label;
    }));
  };
  renderChartLabels(initialTrend.labels);

  const getThemeColors = () => {
    const styles = getComputedStyle(document.documentElement);

    return {
      primary: styles.getPropertyValue("--color-primary").trim() || "#10B981",
      border: styles.getPropertyValue("--color-border").trim() || "#e5e7eb",
      text: styles.getPropertyValue("--color-text-muted").trim() || "#6b7280",
      bg: styles.getPropertyValue("--color-bg").trim() || "#ffffff",
    };
  };

  let colors = getThemeColors();

  const chart = new Chart(canvas, {
    type: "line",

    data: {
      labels: initialTrend.labels,

      datasets: [buildDatasets(initialTrend)],
    },

    options: {
      responsive: true,
      maintainAspectRatio: false,
      interaction: {
        mode: "index",
        intersect: false,
      },

      plugins: {
        legend: {
          display: false,
        },

        tooltip: {
          callbacks: {
            label: (context) => {
              return ` ${context.parsed.y.toLocaleString("fa-IR")} بازدید`;
            },
          },
        },
      },

      scales: {
        x: {
          grid: {
            display: false,
          },

          ticks: {
            color: colors.text,
            font: {
              size: 11,
            },
          },

          border: {
            display: false,
          },
        },

        y: {
          beginAtZero: true,

          grid: {
            color: colors.border,
          },

          ticks: {
            color: colors.text,
            font: {
              size: 11,
            },
          },

          border: {
            display: false,
          },
        },
      },
    },
  });

  const periodSelect = document.getElementById("articleViewPeriod");
  const currentTrends = () => sourceSelect?.value === "traffic" ? trafficTrends : trends;
  const updateSourceUi = () => {
    const isTraffic = sourceSelect?.value === "traffic";
    if (categorySelect) categorySelect.hidden = isTraffic;
    if (chartTitle) chartTitle.textContent = isTraffic ? "ترافیک سایت" : "بازدید مقاله‌ها";
  };
  sourceSelect?.addEventListener("change", () => {
    const selected = currentTrends()[periodSelect?.value || "monthly"];
    if (!selected) return;
    updateSourceUi();
    chart.data.labels = selected.labels;
    chart.data.datasets = [buildDatasets(selected)];
    renderChartLabels(selected.labels);
    chart.update();
  });
  periodSelect?.addEventListener("change", () => {
    const selected = currentTrends()[periodSelect.value];
    if (!selected) return;
    chart.data.labels = selected.labels;
    chart.data.datasets = [buildDatasets(selected)];
    renderChartLabels(selected.labels);
    chart.update();
  });
  categorySelect?.addEventListener("change", () => {
    const selected = currentTrends()[periodSelect?.value || "monthly"];
    if (!selected) return;
    chart.data.datasets = [buildDatasets(selected)];
    chart.update();
  });
  updateSourceUi();

  /*
   * اگر Theme عوض شد، رنگ‌های نمودار هم دوباره از CSS Variables خوانده شوند.
   */
  const themeObserver = new MutationObserver(() => {
    colors = getThemeColors();

    chart.data.datasets[0].borderColor = colors.primary;
    chart.data.datasets[0].pointBorderColor = colors.bg;
    chart.data.datasets[0].pointBackgroundColor = colors.primary;

    chart.options.scales.x.ticks.color = colors.text;
    chart.options.scales.y.ticks.color = colors.text;
    chart.options.scales.y.grid.color = colors.border;

    chart.update();
  });

  themeObserver.observe(document.documentElement, {
    attributes: true,
    attributeFilter: ["data-theme"],
  });
})();
